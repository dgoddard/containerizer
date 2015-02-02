﻿#region

using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Containerizer.Services.Interfaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using IronFoundry.Container;
using Containerizer.Models;

#endregion

namespace Containerizer.Controllers
{
    public class CreateResponse
    {
        [JsonProperty("id")]
        public string Id { get; set; }
    }

    public class ContainersController : ApiController
    {
        private readonly IContainerPathService containerPathService;
        private readonly IContainerService containerService;
        private readonly IPropertyService propertyService;


        public ContainersController(IContainerPathService containerPathService,
            IContainerService containerService, IPropertyService propertyService)
        {
            this.containerPathService = containerPathService;
            this.containerService = containerService;
            this.propertyService = propertyService;
        }

        [Route("api/containers")]
        [HttpGet]
        public IReadOnlyList<string> Index()
        {
            return containerService.GetContainers().Select(x => x.Handle).ToList();
        }

        [Route("api/containers")]
        [HttpPost]
        public CreateResponse Create(ContainerSpecApiModel spec)
        {
            var containerSpec = new ContainerSpec
            {
                Handle = spec.Handle,
            };

            var container = containerService.CreateContainer(containerSpec);
            if (container == null)
            {
                throw new HttpResponseException(HttpStatusCode.InternalServerError);
            }

            if (spec.Properties != null)
            {
                var containerPath = container.Directory.MapUserPath("");
                propertyService.BulkSetWithContainerPath(containerPath, spec.Properties);
            }
            
            return new CreateResponse
            {
                Id = container.Handle
            };
        }

        [Route("api/containers/{id}")]
        [HttpDelete]
        public IHttpActionResult Destroy(string id)
        {
            var container = containerService.GetContainerByHandle(id);
            if (container != null)
            {
                containerService.DestroyContainer(id);
                return Ok();
            }

            return NotFound();
        }

        public IHttpActionResult StopContainer(string id)
        {
            var container = containerService.GetContainerByHandle(id);
            if (container != null)
            {
                container.Stop(true);
                return Ok();
            }
            return NotFound();
        }
    }
}

