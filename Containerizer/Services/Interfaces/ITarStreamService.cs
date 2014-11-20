﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Containerizer.Services.Interfaces
{
    public interface ITarStreamService
    {
        System.IO.Stream WriteTarToStream(string filePath);
        void WriteTarStreamToPath(Stream steam, string filePath);
    }
}
