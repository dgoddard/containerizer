using System;
using System.Collections.Generic;
using NSpec;
using System.Linq;
using System.Web.Http.Results;
using System.Net.Http;
using System.Threading;
using Newtonsoft.Json.Linq;
using Containerizer.Services.Interfaces;
using Containerizer.Services.Implementations;
using Moq;
using Microsoft.Web.Administration;
using System.IO;

namespace Containerizer.Tests
{
    class StreamInServiceSpec : nspec
    {
        StreamInService streamInService;
        private string id;
        private string actualPath;
        private Mock<IContainerPathService> mockIContainerPathService;
        private Mock<ITarStreamService> mockITarStreamService;
        private System.IO.Stream expectedStream;

        void before_each()
        {
            mockIContainerPathService = new Mock<IContainerPathService>();
            mockITarStreamService = new Mock<ITarStreamService>();
            streamInService = new StreamInService(mockIContainerPathService.Object, mockITarStreamService.Object);
            id = Guid.NewGuid().ToString();
        }

        void describe_stream_in()
        {
            System.IO.Stream stream = null;
            
            before = () =>
            {
                mockIContainerPathService.Setup(x => x.GetContainerRoot(It.IsAny<string>()))
                    .Returns(() =>  @"C:\a\path" );
                stream = new MemoryStream();
                streamInService.StreamInFile(stream, id, "file.txt");
            };

            it["passes through its stream and combined path to tarstreamer"] = () =>
            {
                mockITarStreamService.Verify(x => x.WriteTarStreamToPath(
                    It.Is((Stream y) => stream.Equals(y)),
                    It.Is((string p) => p.Equals(Path.Combine(@"C:\a\path", "file.txt")))
                    ));

            };
        }
    }
}


