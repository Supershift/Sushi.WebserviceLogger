using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using static Sushi.WebserviceLogger.SampleService.SampleController;

namespace Sushi.WebserviceLogger.SampleService
{
    /// <summary>
    /// Summary description for SampleAsmxService
    /// </summary>
    [WebService(Namespace = "http://www.supershift.nl/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    // [System.Web.Script.Services.ScriptService]
    public class SampleAsmxService : System.Web.Services.WebService
    {

        [WebMethod]
        public string HelloWorld()
        {
            return "Hello World";
        }

        [WebMethod]
        public OrderResponse AddOrder(OrderRequest request)
        {
            if (request == null || request.Quantity < 0)
                throw new Exception("Empty request or quantity invalid");

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
    }
}
