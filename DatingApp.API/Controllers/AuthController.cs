using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using DatingApp.API.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using DatingApp.API.Data;
using DatingApp.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;

namespace DatingApp.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [AllowAnonymous]
    public class AuthController : ControllerBase
    {
        // private readonly IAuthRepository _repo;
        private readonly IConfiguration _config;
        private readonly IMapper _mapper;
        private readonly SignInManager<User> _signInManager;
        private readonly UserManager<User> _userManager;

        public AuthController(IConfiguration config, IMapper mapper, UserManager<User> userManager, SignInManager<User> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _mapper = mapper;
            _config = config;
            // _repo = repo;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(UserForRegisterDto userForRegisterDto)
        {
            // userForRegisterDto.Username = userForRegisterDto.Username.ToLower();

            // if (await _repo.UserExists(userForRegisterDto.Username))
            // {
            //     return BadRequest("Username already exists");
            // }
        
            var userToCreate = _mapper.Map<User>(userForRegisterDto);

            var result = await _userManager.CreateAsync(userToCreate, userForRegisterDto.Password);

            // var createdUser = await _repo.Register(userToCreate, userForRegisterDto.Password);

            var userToReturn = _mapper.Map<UserForDetailedDto>(userToCreate);

            if (result.Succeeded)
            {
                return CreatedAtRoute("GetUser", new { controller = "Users", id = userToCreate.Id }, userToReturn);
            }

            return BadRequest(result.Errors);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(UserForLoginDto userForLoginDto)
        {
            // var userFromRepo = await _repo.Login(userForLoginDto.Username.ToLower(), userForLoginDto.Password);

            var identityManagerUser = await _userManager.FindByNameAsync(userForLoginDto.Username);

            if (identityManagerUser == null)
            {
                return Unauthorized("Invalid user/password combination");
            }

            var result = await _signInManager.CheckPasswordSignInAsync(identityManagerUser, userForLoginDto.Password, false);

            // if (userFromRepo == null)
            // {
            //     return Unauthorized();
            // }

            if (result.Succeeded)
            {
                var user = _mapper.Map<UserForListDto>(identityManagerUser);

                return Ok(new
                {
                    token = GenerateJwtToken(identityManagerUser).Result,
                    user
                });
            }

            return Unauthorized("Invalid user/password combination");
        }

        private async Task<string> GenerateJwtToken(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.UserName)
            };

            var roles = await _userManager.GetRolesAsync(user);

            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }


            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config.GetSection("AppSettings:Token").Value));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.Now.AddDays(1),
                SigningCredentials = creds
            };

            var tokenHandler = new JwtSecurityTokenHandler();

            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }

    }
}