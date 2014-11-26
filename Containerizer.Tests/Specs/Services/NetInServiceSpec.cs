using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Containerizer.Services.Implementations;
using Microsoft.Web.Administration;
using Newtonsoft.Json.Linq;
using NSpec;

namespace Containerizer.Tests.Specs.Services
{
    internal class NetInServiceSpec : nspec
    {
        private void describe_()
        {
            context["#AddPort"] = () =>
            {
                CreateContainerService createContainerService = null;
                string containerId = null;

                before = () =>
                {
                    createContainerService = new CreateContainerService();
                    containerId = createContainerService.CreateContainer().GetAwaiter().GetResult();
                    containerId.should_not_be_null();
                };

                after = () =>
                {
                    Helpers.RemoveExistingSite(containerId, containerId);
                };

                it["Adds a binding to the container in IIS"] = () =>
                {
                    new NetInService().AddPort(7868, containerId);
                    var serverManager = new ServerManager();
                    var existingSite = serverManager.Sites.First(x => x.Name == containerId);
                    existingSite.Bindings.Any(x => x.EndPoint.Port == 7868).should_be_true();
                };

                context["when there is a binding on port 0"] = () =>
                {
                    it["removes the port 0 binding"] = () =>
                    {
                        var serverManager = new ServerManager();
                        var existingSite = serverManager.Sites.First(x => x.Name == containerId);

                        existingSite.Bindings.Any(x => x.EndPoint.Port == 0).should_be_true();

                        new NetInService().AddPort(7868, containerId);

                        existingSite.Bindings.Any(x => x.EndPoint.Port == 0).should_be_false();
                    };
                };

                context["when port is zero"] = () =>
                {
                    it["picks an unused port and returns it"] = () =>
                    {

                    };

                };
            };
        }
    }
}