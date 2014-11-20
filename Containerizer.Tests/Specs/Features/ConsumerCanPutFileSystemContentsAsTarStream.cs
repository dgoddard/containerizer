using System;
using System.Collections.Generic;
using System.Text;
using Containerizer.Services.Implementations;
using NSpec;
using System.Linq;
using System.Web.Http.Results;
using System.IO;
using Microsoft.Web.Administration;
using System.Net.Http;
using System.Collections.Specialized;
using Newtonsoft.Json.Linq;
using SharpCompress.Reader;

namespace Containerizer.Tests
{
    class ConsumerCanPutFileSystemContentsAsTarStream : nspec
    {
        string id;
        HttpClient client;
        private string containerPath;
        private int port;

        void before_each()
        {
            port = 8088;
            Helpers.SetupSiteInIIS("Containerizer", "Containerizer.Tests", "ContainerizerTestsApplicationPool", port, true);
            File.WriteAllText("file.tgz", "stuff!!!!");
        }

        void after_each()
        {
            Helpers.RemoveExistingSite("Containerizer.Tests", "ContainerizerTestsApplicationPool");
            Helpers.RemoveExistingSite(id, id);
            Directory.Delete(containerPath, true);
        }

        void describe_stream_in()
        {
            context["given that I'm a consumer of the containerizer api"] = () =>
            {
                HttpClient client = null;

                before = () =>
                {
                    client = new HttpClient();
                    client.BaseAddress = new Uri("http://localhost.:" + port.ToString());
                };

                context["there exists a container with a given id"] = () =>
                {
                    before = () =>
                    {
                        id = Helpers.CreateContainer(port);
                    };

                    context["when I PUT a request to /api/Containers/:id/files?destination=file.txt"] = () =>
                    {
                        HttpResponseMessage responseMessage = null;
                        before = () =>
                        {
                            var content = new MultipartFormDataContent();
                            var fileStream = new FileStream("file.tgz", FileMode.Open);
                            var stringContent = new StringContent("FirstName=MUH&LastName=Test", Encoding.UTF8, "multipart/form-data");
                            content.Add(stringContent);
                            var streamContent = new StreamContent(fileStream);
                            content.Add(streamContent);
                            var path = "/api/containers/" + id + "/files?destination=file.txt";
                            responseMessage = client.PutAsync(path, streamContent).GetAwaiter().GetResult();
                            var x = 1;
                        };

                        it["returns a successful status code"] = () =>
                        {
                            responseMessage.IsSuccessStatusCode.should_be_true();
                        };

                        it["sees the new file in the container"] = () =>
                        {
                            var fileContent = File.ReadAllText(Path.Combine(new ContainerPathService().GetContainerRoot(id), "file.txt"));
                            fileContent.should_be("stuff!!!!");
                        };
                    };
                };
            };
        }
    }
}


