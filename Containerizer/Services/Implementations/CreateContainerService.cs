using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Containerizer.Services.Interfaces;

namespace Containerizer.Services.Implementations
{
    public class CreateContainerService : ICreateContainerService
    {
        Task<string> ICreateContainerService.CreateContainer()
        {
            throw new NotImplementedException();
        }
    }
}
