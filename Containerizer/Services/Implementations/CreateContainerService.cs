﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Containerizer.Services.Interfaces;
using Microsoft.Web.Administration;
using System.IO;

namespace Containerizer.Services.Implementations
{
    public class CreateContainerService : ICreateContainerService
    {
        public Task<string> CreateContainer()
        {
            try
            {
                var id = Guid.NewGuid().ToString();
                var serverManager = new ServerManager();
                var path = Path.Combine("C", "containerizer", id);
                var site = serverManager.Sites.Add(id, path, 0);

                serverManager.ApplicationPools.Add(id);
                site.Applications[0].ApplicationPoolName = id;
                var appPool = serverManager.ApplicationPools[id];
                appPool.ManagedPipelineMode = ManagedPipelineMode.Integrated;

                return Task.Factory.StartNew(() =>
                {
                    serverManager.CommitChanges();
                    return id;
                });
            }
            catch (Exception ex)
            {
                throw new CouldNotCreateContainerException(ex);
            }
        }
    }
}
