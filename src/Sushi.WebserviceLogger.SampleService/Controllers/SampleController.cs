using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Sushi.WebserviceLogger.SampleService.Controllers
{   
    [ApiController]
    public class SampleController : ControllerBase
    {
        [HttpGet]
        [Route("api/ping")]
        [Route("mock/ping")]
        public ActionResult Ping()
        {
            return Ok();
        }

        [HttpGet]
        [Route("api/echo/{input}")]
        [Route("mock/echo/{input}")]
        public ActionResult Echo(string input)
        {
            return Ok(input);
        }

        /// <summary>
        /// Sends a response after the specified delay, with a max time of 60 seconds. Delay time is specified in milliseconds.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("api/delay")]
        [Route("mock/delay")]
        public async Task<ActionResult> Echo(int input)
        {
            if(input > 0)
            {
                int delayTime = Math.Min(input, 60000);
                await Task.Delay(delayTime);
            }
            return Ok(input);
        }

        public class MyPayload
        {
            public string Data { get; set; }
        }

        /// <summary>
        /// Sends a response after the specified delay, with a max time of 60 seconds. Delay time is specified in milliseconds. Response is an echo of request
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("api/random")]
        [Route("mock/random")]
        [Route("none/random")]
        public async Task<ActionResult> Random([FromQuery]int input, [FromBody] MyPayload request)
        {
            if (input > 0)
            {
                int delayTime = Math.Min(input, 60000);
                await Task.Delay(delayTime);
            }
            return Ok(request);
        }

        [HttpGet]
        [Route("api/order")]
        [Route("mock/order")]
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
        [Route("mock/order")]
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

        public class RandomRequest
        {
            public int MinDelay { get; set; }
            public int MaxDelay { get; set; }
            public string Body { get; set; }
        }

        public class RandomResponse
        {
            public int DelayTime { get; set; }
            public string Body { get; set; }
        }
    }
}