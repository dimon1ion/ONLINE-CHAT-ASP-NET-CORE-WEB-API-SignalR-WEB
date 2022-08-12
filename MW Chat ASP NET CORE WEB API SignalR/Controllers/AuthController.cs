using FluentValidation;
using FluentValidation.AspNetCore;
using FluentValidation.Results;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using MW_Chat_ASP_NET_CORE_WEB_API_SignalR.Data;
using MW_Chat_ASP_NET_CORE_WEB_API_SignalR.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace MW_Chat_ASP_NET_CORE_WEB_API_SignalR.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    public class AuthController : ControllerBase
    {
        private UserContext _userContext;
        private IValidator<User> _validator;

        private ConfigurationManager? _configuration;

        private ConfigurationManager Configuration
        {
            get
            {
                if (_configuration == null)
                {
                    _configuration = new ConfigurationManager();
                    _configuration.AddJsonFile("appsettings.json");
                }
                return _configuration;
            }
        }

        public AuthController(IValidator<User> validator, UserContext userContext)
        {   
            this._userContext = userContext;
            this._validator = validator;
        }

        //[HttpGet(Name = "GetWeatherForecast")]
        //public IEnumerable<WeatherForecast> Get()
        //{
        //    return Enumerable.Range(1, 5).Select(index => new WeatherForecast
        //    {
        //        Date = DateTime.Now.AddDays(index),
        //        TemperatureC = Random.Shared.Next(-20, 55),
        //        Summary = Summaries[Random.Shared.Next(Summaries.Length)]
        //    })
        //    .ToArray();
        //}

        [HttpPost]
        public async Task<IActionResult> Register([FromBody]User user)
        {
            var s = _userContext.Users.FirstOrDefault(u => u.Name == user.Name);
            var d = _userContext.Users;
            ValidationResult resultValid = await _validator.ValidateAsync(user);
            if (!resultValid.IsValid)
            {
                resultValid.AddToModelState(this.ModelState, "Hey");
                return BadRequest(ModelState.Values);
            }
            else if (_userContext.Users.FirstOrDefault(u => u.Name == user.Name) != null)
            {
                ModelState.AddModelError("Server", "This user already exists");
                return BadRequest(ModelState.Values);
            }
            await _userContext.Users.AddAsync(user);

            var saving = _userContext.SaveChangesAsync();
            var token = CreateJWTTokenAsync(user.Name);

            await saving;

            return Ok(await token);
        }

        [HttpPost]
        public async Task<IActionResult> Login([FromBody] User user)
        {
            if (_userContext.Users.FirstOrDefault(u => u.Name == user.Name && u.Password == user.Password) == null)
            {
                ModelState.AddModelError("Server", "Login or Password is incorrect");
                return BadRequest(ModelState.Values);
            }
            var token = CreateJWTTokenAsync(user.Name);

            return Ok(await token);
        }

        private Task<string> CreateJWTTokenAsync(string userName)
        {
            SymmetricSecurityKey key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration.GetSection("Token").GetValue<string>("SuperSecretKey")));
            SigningCredentials credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            List<Claim> claims = new List<Claim>
            {
                new Claim(ClaimsIdentity.DefaultNameClaimType, userName),
                new Claim(ClaimsIdentity.DefaultRoleClaimType, "User")
            };

            DateTime expires = DateTime.Now.AddMinutes(1);

            JwtSecurityToken jwtToken = new JwtSecurityToken(claims: claims, expires: expires, signingCredentials: credentials);

            return Task.FromResult(new JwtSecurityTokenHandler().WriteToken(jwtToken));
        }
    }
}