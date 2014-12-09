using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Containerizer.Services.Interfaces;
using Microsoft.Web.Administration;

namespace Containerizer.Services.Implementations
{
    public class CreateContainerService : ICreateContainerService
    {
        public Task<string> CreateContainer(string id)
        {
            return Task.Factory.StartNew(() =>
            {
                try
                {
                    ServerManager serverManager = ServerManager.OpenRemote("localhost");
                    string rootDir =
                        Directory.GetDirectoryRoot(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));
                    string path = Path.Combine(rootDir, "containerizer", id);
                    Site site = serverManager.Sites.Add(id, path, 0);

                    // FIXME: test removing dashes from app pool name
                    var appPoolId = id.Replace("-", "");
                    serverManager.ApplicationPools.Add(appPoolId);
                    site.Applications[0].ApplicationPoolName = appPoolId;
                    ApplicationPool appPool = serverManager.ApplicationPools[appPoolId];
                    appPool.ManagedPipelineMode = ManagedPipelineMode.Integrated;

                    serverManager.CommitChanges();
                    Directory.CreateDirectory(path);
                    return id;
                }
                catch (COMException ex)
                {
                    if (ex.Message.Contains("2B72133B-3F5B-4602-8952-803546CE3344"))
                    {
                        throw new CouldNotCreateContainerException("Please install IIS.", ex);
                    }
                    throw new CouldNotCreateContainerException("", ex);
                }
                catch (Exception ex)
                {
                    throw new CouldNotCreateContainerException(String.Empty, ex);
                }
            });
        }
    }
}