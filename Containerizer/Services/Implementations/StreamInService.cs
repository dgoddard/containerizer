﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Xml.Linq;
using Containerizer.Services.Interfaces;

namespace Containerizer.Services.Implementations
{
    public class StreamInService : IStreamInService
    {
        private readonly IContainerPathService containerPathService;
        private ITarStreamService tarStreamService;

        public StreamInService(IContainerPathService containerPathService, ITarStreamService tarStreamService)
        {
            this.containerPathService = containerPathService;
            this.tarStreamService = tarStreamService;
        }

        public void StreamInFile(string id, string destination)
        {
            throw new NotImplementedException();
        }
    }
}