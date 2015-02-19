#region

using System;
using System.Collections.Generic;
using System.IO;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using Containerizer.Services.Implementations;
using NSpec;
using System.Net.Http;
using Newtonsoft.Json.Linq;
using System.Collections;

#endregion

namespace Containerizer.Tests.Specs.Features
{
    internal class DevCanRunProcessSpec : nspec
    {
        public class TestProcessIO : IronFoundry.Container.IProcessIO
        {
            public StringWriter Output = new StringWriter();
            public StringWriter Error = new StringWriter();

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

        private void describe_consumer_can_run_a_process()
        {
            IronFoundry.Container.ContainerService containerService = null;
            IronFoundry.Container.IContainer container = null;
            before = () =>
            {
                containerService = new IronFoundry.Container.ContainerService(@"C:\Containers", "Users");
                container = containerService.CreateContainer(new IronFoundry.Container.ContainerSpec
                {
                    Handle = Guid.NewGuid().ToString()
                });
            };
            after = () => containerService.DestroyContainer(container.Handle);

            context["I run stuff directly"] = () =>
            {
                it["works"] = () =>
                {
                    var containerRoot = container.Directory.MapUserPath(".");
                    File.WriteAllBytes(containerRoot + "/myfile.bat",
                        new UTF8Encoding(true).GetBytes(
                            "@echo off\r\n@echo Hi Fred\r\n@echo Jane is good\r\n@echo Jill is better\r\nset /p str=\"A Cool Prompt: \"\r\necho %str%\r\n"));
                    var io = new TestProcessIO();
                    io.StandardInput = new StringReader("Some exciting input\r\n");

                    IronFoundry.Container.ContainerProcess icp = (IronFoundry.Container.ContainerProcess)container.Run(new IronFoundry.Container.ProcessSpec
                    {
                        DisablePathMapping = false,
                        Privileged = false,
                        WorkingDirectory = containerRoot,
                        ExecutablePath = "/myfile.bat",
                        Environment = new Dictionary<string, string>(),
                        Arguments = new string[0],
                    }, io);
                    /*
                    icp.process.Exited += (object sendingProcess, EventArgs e) =>
                    {
                        Console.WriteLine(e.ToString());
                    };
                    */
                    var ret = icp.WaitForExit();
                    io.StandardOutput.ToString().should_contain("Hi Fred");
                    io.StandardOutput.ToString().should_contain("Jill is better");

                    io.StandardOutput.ToString().should_contain("Some exciting input");
                };
            };
        }
    }
}
