using System;
using System.Collections.Generic;
using NSpec;
using System.Linq;
using System.Web.Http.Results;
using System.Net.Http;
using System.Threading;
using Newtonsoft.Json.Linq;
using Containerizer.Services.Interfaces;
using Moq;

namespace Containerizer.Tests
{
    class ContainersControllerSpec : nspec
    {
        Containerizer.Controllers.ContainersController containersController;
        Mock<ICreateContainerService> mockCreateContainerService;

        void before_each()
        {
            mockCreateContainerService = new Mock<ICreateContainerService>();
            containersController = new Controllers.ContainersController(mockCreateContainerService.Object);
            containersController.Configuration = new System.Web.Http.HttpConfiguration();
            containersController.Request = new HttpRequestMessage();
        }

        void describe_create()
        {
            context["when the container is created successfully"] = () =>
            {
                var containerId = Guid.NewGuid().ToString();
                mockCreateContainerService.Setup(x => x.CreateContainer())
                    .ReturnsAsync(containerId);

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
                mockCreateContainerService.Setup(x => x.CreateContainer())
                    .Throws<CouldNotCreateContainerException>();

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


