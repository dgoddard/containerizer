﻿#region

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Web.Http;
using Containerizer.Controllers;
using Containerizer.Services.Interfaces;
using Moq;
using NSpec;
using IronFoundry.Container;
using System.Web.Http.Results;

#endregion

namespace Containerizer.Tests.Specs.Controllers
{
    internal class NetControllerSpec : nspec
    {
        private void describe_()
        {
            Mock<IContainerService> mockContainerService = null;
            NetController netController = null;

            before = () =>
            {
                mockContainerService = new Mock<IContainerService>();

                netController = new NetController(mockContainerService.Object);
            };

            describe[Controller.Create] = () =>
            {
                string containerId = null;
                int requestedHostPort = 0;
                IHttpActionResult result = null;
                Mock<IContainer> mockContainer = null;

                before = () =>
                {
                    containerId = Guid.NewGuid().ToString();
                    requestedHostPort = 0;
                    mockContainer = new Mock<IContainer>();

                    mockContainerService.Setup(x => x.GetContainerByHandle(containerId)).Returns(mockContainer.Object);
                };

                act = () =>
                {
                    result = netController.Create(containerId, new NetInRequest { HostPort = requestedHostPort });
                };

                it["reserves the port in the container"] = () =>
                {
                    mockContainer.Verify(x => x.ReservePort(requestedHostPort));
                };

                context["when the container does not exist"] = () =>
                {
                    before = () =>
                    {
                        mockContainerService.Setup(x => x.GetContainerByHandle(It.IsAny<string>())).Returns(null as IContainer);
                    };

                    it["Returns not found"] = () =>
                    {
                        result.should_cast_to<NotFoundResult>();
                    };
                };

                context["reserving the port in the container succeeds and returns a port"] = () =>
                {
                    before = () =>
                    {
                        mockContainer.Setup(x => x.ReservePort(requestedHostPort)).Returns(8765);
                    };

                    it["calls reservePort on the container"] = () =>
                        {
                            mockContainer.Verify(x => x.ReservePort(requestedHostPort));
                        };

                    context["container reservePort succeeds and returns a port"] = () =>
                    {
                        before = () =>
                        {
                            mockContainer.Setup(x => x.ReservePort(requestedHostPort)).Returns(8765);
                        };

                        it["returns the port that the net in service returns"] = () =>
                        {
                            var jsonResult = result.should_cast_to<JsonResult<NetInResponse>>();
                            jsonResult.Content.HostPort.should_be(8765);
                        };
                    };

                    context["reserving the port in the container fails and throws an exception"] = () =>
                    {
                        before = () =>
                        {
                            mockContainer.Setup(x => x.ReservePort(requestedHostPort)).Throws(new Exception("BOOM"));
                        };

                        it["returns an error"] = () =>
                        {
                            var errorResult = result.should_cast_to<ExceptionResult>();
                            errorResult.Exception.Message.should_be("BOOM");
                        };
                    };
                };
            };
        }
    }
}