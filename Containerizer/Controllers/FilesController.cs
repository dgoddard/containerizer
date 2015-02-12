#region

using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Containerizer.Models;
using Containerizer.Services.Interfaces;

#endregion

namespace Containerizer.Controllers
{
    public class FilesController : ApiController
    {
        private readonly IStreamInService streamInService;
        private readonly IStreamOutService streamOutService;


        public FilesController(IStreamInService streamInService, IStreamOutService streamOutService)
        {
            this.streamOutService = streamOutService;
            this.streamInService = streamInService;
        }

        [Route("api/containers/{id}/files")]
        [HttpGet]
        public Task<HttpResponseMessage> Show(string id, string source)
        {
            var path = new LinuxAbsolutePath(source);
            Stream outStream = streamOutService.StreamOutFile(id, path);
            HttpResponseMessage response = Request.CreateResponse();
            response.Content = new StreamContent(outStream);
            return Task.FromResult(response);
        }

        [Route("api/containers/{id}/files")]
        [HttpPut]
        public async Task<IHttpActionResult> Update(string id, string destination)
        {
            var path = new LinuxAbsolutePath(destination);
            Stream stream = await Request.Content.ReadAsStreamAsync();
            streamInService.StreamInFile(stream, id, path);
            return Ok();
        }
    }
}