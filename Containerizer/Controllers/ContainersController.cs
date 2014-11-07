using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Newtonsoft.Json;
using Containerizer.Services.Interfaces;
using System.Threading.Tasks;
using tar_cs;

namespace Containerizer.Controllers
{
    public class CreateResponse
    {
        [JsonProperty("id")]
        public string Id { get; set; }
    }

    public class ContainersController : ApiController
    {
        private ICreateContainerService createContainerService;

        public ContainersController(ICreateContainerService createContainerService)
        {
            this.createContainerService = createContainerService;
        }

        [Route("api/containers")]
        public async Task<IHttpActionResult> Post()
        {
            try
            {
               var id = await createContainerService.CreateContainer();
               return Json(new CreateResponse { Id = id });
            }
            catch (CouldNotCreateContainerException ex)
            {
                return this.InternalServerError(ex);
            }
        }

        [Route("api/containers/{id}/files")]
        public  Task<HttpResponseMessage> StreamOut(string source)
        {
            var outStream = new MemoryStream();
            using (var tar = new TarWriter(outStream))
            {
                tar.Write(source);
            }
            var response = Request.CreateResponse();
            response.Content = new StreamContent(outStream);
            return Task.FromResult(response);
        }
    }
}
