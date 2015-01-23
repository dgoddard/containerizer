﻿#region

using System;
using System.IO;
using Containerizer.Services.Implementations;
using Containerizer.Services.Interfaces;
using Moq;
using NSpec;

#endregion

namespace Containerizer.Tests.Specs.Services
{
    internal class StreamOutServiceSpec : nspec
    {
        private string actualPath;
        private Stream expectedStream;
        private string id;
        private Mock<IContainerPathService> mockIContainerPathService;
        private Mock<ITarStreamService> mockITarStreamService;
        private StreamOutService streamOutService;

        private void before_each()
        {
            mockIContainerPathService = new Mock<IContainerPathService>();
            mockITarStreamService = new Mock<ITarStreamService>();
            streamOutService = new StreamOutService(mockIContainerPathService.Object, mockITarStreamService.Object);
            id = Guid.NewGuid().ToString();
        }

        private void describe_stream_out()
        {
            Stream stream = null;

            before = () =>
            {
                mockIContainerPathService.Setup(x => x.GetSubdirectory(It.IsAny<string>(), "/file.txt"))
                    .Returns(() => @"C:\a\path\file.txt");
                mockITarStreamService.Setup(x => x.WriteTarToStream(It.IsAny<string>()))
                    .Returns((string path) =>
                    {
                        expectedStream = new MemoryStream();
                        actualPath = path;
                        return expectedStream;
                    });
                stream = streamOutService.StreamOutFile(id, "/file.txt");
            };

            it["returns a stream from the tarstreamer"] = () =>
            {
                stream.should_be_same(expectedStream);
            };

            it["passes the path combined with the id to tarstreamer"] =
                () =>
                {
                    actualPath.should_be(@"C:\a\path\file.txt");
                };
        }
    }
}