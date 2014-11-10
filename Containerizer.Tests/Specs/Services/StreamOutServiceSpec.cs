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
    class StreamOutServiceSpec : nspec
    {
        StreamOutService streamOutService;
        ServerManager serverManager;
        private string id;
        private string path;
        private string content;
        private IContainerPathService containerPathService;

        void before_each()
        {
            containerPathService = new ContainerPathService();
            streamOutService = new StreamOutService(containerPathService);
            id = Guid.NewGuid().ToString();
            path = Path.Combine(containerPathService.GetContainerRoot(id), "file.txt");
            content = Guid.NewGuid().ToString();
            containerPathService.CreateContainerDirectory(id);
            File.WriteAllText(path, content);
        }

        void describe_stream_out()
        {
            System.IO.Stream stream = null;
            
            before = () =>
            {
                stream = streamOutService.StreamFile(id, "file.txt");
            };

            it["streams out the contents of a file"] = () =>
            {
                var reader = new StreamReader(stream);
                reader.ReadToEnd().should_be(content);
                reader.Close();
            };

        }

        private void after_each()
        {
            File.Delete(path);
        }

    }
}


