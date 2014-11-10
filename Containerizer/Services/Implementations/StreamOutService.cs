using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Xml.Linq;
using Containerizer.Services.Interfaces;
using tar_cs;

namespace Containerizer.Services.Implementations
{
    public class StreamOutService : IStreamOutService
    {
        private readonly IContainerPathService containerPathService;

        public StreamOutService(IContainerPathService containerPathService)
        {
            this.containerPathService = containerPathService;
        }

        public System.IO.Stream StreamFile(string id, string source)
        {
            StreamReader reader;

            var rootDir = containerPathService.GetContainerRoot(id);
            var path = Path.Combine(rootDir, source);

            var stream = new MemoryStream();
            var tarWriter = new TarWriter(stream);
            tarWriter.WriteDirectory(path, true);
            return stream;
        }
    }
}