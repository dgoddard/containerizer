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
using System.IO;
using tar_cs;

namespace Containerizer.Tests
{
    class TarStreamServiceSpec : nspec
    {
        TarStreamService tarStreamService;
        string id;
        string returnedPath;
        private Stream stream;

        void before_each()
        {
            tarStreamService = new TarStreamService();
        }

        void describe_path_service()
        {
            string path = null;
            string tempDir = null;

            before = () =>
            {
                id = Guid.NewGuid().ToString();
                var rootDir =
                    Directory.GetDirectoryRoot(
                        Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location));
                tempDir = Path.Combine(rootDir, "temp" + Guid.NewGuid().ToString());
            };

            after = () => Directory.Delete(tempDir);

            describe["#WriteTarToStream"] = () =>
            {
                context["when tarring a file"] = () =>
                {

                    before = () =>
                    {
                        path = Path.Combine(tempDir, "file.txt");
                        File.WriteAllText(path, "I am file contentz!");
                        stream = tarStreamService.WriteTarToStream(path);

                    };

                    it["creates a tarstream of that file"] = () =>
                    {
                        var tarReader = new TarReader(stream);
                        var outDir = Path.Combine(tempDir, "output");
                        tarReader.ReadToEnd(outDir);
                        File.ReadAllText(Path.Combine(outDir, "file.txt")).should_be("I am file contentz!");
                    };
                };

                context["when tarring a directory"] = () =>
                {
                    it["creates a tarstream of that directory and all its contents, recursively"] = () =>
                    {
                        throw new NotImplementedException();
                    };
                };
            };
        }
    }
}


