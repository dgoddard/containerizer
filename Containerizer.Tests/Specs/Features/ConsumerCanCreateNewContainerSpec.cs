﻿#region

using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Web.Administration;
using Newtonsoft.Json.Linq;
using NSpec;
using System.Net.Http.Headers;
using System.Text;

#endregion

namespace Containerizer.Tests.Specs.Features
{
    internal class ConsumerCanCreateNewContainerSpec : nspec
    {
        // Containerizer.Controllers.ContainersController containersController;
        private string id;
        private int port;

        private void before_each()
        {
            port = 8088;
            Helpers.SetupSiteInIIS("Containerizer", "Containerizer.Tests", "ContainerizerTestsApplicationPool", port,
                true);
        }

        private void after_each()
        {
            Helpers.RemoveExistingSite("Containerizer.Tests", "ContainerizerTestsApplicationPool");
            Helpers.RemoveExistingSite(id, id);
        }

        private void describe_consumer_can_create_new_container()
        {
            HttpClient client = null;

            context["given that I am a consumer of the api"] = () =>
            {
                before = () =>
                {
                    client = new HttpClient {BaseAddress = new Uri("http://localhost:" + port)};
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                };

                context["when I post a request"] = () =>
                {
                    string handle = null;
                    string propertyKey = "awesome";
                    string propertyValue = "sauce";

                    before = () =>
                    {
                        handle = Guid.NewGuid() + "-" + Guid.NewGuid();
                        Task<HttpResponseMessage> postTask = client.PostAsync("/api/Containers",
                            new StringContent(
                                "{\"Handle\": \"" + handle + "\", \"Properties\":{\"" + propertyKey + "\":\"" +
                                propertyValue + "\"}}", Encoding.UTF8, "application/json"));
                        postTask.Wait();
                        HttpResponseMessage postResult = postTask.Result;
                        Task<string> readTask = postResult.Content.ReadAsStringAsync();
                        readTask.Wait();
                        string response = readTask.Result;
                        JObject json = JObject.Parse(response);
                        id = json["id"].ToString();
                    };

                    it["creates the container"] = () =>
                    {
                        id.should_be(handle);

                        var listResponse = client.GetAsync("/api/Containers").Result.Content.ReadAsJsonArray();
                        listResponse.Values<string>().Contains(handle).should_be_true();

                        var propertyResponse =
                            client.GetAsync("/api/Containers/" + handle + "/properties/" + propertyKey)
                                .Result.Content.ReadAsJson();
                        propertyResponse["value"].ToString().should_be(propertyValue);
                    };
                };
            };
        }
    }
}