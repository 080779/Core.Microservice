using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Email.Controllers
{
    [Produces("application/json")]
    [Route("api/Email")]
    public class EmailController : Controller
    {
        public string Get()
        {
            return "发送网易邮箱成功";
        }
        public string Post()
        {
            return "发送腾讯邮箱成功";
        }
    }
}