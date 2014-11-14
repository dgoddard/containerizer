using System;
using System.Collections.Generic;
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

        void before_each()
        {
            var port = 8088;
            Helpers.SetupSiteInIIS("Containerizer", "Containerizer.Tests", "ContainerizerTestsApplicationPool", port, true);

            client = new HttpClient();
            client.BaseAddress = new Uri("http://localhost:" + port.ToString());
            var postTask = client.PostAsync("/api/Containers", new FormUrlEncodedContent(new List<KeyValuePair<string, string>>()));
            postTask.Wait();
            var postResult = postTask.Result;
            var readTask = postResult.Content.ReadAsStringAsync();
            readTask.Wait();
            var response = readTask.Result;
            var json = JObject.Parse(response);
            id = json["id"].ToString();
            containerPath = new ContainerPathService().GetContainerRoot(id);
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
            context["when putting a file"] = () =>
            {
                HttpResponseMessage response = null;

                before = () =>
                {
                    var content = new MultipartFormDataContent();
                    var fileStream = new FileStream("file.tgz", FileMode.Open);
                    var streamContent = new StreamContent(fileStream);
                    content.Add(streamContent);
                    var path = "/api/containers/" + id + "/files?destination=file.txt";
                    response = client.PutAsync(path, streamContent).GetAwaiter().GetResult();
                };

                context["when the file doesn't exist"] = () =>
                {
                    it["creates the file"] = () =>
                    {
                        response.IsSuccessStatusCode.should_be_true();
                        var fileContent = File.ReadAllText(Path.Combine(new ContainerPathService().GetContainerRoot(id), "file.txt"));
                        fileContent.should_be("stuff!!!!");

                    };
                };

                context["when the file already exists"] = () =>
                {
                    xit["overwrites the existing file"] = () =>
                    {
                    };
                };

            };
        }
    }
}


