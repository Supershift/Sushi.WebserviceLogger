using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Sushi.WebserviceLogger.AspNetCore.SampleService.Controllers
{   
    [ApiController]
    public class SampleController : ControllerBase
    {
        [HttpGet]
        [Route("api/ping")]
        public ActionResult Ping()
        {
            return Ok();
        }

        [HttpGet]
        [Route("api/order")]
        public ActionResult<OrderResponse> GetOrder(Guid orderID)
        {
            if (orderID == Guid.Empty)
                return BadRequest("Empty or invalid orderID");

            if (orderID == new Guid("1E254F00-11D1-45FE-9A4B-1B552CC4E7E7"))
            {
                throw new Exception("Exception triggered by request");
            }

            var result = new OrderResponse()
            {
                Amount = 15,
                ClientTransactionID = Guid.NewGuid().ToString(),
                ProductID = "12412",
                OrderID = orderID,
                Quantity = 1
            };

            return result;
        }

        [HttpPost]
        [Route("api/order")]
        public ActionResult<OrderResponse> AddOrder(OrderRequest request)
        {
            if (request == null || request.Quantity < 0)
                return BadRequest("Empty request or quantity invalid");

            var result = new OrderResponse()
            {
                Amount = 15 * request.Quantity,
                ClientTransactionID = request.ClientTransactionID,
                ProductID = request.ProductID,
                OrderID = Guid.NewGuid(),
                Quantity = request.Quantity
            };

            return result;
        }

        public class OrderRequest
        {
            public int Quantity { get; set; }
            public string ProductID { get; set; }
            public string ClientTransactionID { get; set; }
        }

        public class OrderResponse
        {
            public int Quantity { get; set; }
            public string ProductID { get; set; }
            public string ClientTransactionID { get; set; }
            public Guid OrderID { get; set; }
            public decimal Amount { get; set; }
        }
    }
}