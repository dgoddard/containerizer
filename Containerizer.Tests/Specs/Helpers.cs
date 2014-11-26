﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Reflection;
using Microsoft.Web.Administration;
using Newtonsoft.Json.Linq;

namespace Containerizer.Tests.Specs
{
    public static class Helpers
    {

        /// <returns>The newly created container's id.</returns>
        public static string CreateContainer(HttpClient client)
        {
            var postTask = client.PostAsync("/api/Containers",
                new FormUrlEncodedContent(new List<KeyValuePair<string, string>>()));
            postTask.Wait();
            var postResult = postTask.Result;
            var readTask = postResult.Content.ReadAsStringAsync();
            readTask.Wait();
            var response = readTask.Result;
            var json = JObject.Parse(response);
            return json["id"].ToString();
        }

        public static void SetupSiteInIIS(string applicationFolderName, string siteName, string applicationPoolName, int port, bool privleged)
        {
            try
            {
                var serverManager = new ServerManager();
                var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, applicationFolderName);

                RemoveExistingSite(siteName, applicationPoolName);

                Site mySite = serverManager.Sites.Add(siteName, path, port);
                mySite.ServerAutoStart = true;

                serverManager.ApplicationPools.Add(applicationPoolName);
                mySite.Applications[0].ApplicationPoolName = applicationPoolName;
                ApplicationPool apppool = serverManager.ApplicationPools[applicationPoolName];
                if (privleged)
                {
                    apppool.ProcessModel.IdentityType = ProcessModelIdentityType.LocalSystem;
                }
                apppool.ManagedRuntimeVersion = "v4.0";
                apppool.ManagedPipelineMode = ManagedPipelineMode.Integrated;

                serverManager.CommitChanges();
            }
            catch (System.Runtime.InteropServices.COMException ex)
            {
                if (ex.Message.Contains("2B72133B-3F5B-4602-8952-803546CE3344"))
                {
                    throw new Exception("Please install IIS.", ex);
                }
                throw;
            }
        }

        public static void RemoveExistingSite(string siteName, string applicationPoolName)
        {
            try
            {
                var serverManager = new ServerManager();
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
            catch (System.Runtime.InteropServices.COMException ex)
            {
                if (ex.Message.Contains("2B72133B-3F5B-4602-8952-803546CE3344"))
                {
                    throw new Exception("Please install IIS.", ex);
                }
                throw;
            }
            catch (UnauthorizedAccessException ex)
            {
                throw new Exception("Try running Visual Studio/test runner as Administrator instead.", ex);
            }
        }



        public static DataReceivedEventArgs CreateMockDataReceivedEventArgs(string TestData)
        {
            var MockEventArgs =
                (DataReceivedEventArgs) System.Runtime.Serialization.FormatterServices
                    .GetUninitializedObject(typeof (DataReceivedEventArgs));

            FieldInfo[] EventFields = typeof (DataReceivedEventArgs)
                .GetFields(
                    BindingFlags.NonPublic |
                    BindingFlags.Instance |
                    BindingFlags.DeclaredOnly);

            if (EventFields.Any())
            {
                EventFields[0].SetValue(MockEventArgs, TestData);
            }
            else
            {
                throw new ApplicationException(
                    "Failed to find _data field!");
            }

            return MockEventArgs;
        }

        public static bool PortIsUsed(int port)
        {
            IPGlobalProperties ipGlobalProperties = IPGlobalProperties.GetIPGlobalProperties();
            TcpConnectionInformation[] tcpConnInfoArray = ipGlobalProperties.GetActiveTcpConnections();

            foreach (TcpConnectionInformation tcpi in tcpConnInfoArray)
            {
                if (tcpi.LocalEndPoint.Port == port)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
