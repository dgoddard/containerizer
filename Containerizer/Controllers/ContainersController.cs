using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Newtonsoft.Json;
using Containerizer.Services.Interfaces;

namespace Containerizer.Controllers
{
    public class Response
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

        public IHttpActionResult Post()
        {
            return Json(new Response { Id = "hi" });
        }
    }
}
