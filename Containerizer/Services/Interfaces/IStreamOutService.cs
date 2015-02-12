#region

using System.IO;
using Containerizer.Models;

#endregion

namespace Containerizer.Services.Interfaces
{
    public interface IStreamOutService
    {
        Stream StreamOutFile(string id, LinuxAbsolutePath source);
    }
}