using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ApiThreeTierPhotoGallery.Infrastructure.Repositories.Abstract;
using ApiThreeTierPhotoGallery.Infrastructure.Services.Abstract;
using ApiThreeTierPhotoGallery.ViewModels;
using ApiThreeTierPhotoGallery.Infrastructure.Core;
using ApiThreeTierPhotoGallery.Entities;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Cookies;

// For more information on enabling MVC for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace ApiThreeTierPhotoGallery.Controllers
{
    [Route("api/[controller]")]
    public class AccountController : Controller
    {
        private readonly IMembershipService _membershipService;
        private readonly IUserRepository _userRepository;
        private readonly ILoggingRepository _loggingRepository;

        public AccountController(IMembershipService membershipService,
            IUserRepository userRepository,
            ILoggingRepository _errorRepository)
        {
            _membershipService = membershipService;
            _userRepository = userRepository;
            _loggingRepository = _errorRepository;
        }
        

        /// <summary>
        /// 登录
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        [HttpPost("authenticate")]
        public async Task<IActionResult> Login([FromBody]LoginViewModel user)
        {
            //ObjectResult是ActionResult的子类
            IActionResult _result = new ObjectResult(false);
            GenericResult _authenticaitonResult = null;

            try
            {
                //先来验证用户
                MembershipContext _userMemberContext = _membershipService.ValidateUser(user.Username, user.Password);

                if (_userMemberContext != null)
                {
                    //获取用户所有角色，根据用户名
                    IEnumerable<Role> _roles = _userRepository.GetUserRoles(user.Username);

                    //根据用户的角色收集到所有Claim
                    List<Claim> _claims = new List<Claim>();
                    foreach(Role role in _roles)
                    {
                        //本来第二个形参应该是role.Name,这里默认设置为Admin
                        Claim _claim = new Claim(ClaimTypes.Role, "Admin", ClaimValueTypes.String, user.Username);
                        _claims.Add(_claim);
                    }

                    //用户所有claim搜集好，那就开始登录
                    await HttpContext.Authentication.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                        new ClaimsPrincipal(new ClaimsIdentity(_claims, CookieAuthenticationDefaults.AuthenticationScheme)), new Microsoft.AspNetCore.Http.Authentication.AuthenticationProperties { IsPersistent = user.RememberMe });

                    _authenticaitonResult = new GenericResult() {
                        Succeeded =true,
                        Message="Authentication succeeded"
                    };
                }
                else
                {
                    _authenticaitonResult = new GenericResult()
                    {
                        Succeeded = false,
                        Message = "Authentication failed"
                    };
                }
            }
            catch (Exception ex)
            {
                _authenticaitonResult = new GenericResult()
                {
                    Succeeded = false,
                    Message = ex.Message
                };

                _loggingRepository.Add(new Error() {
                    Message = ex.Message,
                    StackTrace = ex.StackTrace, 
                    DateCreated=DateTime.Now
                });
                _loggingRepository.Commit();
            }

            _result = new ObjectResult(_authenticaitonResult);
            return _result;
        }


        /// <summary>
        /// 登出
        /// </summary>
        /// <returns></returns>
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            try
            {
                await HttpContext.Authentication.SignOutAsync("Cookies");
                return Ok();
            }
            catch (Exception ex)
            {
                _loggingRepository.Add(new Error() {
                    Message = ex.Message,
                    StackTrace = ex.StackTrace,
                    DateCreated=DateTime.Now
                });

                _loggingRepository.Commit();
                return BadRequest();
            }
        }



        /// <summary>
        /// 注册
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        [Route("register")]
        [HttpPost]
        public IActionResult Register([FromBody]RegistrationViewModel user)
        {
            IActionResult _result = new ObjectResult(false);
            GenericResult _registrationResult = null;

            try
            {
                if(ModelState.IsValid)
                {
                    //这里我想知道CreateUser的本质是什么？
                    User _user = _membershipService.CreateUser(user.Username, user.Email, user.Password, new int[] { 1 });

                    if(_user!=null)
                    {
                        _registrationResult = new GenericResult() {
                            Succeeded = true,
                            Message="Registration secceeded"
                        };
                    }
                }
                else
                {
                    _registrationResult = new GenericResult()
                    {
                        Succeeded = false,
                        Message = "Invalid fields"
                    };
                }
            }
            catch (Exception ex)
            {

                _registrationResult = new GenericResult()
                {
                    Succeeded = false,
                    Message = ex.Message
                };

                _loggingRepository.Add(new Error() {
                    Message = ex.Message,
                    StackTrace=ex.StackTrace,
                    DateCreated=DateTime.Now
                });
            }


            _result = new ObjectResult(_registrationResult);
            return _result;
        }
    }
}
