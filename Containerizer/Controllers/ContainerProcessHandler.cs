#region

using System;
using System.Diagnostics;
using Containerizer.Facades;
using Containerizer.Models;
using Containerizer.Services.Interfaces;
using Microsoft.Web.WebSockets;
using Newtonsoft.Json;
using IronFoundry.Container;
using System.Collections.Generic;
using System.IO;
using System.Text;

#endregion

namespace Containerizer.Controllers
{
    public interface IWebSocketEventSender
    {
        void SendEvent(string messageType, string message);
    }

    public class ContainerProcessHandler : WebSocketHandler, IWebSocketEventSender
    {
        public class WSWriter : TextWriter
        {
            private string streamName;
            private IWebSocketEventSender ws;
            public WSWriter(string streamName, IWebSocketEventSender ws)
            {
                this.streamName = streamName;
                this.ws = ws;
            }


            public override void WriteLine(string value)
            {
                Write(value + "\r\n");
            }

            public override void Write(string value)
            {
                ws.SendEvent(streamName, value);
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

        public class ProcessIO : IronFoundry.Container.IProcessIO
        {
            public TextWriter StandardOutput { get; set; }
            public TextWriter StandardError { get; set; }
            public TextReader StandardInput { get; set; }

            public ProcessIO(IWebSocketEventSender ws)
            {
                StandardOutput = new WSWriter("stdout", ws);
                StandardError = new WSWriter("stderr", ws);
            }
        }

        private readonly string containerRoot;
        private readonly IContainer container;

        public ContainerProcessHandler(string containerId, IContainerService containerService)
        {
            containerRoot = containerService.GetContainerByHandle(containerId).Directory.MapUserPath("");
            this.container = containerService.GetContainerByHandle(containerId);
        }

        public override void OnMessage(string message)
        {
            var streamEvent = JsonConvert.DeserializeObject<ProcessStreamEvent>(message);

            if (streamEvent.MessageType == "run" && streamEvent.ApiProcessSpec != null)
            {
                var processSpec = new IronFoundry.Container.ProcessSpec { 
                    DisablePathMapping = false,
                    Privileged = false,
                    WorkingDirectory = containerRoot,
                    ExecutablePath = streamEvent.ApiProcessSpec.Path,
                    Environment = new Dictionary<string, string>(),
                    Arguments = streamEvent.ApiProcessSpec.Args,
                };
                var reservedPorts = container.GetInfo().ReservedPorts;
                if (reservedPorts.Count > 0)
                    processSpec.Environment["PORT"] = reservedPorts[0].ToString();
                
                try
                {
                    container.Run(processSpec, new ProcessIO(this));
                }
                catch (Exception e)
                {
                    SendEvent("error", e.Message);
                    return;
                }
            }
        }

        public void SendEvent(string messageType, string message)
        {
            string data = JsonConvert.SerializeObject(new ProcessStreamEvent
            {
                MessageType = messageType,
                Data = message
            }, Formatting.None);
            Send(data);
        }

        private void ProcessExitedHandler(object sendingProcess, EventArgs e)
        {
            SendEvent("close", null);
        }
    }
}