using ManagementSystem.Contract.Response;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ManagementSystem.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProjectController : ControllerBase
    {

        public ProjectController()
        {
 
        }

        [HttpGet("GetList")]
        public SingleResponeMessage<string> GetList() { 
            var res = new SingleResponeMessage<string>();
            res.Item = "Hello Haiduong";
            res.IsSuccess = false;
            return res;
        }
    }
}
