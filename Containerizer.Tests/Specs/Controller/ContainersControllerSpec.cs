using System;
using System.Collections.Generic;
using NSpec;
using System.Linq;
using System.Web.Http.Results;
using System.Net.Http;
using System.Threading;
using Newtonsoft.Json.Linq;

namespace Containerizer.Tests
{
    class ContainersControllerSpec : nspec
    {
        Containerizer.Controllers.ContainersController containersController;

        void before_each()
        {
            containersController = new Controllers.ContainersController();
            containersController.Configuration = new System.Web.Http.HttpConfiguration();
            containersController.Request = new HttpRequestMessage();
        }

        void describe_create()
        {
            context["when the container is created successfully"] = () =>
            {
                var containerId = Guid.NewGuid().ToString();

                it["returns a successful status code"] = () =>
                {
                    var postTask = containersController.Post().ExecuteAsync(new CancellationToken());
                    postTask.Wait();
                    postTask.Result.IsSuccessStatusCode.should_be_true();

                };

                it["returns the container's id"] = () =>
                {
                    var postTask = containersController.Post().ExecuteAsync(new CancellationToken());
                    postTask.Wait();
                    var readTask = postTask.Result.Content.ReadAsStringAsync();
                    readTask.Wait();
                    var json = JObject.Parse(readTask.Result);
                    json["id"].ToString().should_be(containerId);
                };
            };
            context["when the container is not created successfully"] = () =>
            {
                it["returns a error status code"] = () =>
                {
                    var postTask = containersController.Post().ExecuteAsync(new CancellationToken());
                    postTask.Wait();
                    postTask.Result.IsSuccessStatusCode.should_be_false();
                };

            };
        }
    }
}


