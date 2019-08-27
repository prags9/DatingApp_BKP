using System.Security.Claims;
using System.Threading.Tasks;
using DatingApp.api.Data;
using DatingApp.api.DTOs;
using DatingApp.api.Models;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Configuration;
using System;
using System.IdentityModel.Tokens.Jwt;

namespace DatingApp.api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthRepository _repo;
        private readonly IConfiguration _config;
        public AuthController(IAuthRepository repo, IConfiguration config)
        {
            _config = config;
            _repo= repo;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(UserForRegisterDto userForRegisterDto){
            //If [ApiControlle] isnotusedthen wehave tomanually validate the state

            if(!ModelState.IsValid)
                return BadRequest(ModelState);

            //valiate user

            userForRegisterDto.Username = userForRegisterDto.Username.ToLower(); 
            if(await _repo.UserExists(userForRegisterDto.Username))
            return BadRequest("user alraedy exists");

            var userToCreate = new User{
                Username = userForRegisterDto.Username
            };
            var createdUser = await _repo.Register(userToCreate, userForRegisterDto.Password);
            return StatusCode(201);

        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(UserForLoginDto userForLoginDto){

            var userFromRepo = await _repo.Login(userForLoginDto.Username.ToLower(), userForLoginDto.Password);

            if(userFromRepo==null)
                return Unauthorized();

            //Thisis used for craeting token, 2 bits of info about user ie Id and Username.
            //The token will be passed backto the user so that each time wedont have to lookup into the databaseto validate.
            var claims = new[]{
                new Claim(ClaimTypes.NameIdentifier, userFromRepo.Id.ToString()),
                new Claim(ClaimTypes.Name, userFromRepo.Username)
            };

            //_config isused to configure in AppSettings.json file.
            var key = new SymmetricSecurityKey
                        (Encoding.UTF8.GetBytes(_config.GetSection("AppSettings:Token").Value));

            //Credentials. Encryptinh the key with hashing algorithm
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256Signature);

            var tokenDescriptor= new SecurityTokenDescriptor{
                Subject = new ClaimsIdentity(claims), 
                Expires = DateTime.Now.AddDays(1),
                SigningCredentials = creds
            };
            
            //using this we can craete a token
            var tokenhandler = new JwtSecurityTokenHandler();
            var token = tokenhandler.CreateToken(tokenDescriptor);

            return Ok(new {
                token = tokenhandler.WriteToken(token)
            });
    }
    }
}