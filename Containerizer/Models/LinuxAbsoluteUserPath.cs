using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Containerizer.Models
{
    public class LinuxAbsoluteUserPath
    {
        public LinuxAbsoluteUserPath(string path)
        {
            if (path.StartsWith("/user"))
            {
                Value = path;
            }

            else
            {
                throw new ArgumentException("Path should begin with '/user'.");
            }
        }

        public string Value { get; private set; }

        public static LinuxAbsoluteUserPath FromLinuxAbsolutePath(LinuxAbsolutePath path)
        {
            return new LinuxAbsoluteUserPath("/user" + path.Value);
        }
    }
}