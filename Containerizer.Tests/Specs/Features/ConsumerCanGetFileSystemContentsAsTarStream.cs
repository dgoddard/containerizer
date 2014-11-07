using System;
using System.Collections.Generic;
using NSpec;
using System.Linq;
using System.Web.Http.Results;
using System.IO;
using Microsoft.Web.Administration;
using System.Net.Http;
using System.Collections.Specialized;
using Newtonsoft.Json.Linq;
using tar_cs;

namespace Containerizer.Tests
{
    class ConsumerCanGetFIleSystemContentsAsTarStream : nspec
    {
        string id;
        HttpClient client;

        void before_each()
        {
            var port = 8088;
            Helpers.SetupSiteInIIS("Containerizer", "Containerizer.Tests", "ContainerizerTestsApplicationPool", port);

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
        }

        void after_each()
        {
            Helpers.RemoveExistingSite("Containerizer.Tests", "ContainerizerTestsApplicationPool");
            Helpers.RemoveExistingSite(id, id);
        }

        void describe_stream_out()
        {
            context["when asking for a file"] = () =>
            {
                it["streams the file as a tarball"] = () =>
                {
                    var getTask = client.GetAsync("/api/containers/" + id + "/files?source=fred.txt").GetAwaiter().GetResult();
                    getTask.IsSuccessStatusCode.should_be_true();
                    var stream = getTask.Content.ReadAsStreamAsync().GetAwaiter().GetResult();
                    var tar = new TarReader(stream);
                    tar.FileInfo.FileName.should_be("fred.txt");
                };
            };

            context["when asking for a directory"] = () =>
            {
            };

            context["when tar fails"] = () =>
            {
            };
        }
    }
}


