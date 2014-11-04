﻿using System;
using System.Collections.Generic;
using NSpec;
using System.Linq;
using System.Web.Http.Results;
using System.IO;
using Microsoft.Web.Administration;

namespace Containerizer.Tests
{
    class ConsumerCanCreateNewContainerSpec : nspec
    {
        Containerizer.Controllers.ContainersController containersController;

        void before_each()
        {
            var applicationFolderName = "Containerizer";
            var siteName = "Containerizer.Tests";
            var applicationPoolName = "ContainerizerTestsApplicationPool";
            ServerManager serverManager = new ServerManager();
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, applicationFolderName);

            try
            {
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

            Site mySite = serverManager.Sites.Add(siteName, path, 8080);
            mySite.ServerAutoStart = true;

            serverManager.ApplicationPools.Add(applicationPoolName);
            mySite.Applications[0].ApplicationPoolName = applicationPoolName;
            ApplicationPool apppool = serverManager.ApplicationPools[applicationPoolName];
            apppool.ManagedRuntimeVersion = "v4.0"; 
            apppool.ManagedPipelineMode = ManagedPipelineMode.Integrated;

            serverManager.CommitChanges();

        }

        void describe_get()
        {
            it["returns an array of strings"] = () =>
            {
                1.should_be_greater_than(0);
            };
        }
    }
}


