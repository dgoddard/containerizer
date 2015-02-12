#region

using System.IO;
using Containerizer.Models;
using Containerizer.Services.Interfaces;
using IronFoundry.Container;

#endregion

namespace Containerizer.Services.Implementations
{
    public class StreamOutService : IStreamOutService
    {
        private readonly IContainerService containerService;
        private readonly ITarStreamService tarStreamService;

        public StreamOutService(IContainerService containerService, ITarStreamService tarStreamService)
        {
            this.containerService = containerService;
            this.tarStreamService = tarStreamService;
        }

        public Stream StreamOutFile(string id, LinuxAbsolutePath path)
        {
            IContainer container = containerService.GetContainerByHandle(id);
            string windowsPath = container.Directory.MapUserPath(path.Value);
            Stream stream = tarStreamService.WriteTarToStream(windowsPath);
            return stream;
        }
    }
}
