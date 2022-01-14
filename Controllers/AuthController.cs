using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RoomBookingAPI.Model;


namespace RoomBookingAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly ILogger<AuthController> _logger;
        private readonly Authentication auth;

        public AuthController(ILogger<AuthController> logger)
        {
            _logger = logger;
            auth = new Authentication();
        }

        [HttpGet]
        [Route("api/[controller]/CreateToken")]
        public string CreateToken(string name, string password)
        {
            if (auth.TryCreateToken(name, password, out var token))
                return token;
            
            return "";
        }
    }
}
