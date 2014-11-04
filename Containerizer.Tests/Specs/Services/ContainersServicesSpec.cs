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

namespace Containerizer.Tests
{
    class CreateContainersServiceSpec : nspec
    {
        CreateContainerService createContainerService;
        ServerManager serverManager;
        string id;

        void before_each()
        {
            createContainerService = new CreateContainerService();
            serverManager = new ServerManager();
        }

        void describe_create_container()
        {
            it["creates a new site in IIS named with the given id"] = () =>
            {
                var task = createContainerService.CreateContainer();
                task.Wait();
                id = task.Result;
                serverManager.Sites.should_contain(x => x.Name == id);
            };
            it["creates a new associated app pool in IIS named with the given id"] = () =>
            {
                var task = createContainerService.CreateContainer();
                task.Wait();
                id = task.Result;
                serverManager.Sites.First(x => x.Name == id).Applications[0].ApplicationPoolName.should_be(id);
            };
        }

        void after_each()
        {
            Helpers.RemoveExistingSite(id, id);
        }

    }
}


