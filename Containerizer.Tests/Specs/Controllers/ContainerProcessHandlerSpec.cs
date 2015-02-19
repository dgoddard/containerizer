#region

using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Containerizer.Controllers;
using Containerizer.Facades;
using Containerizer.Services.Interfaces;
using Containerizer.Tests.Specs.Facades;
using Moq;
using NSpec;
using IronFoundry.Container;
using System.Collections.Generic;

#endregion

namespace Containerizer.Tests.Specs.Controllers
{
    internal class ContainerProcessHandlerSpec : nspec
    {
        private string containerId;
        private byte[] fakeStandardInput;
        private ContainerProcessHandler handler;
        Mock<IContainerService> mockContainerService = null;
        Mock<IContainer> mockContainer = null;
        private int expectedHostPort = 6336;
        private Mock<IContainerDirectory> mockContainerDirectory;
        private IronFoundry.Container.ProcessSpec processSpec;
        private IronFoundry.Container.IProcessIO processIO;
        private Mock<IronFoundry.Container.Utilities.IProcess> mockProcess;
        private Mock<IronFoundry.Container.IContainerProcess> mockContainerProcess;


        private void before_each()
        {

            mockContainerService = new Mock<IContainerService>();
            mockContainerDirectory = new Mock<IContainerDirectory>();

            containerId = new Guid().ToString();

            mockContainer = new Mock<IContainer>();
            mockContainerService.Setup(x => x.GetContainerByHandle(containerId)).Returns(mockContainer.Object);
            mockContainer.Setup(x => x.GetInfo()).Returns(
                new ContainerInfo
                {
                    ReservedPorts = new List<int> { expectedHostPort },
                });


            mockContainer.Setup(x => x.Directory).Returns(mockContainerDirectory.Object);
            mockContainerDirectory.Setup(x => x.MapUserPath("")).Returns(@"C:\A\Directory\user");

            mockProcess = new Mock<IronFoundry.Container.Utilities.IProcess>();
            mockContainerProcess = new Mock<IronFoundry.Container.IContainerProcess>();
            //mockContainerProcess.Setup(x => x.process).Returns(mockProcess.Object);
            mockContainer.Setup(x => x.Run(It.IsAny<IronFoundry.Container.ProcessSpec>(), It.IsAny<IronFoundry.Container.IProcessIO>()))
                .Callback<IronFoundry.Container.ProcessSpec, IronFoundry.Container.IProcessIO>((processSpec, processIO) =>
                {
                    this.processSpec = processSpec;
                    this.processIO = processIO;
                })
                .Returns(mockContainerProcess.Object);

            handler = new ContainerProcessHandler(containerId, mockContainerService.Object);
        }

        private void SendProcessOutputEvent(string message)
        {
            processIO.StandardOutput.WriteLine(message);
        }

        private void SendProcessErrorEvent(string message)
        {
            processIO.StandardError.WriteLine(message);
        }

        private void SendProcessExitEvent()
        {
            mockProcess.Raise(mock => mock.Exited += null, (EventArgs)null);
        }

        private string WaitForWebSocketMessage(FakeWebSocket websocket)
        {
            Thread.Sleep(100);
            if (websocket.LastSentBuffer.Array == null)
            {
                return "no message sent (test)";
            }
            byte[] byteArray = websocket.LastSentBuffer.Array;
            return Encoding.Default.GetString(byteArray);
        }

        private void describe_onmessage()
        {
            FakeWebSocket websocket = null;

            before = () =>
            {
                handler.WebSocketContext = new FakeAspNetWebSocketContext();
                websocket = (FakeWebSocket)handler.WebSocketContext.WebSocket;
            };

            act =
                () =>
                    handler.OnMessage(
                        "{\"type\":\"run\", \"pspec\":{\"Path\":\"foo.exe\", \"Args\":[\"some\", \"args\"]}}");

            it["sets working directory"] = () =>
            {
                processSpec.WorkingDirectory.should_be("C:\\A\\Directory\\user");
            };


            it["sets start info correctly"] = () =>
            {
                processSpec.ExecutablePath.should_be("foo.exe");
                processSpec.Arguments.should_be(new List<string> { "some", "args" });
            };

            it["sets PORT on the environment variable"] = () =>
            {
                processSpec.Environment.ContainsKey("PORT").should_be_true();
                processSpec.Environment["PORT"].should_be("6336");
            };



            context["when a port has not been reserved"] = () =>
            {
                before = () =>
                {
                    mockContainer.Setup(x => x.GetInfo()).Returns(
                        new ContainerInfo
                        {
                            ReservedPorts = new List<int>(),
                        });
                };

                it["does not set PORT env variable"] = () =>
                {
                    processSpec.Environment.ContainsKey("PORT").should_be_false();
                };
            };

            it["runs something"] = () =>
            {
                mockContainer.Verify(x => x.Run(It.IsAny<IronFoundry.Container.ProcessSpec>(), It.IsAny<IronFoundry.Container.IProcessIO>()));
            };


            context["when process.start raises an error"] = () =>
            {
                before = () =>
                    mockContainer.Setup(mock => mock.Run(It.IsAny<IronFoundry.Container.ProcessSpec>(), It.IsAny<IronFoundry.Container.IProcessIO>()))
                        .Throws(new Exception("An Error Message"));

                it["sends the error over the socket"] = () =>
                {
                    string message = WaitForWebSocketMessage(websocket);
                    message.should_be("{\"type\":\"error\",\"data\":\"An Error Message\"}");
                };
            };

            xdescribe["standard in"] = () => { };

            describe["standard out"] = () =>
                    {
                        it["sends over socket"] = () =>
                        {
                            SendProcessOutputEvent("Hi");

                            string message = WaitForWebSocketMessage(websocket);
                            message.should_be("{\"type\":\"stdout\",\"data\":\"Hi\\r\\n\"}");
                        };
                    };

            describe["standard error"] = () =>
            {
                it["sends over socket"] = () =>
                {
                    SendProcessErrorEvent("Hi");

                    string message = WaitForWebSocketMessage(websocket);
                    message.should_be("{\"type\":\"stderr\",\"data\":\"Hi\\r\\n\"}");
                };
            };

            describe["once the process exits"] = () =>
            {
                it["sends close event over socket"] = () =>
                {
                    SendProcessExitEvent();

                    string message = WaitForWebSocketMessage(websocket);
                    message.should_be("{\"type\":\"close\"}");
                };
            };
        }
    }
}