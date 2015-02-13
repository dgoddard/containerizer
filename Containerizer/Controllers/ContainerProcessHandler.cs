#region

using System;
using System.Diagnostics;
using Containerizer.Facades;
using Containerizer.Models;
using Containerizer.Services.Interfaces;
using Microsoft.Web.WebSockets;
using Newtonsoft.Json;
using IronFoundry.Container;
using System.IO;
using System.Text;
using System.Collections.Generic;

#endregion

namespace Containerizer.Controllers
{

    internal class WSTextWriter : TextWriter
    {
        private WebSocketHandler ws;
        private string name;

        public WSTextWriter(WebSocketHandler ws, string name)
        {
            this.ws = ws;
        }

        public override void Write(string value)
        {
            string data = JsonConvert.SerializeObject(new ProcessStreamEvent
            {
                MessageType = this.name,
                Data = value
            }, Formatting.None);
            ws.Send(data);
        }

        public override void Write(char value)
        {
            Write(value.ToString());
        }

        public override Encoding Encoding
        {
            get { return Encoding.Default; }
        }
    }
    internal class StringProcessIO : IProcessIO
    {
        public WSTextWriter Error;
        public WSTextWriter Output;

        public StringProcessIO(WebSocketHandler ws)
        {
            Output = new WSTextWriter(ws, "stdout");
            Error = new WSTextWriter(ws, "stderr");
        }

        public TextWriter StandardOutput
        {
            get { return Output; }
        }

        public TextWriter StandardError
        {
            get { return Error; }
        }

        public TextReader StandardInput { get; set; }
    }

    public class ContainerProcessHandler : WebSocketHandler
    {
        private readonly string containerRoot;
        private readonly IProcessFacade process;
        private readonly IContainer container;

        public ContainerProcessHandler(string containerId, IContainerService containerService, IProcessFacade process)
        {
            containerRoot = containerService.GetContainerByHandle(containerId).Directory.MapUserPath("");
            this.container = containerService.GetContainerByHandle(containerId);
            this.process = process;
        }

        public override void OnMessage(string message)
        {
            var streamEvent = JsonConvert.DeserializeObject<ProcessStreamEvent>(message);
            var processIO = new StringProcessIO(this);

            if (streamEvent.MessageType == "run" && streamEvent.ApiProcessSpec != null)
            {
                ApiProcessSpec pSpec = streamEvent.ApiProcessSpec;
                var processSpec = new IronFoundry.Container.ProcessSpec
                {
                    ExecutablePath = containerRoot + '\\' + pSpec.Path,
                    Arguments = pSpec.Args,
                    WorkingDirectory = containerRoot,
                    Environment = new Dictionary<string, string>(),
                };
            
                var reservedPorts = container.GetInfo().ReservedPorts;
                if (reservedPorts.Count > 0)
                    processSpec.Environment["PORT"] = reservedPorts[0].ToString();
                
                try
                {
                    container.Run(processSpec, processIO);
                }
                catch (Exception e)
                {
                    SendEvent("error", e.Message);
                    return;
                }
            }
            else if (streamEvent.MessageType == "stdin")
            {
                // process.StandardInput.Write(streamEvent.Data);
            }
        }

        private void SendEvent(string messageType, string message)
        {
            string data = JsonConvert.SerializeObject(new ProcessStreamEvent
            {
                MessageType = messageType,
                Data = message
            }, Formatting.None);
            Send(data);
        }

        /*
        private void OutputDataHandler(object sendingProcess, DataReceivedEventArgs outLine)
        {
            if (outLine.Data == null) return;
            SendEvent("stdout", outLine.Data + "\r\n");
        }

        private void OutputErrorDataHandler(object sendingProcess, DataReceivedEventArgs outLine)
        {
            if (outLine.Data == null) return;
            SendEvent("stderr", outLine.Data + "\r\n");
        }

        private void ProcessExitedHandler(object sendingProcess, EventArgs e)
        {
            SendEvent("close", null);
        }
        */
    }
}