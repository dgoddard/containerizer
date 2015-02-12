#region

using System.IO;
using Containerizer.Models;

#endregion

namespace Containerizer.Services.Interfaces
{
    public interface IStreamInService
    {
        void StreamInFile(Stream steam, string id, LinuxAbsolutePath destination);
    }
}