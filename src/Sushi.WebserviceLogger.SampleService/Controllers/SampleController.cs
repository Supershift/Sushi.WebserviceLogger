using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Sushi.WebserviceLogger.Core;
using Sushi.WebserviceLogger.SampleService.Filters;

namespace Sushi.WebserviceLogger.SampleService.Controllers
{   
    [Route("api/")]
    [Route("mock/")]
    [Route("filter/")]
    [Route("none/")]    
    [ServiceFilter(typeof(Filter.WebserviceLoggerFilter<MyLogItem>))]
    public class SampleController : ControllerBase
    {
        [HttpGet]
        [Route("ping")]        
        public ActionResult Ping()
        {
            return Ok("hello world");
        }

        [HttpGet]
        [Route("fromQuery")]
        public IActionResult FromQuery([Required, MaxLength(256)] string input)
        {
            return Ok(input);
        }

        /// <summary>
        /// Echo's the request after the specified delay, with a max time of 60 seconds. Delay time is specified in milliseconds.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("echo")]        
        public async Task<ActionResult<MyPayload>> Echo(MyPayload request, [FromQuery] int input = 0)
        {
            if(input > 0)
            {
                int delayTime = Math.Min(input, 60000);
                await Task.Delay(delayTime);
            }
            return Ok(request);
        }

        public class MyPayload
        {
            public string Data { get; set; }
        }

        /// <summary>
        /// Always throws an exception
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("exception")]
        public ActionResult Exception(MyPayload request)
        {
            throw new Exception("Exception triggered by calling the exception API.");
        }

        /// <summary>
        /// Triggers error handling filter.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [TypeFilter(typeof(ErrorFilter))]
        [HttpPost]
        [Route("internalServerError")]
        public ActionResult InternalServerError(MyPayload request)
        {
            throw new Exception("Exception triggered by calling the internal server error API.");
        }

        /// <summary>
        /// Always returns a bad request
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("badrequest")]
        public ActionResult BadRequest(MyPayload request)
        {
            if(string.IsNullOrWhiteSpace(request.Data))
            {
                return BadRequest();
            }
            else
            {
                return BadRequest("request.Data was filled wrong");
            }
        }

        [HttpGet]
        [Route("order/{orderID}")]        
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
        [Route("order")]        
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