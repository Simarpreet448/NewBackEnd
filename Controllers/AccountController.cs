using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using NewBackEnd.Data.Models;
using NewBackEnd.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NewBackEnd.Controllers
{
    [ApiController]
    [Route("account")]
    public class AccountController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;

        public AccountController(UserManager<IdentityUser> userManager,SignInManager<IdentityUser> signInManager,IConfiguration configuration)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        [Route("current")]
        [AllowAnonymous]
        public async Task<IActionResult> Current()
        {
            var appUser = await _userManager.GetUserAsync(User);
            if (appUser == null)
                return Ok(new { message = "No user" });

            string roleName = _userManager.GetRolesAsync(appUser).Result.FirstOrDefault();
            return Ok(
                new
                {
                    name = appUser.Email,
                    email = appUser.Email,
                    role = roleName
                });
        }

        [HttpPost]
        [Route("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] UserInfo user)
        {
            if (ModelState.IsValid)
            {
                IdentityUser appUser = await _userManager.FindByEmailAsync(user.Email);
                if (appUser != null)
                {
                    //await signInManager.SignOutAsync();
                    Microsoft.AspNetCore.Identity.SignInResult result = await _signInManager
                        .PasswordSignInAsync(appUser, user.Password, true, false);
                    if (result.Succeeded)
                        return Ok(appUser);
                    else
                        return BadRequest(new { message = "Login Failed: Invalid Email or Password" });
                }
                else
                    return BadRequest(new { message = "Login Failed: Invalid Email or Password" });
            }
            else
                return BadRequest(ModelStateErrors());
        }


        [HttpPost]
        [Route("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] UserInfo user)
        {
            if (ModelState.IsValid)
            {
                var appUser = new IdentityUser
                {
                    UserName = user.Email,
                    Email = user.Email
                };
                IdentityResult result = await _userManager.CreateAsync(appUser, user.Password);
                if (result.Succeeded)
                    return Ok();
                else
                    return BadRequest(new { message = result.Errors });
            }
            else
                return BadRequest(ModelStateErrors());
        }

        [Route("logout")]
        [AllowAnonymous]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return Ok(new { message = "Logged Out" });
        }

        private IEnumerable<string> ModelStateErrors()
        {
            return ModelState.Values.SelectMany(v => v.Errors.Select(b => b.ErrorMessage));
        }

    }
}
