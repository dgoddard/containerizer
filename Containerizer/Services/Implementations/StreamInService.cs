﻿#region

using System.IO;
using Containerizer.Services.Interfaces;
using IronFoundry.Container;

#endregion

namespace Containerizer.Services.Implementations
{
    public class StreamInService : IStreamInService
    {
        private readonly ITarStreamService tarStreamService;
        private readonly IContainerService containerService;

        public StreamInService(IContainerService containerService, ITarStreamService tarStreamService)
        {
            this.tarStreamService = tarStreamService;
            this.containerService = containerService;
        }

        public void StreamInFile(Stream stream, string id, string destination)
        {
            IContainer container = containerService.GetContainerByHandle(id);
            var path = container.Directory.MapUserPath(destination);
            tarStreamService.WriteTarStreamToPath(stream, path);
        }
    }
}