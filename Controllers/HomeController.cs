using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using JwtAuthCore2.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Authorization;
using Newtonsoft.Json;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using JwtAuthCore2.Services;

namespace JwtAuthCore2.Controllers
{
    public class HomeController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signinManager;
        private readonly IJwtFactory _jwtFactory;
        private readonly JwtIssuerOptions _jwtOptions;
        private readonly IEmailSender _emailsender;       
        
        public HomeController(UserManager<ApplicationUser> userManager, IJwtFactory jwtFactory, IOptions<JwtIssuerOptions> jwtOptions,
        SignInManager<ApplicationUser> signinManager,IEmailSender emailsender)
        {
            _userManager = userManager;
            _jwtFactory = jwtFactory;
            _jwtOptions = jwtOptions.Value;
            _signinManager = signinManager;
            _emailsender = emailsender;        
        }

        public IActionResult Index()
        {          
           
            return View();
        }

        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";
            return View();
        }

        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [HttpGet]
        [Authorize]
        public IActionResult RequestToken()
        {          
            return View();
        }

        [HttpPost]
        [Authorize(AuthenticationSchemes=JwtBearerDefaults.AuthenticationScheme,Policy = "ApiUser")]      
        public IActionResult Prot([FromBody] Person person)
        {         
            return Json($" dear {person.name} You have a secret and it is that your birthday is not {person.birthday}. It is indeed 2 days later");
        }


        [HttpGet]
        [Authorize]
        public IActionResult InquireTokenApi()
        {
            return View(new TokenIssued());
        }



        [Authorize]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult> RequestToken(CredentialsViewModel credentials, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var identity = await GetClaimsIdentity(credentials.UserName, credentials.Password);
            if (identity == null)
            {
                //return BadRequest();
                return View(credentials);
            }
            var jwt = await Tokens.GenerateJwt(identity, _jwtFactory,identity.Name, _jwtOptions, new JsonSerializerSettings { Formatting = Formatting.Indented });
            TokenIssued tsi = JsonConvert.DeserializeObject<TokenIssued>(jwt);
            await _emailsender.SendEmailAsync("burlandodiego@alice.it", "Tokenrequested", $"<H2>Here's the token</H2><p>${tsi.auth_token}</p>");
            return View("InquireTokenApi", tsi);       
        }

        private async Task<ClaimsIdentity> GetClaimsIdentity(string userName, string password)
        {
            if (string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(password))
                return await Task.FromResult<ClaimsIdentity>(null);

            // get the user to verifty
            var userToVerify = await _userManager.FindByNameAsync(userName);
            if (userToVerify == null) return await Task.FromResult<ClaimsIdentity>(null);
            // check the credentials
            if (await _userManager.CheckPasswordAsync(userToVerify, password))
            {
                return await Task.FromResult(_jwtFactory.GenerateClaimsIdentity(userName, userToVerify.Id));
            }
            // Credentials are invalid, or account doesn't exist
            return await Task.FromResult<ClaimsIdentity>(null);
        }


    }
}

/*
{
  "id": "5cedfe45-caef-4dcb-aad6-1f3d3644733f",
  "auth_token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiJEaWVnbyIsImp0aSI6IjEzM2FjOWM5LWM1NjgtNGYzZC04YTUzLWE3NTQzNjI5ZTM0ZCIsImlhdCI6MTUyMDAwMTY2Niwicm9sIjoiYXBpX2FjY2VzcyIsImlkIjoiNWNlZGZlNDUtY2FlZi00ZGNiLWFhZDYtMWYzZDM2NDQ3MzNmIiwibmJmIjoxNTIwMDAxNjY2LCJleHAiOjE1MjAwMDg4NjYsImlzcyI6IkRpZWdvIEFsZG8gQnVybGFuZG8iLCJhdWQiOiJodHRwOi8vbG9jYWxob3N0OjUwMDAvIn0.OfNVszn9CuT5BclZuHF_QyWmHah59QCReqcQjuMcjqo",
  "expires_in": 7200
}
*/