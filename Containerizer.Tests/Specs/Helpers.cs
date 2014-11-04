using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Web.Administration;

namespace Containerizer.Tests
{
    public static class Helpers
    {
        public static void RemoveExistingSite(string siteName, string applicationPoolName)
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
    }
}
