using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Containerizer.Models
{
    public class LinuxAbsolutePath
    {
        public LinuxAbsolutePath(string path)
        {
            if (path.StartsWith("/"))
            {
                Value = path;
            }

            else
            {
                throw new ArgumentException("Path should begin with '/'.");
            }
        }

        public string Value { get; private set; }
    }
}