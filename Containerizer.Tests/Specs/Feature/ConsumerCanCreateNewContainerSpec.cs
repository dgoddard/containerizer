using System;
using System.Collections.Generic;
using NSpec;
using System.Linq;
using System.Web.Http.Results;
using System.IO;
using Microsoft.Web.Administration;
using System.Net.Http;
using System.Collections.Specialized;

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
                HttpClient client = null;
                string id = null;
                NameValueCollection response = null;
                ServerManager serverManager = new ServerManager();

                Action givenThatImAConsumerOfTheApi = () =>
                {
                    client = new HttpClient();
                    client.BaseAddress = new Uri("http://localhost:" + port.ToString());
                };

                Action whenIPostARequest = () =>
                {
                    var postTask = client.PostAsync("/api/Containers", new FormUrlEncodedContent(new List<KeyValuePair<string, string>>()));
                    postTask.Wait();
                    var postResult = postTask.Result;
                    var readTask = postResult.Content.ReadAsFormDataAsync();
                    readTask.Wait();
                    response = readTask.Result;
                };

                Action thenIShouldReceiveTheContainersIdInTheResponse = () =>
                {
                    response.should_not_be_null();
                    response["id"].should_not_be_null();
                    response["id"].should_not_be_empty();
                };

                Action andIShouldSeeANewSiteWithTheContainersId = () =>
                    {

                        serverManager.Sites.should_contain(x => x.Name == id);
                    };

                Action andTheSiteSHouldHaveANewAppPoolWithTheSameNameAsTheContainersId = () =>
                {

                    serverManager.Sites.First(x => x.Name == id).Applications[0].ApplicationPoolName.should_be(id);
                };

                givenThatImAConsumerOfTheApi();
                whenIPostARequest();
                thenIShouldReceiveTheContainersIdInTheResponse();
                andIShouldSeeANewSiteWithTheContainersId();
                andTheSiteSHouldHaveANewAppPoolWithTheSameNameAsTheContainersId();





            };
        }
    }
}


