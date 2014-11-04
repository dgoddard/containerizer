using System;
using System.Collections.Generic;
using NSpec;
using System.Linq;
using System.Web.Http.Results;
using System.IO;
using Microsoft.Web.Administration;
using System.Net.Http;

namespace Containerizer.Tests
{
    class ConsumerCanCreateNewContainerSpec : nspec
    {
        Containerizer.Controllers.ContainersController containersController;
        int port;

        void before_each()
        {
            port = 8088;
            SetupSiteInIIS("Containerizer", "Containerizer.Tests", "ContainerizerTestsApplicationPool", port);

        }

        void after_each()
        {
            RemoveExistingSite("Containerizer.Tests", "ContainerizerTestsApplicationPool");
        }

        private static void SetupSiteInIIS(string applicationFolderName, string siteName, string applicationPoolName, int port)
        {
            ServerManager serverManager = new ServerManager();
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, applicationFolderName);

            RemoveExistingSite(siteName, applicationPoolName);

            Site mySite = serverManager.Sites.Add(siteName, path, port);
            mySite.ServerAutoStart = true;

            serverManager.ApplicationPools.Add(applicationPoolName);
            mySite.Applications[0].ApplicationPoolName = applicationPoolName;
            ApplicationPool apppool = serverManager.ApplicationPools[applicationPoolName];
            apppool.ManagedRuntimeVersion = "v4.0";
            apppool.ManagedPipelineMode = ManagedPipelineMode.Integrated;

            serverManager.CommitChanges();
        }

        private static void RemoveExistingSite(string siteName, string applicationPoolName)
        {
            try
            {
                ServerManager serverManager = new ServerManager();
                var existingSite = serverManager.Sites.FirstOrDefault(x => x.Name == siteName);
                if (existingSite != null)
                {
                    serverManager.Sites.Remove(existingSite);
                    serverManager.CommitChanges();
                }

                var existingAppPool = serverManager.ApplicationPools.FirstOrDefault(x => x.Name == applicationPoolName);
                if (existingAppPool != null)
                {
                    serverManager.ApplicationPools.Remove(existingAppPool);
                    serverManager.CommitChanges();
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                throw new Exception("Try running Visual Studio/test runner as Administrator instead.", ex);
            }
        }

        void describe_consumer_can_create_new_container()
        {
            it["can create a new container"] = () =>
            {
                var client = new HttpClient();
                client.BaseAddress = new Uri("http://localhost:" + port.ToString());
                var postTask = client.PostAsync("/api/Containers", new FormUrlEncodedContent(new List<KeyValuePair<string, string>>()));
                postTask.Wait();
                var response = postTask.Result;
                var readTask = response.Content.ReadAsFormDataAsync();
                readTask.Wait();
                var data = readTask.Result;
                var id = data["id"];

                var serverManager = new ServerManager();
                serverManager.Sites.should_contain(x => x.Name == id);

            };
        }
    }
}


