using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace Alterity.Controllers
{
public class ExampleController : JRPCNet.ApiController
{
    [JRPCNet.ApiMethod]
    public int Add(int left, int right)
    {
        return left + right;
    }
}
}
