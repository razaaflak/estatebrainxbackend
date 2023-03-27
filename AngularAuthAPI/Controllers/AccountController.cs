using AngularAuthAPI.Classes;
using AngularAuthAPI.Dtos;
using AngularAuthAPI.Helpers;
using AngularAuthAPI.Models;
using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Net.Mail;

namespace AngularAuthAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {

        private const string AuthSchemes = JwtBearerDefaults.AuthenticationScheme;

        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<AccountController> _logger;
        public IConfiguration _config;
        public IMapper _mapper;

        public AccountController(
                SignInManager<ApplicationUser> signInManager,
                UserManager<ApplicationUser> userManager,
                ILogger<AccountController> logger,
                IConfiguration config,
                 IMapper mapper
            )
        {
            _signInManager= signInManager;
            _userManager= userManager;
            _logger= logger;
            _config = config;
            _mapper = mapper;

        }

        [HttpGet]
        public IActionResult Index()
        {
            return Ok("we will return view from here");
        }

        [HttpPost("login")]
        public async Task<IActionResult> LoginAsync(UserLoginDto userLoginDto)
        {
            string userName = userLoginDto.Email;
            ApplicationUser appUser = null;

            //userDto.Email might contain email or username
            if (Utilities.IsValidEmailAddress(userLoginDto.Email))
            {
                appUser = await _userManager.FindByEmailAsync(userLoginDto.Email);
                if (appUser != null)
                {
                    userName = appUser.UserName;
                }
            }
            else
            {
                appUser = await _userManager.FindByNameAsync(userLoginDto.Email);

            }
            
            Microsoft.AspNetCore.Identity.SignInResult result = await _signInManager.PasswordSignInAsync(userName,userLoginDto.Password,false,false);
            if (result.Succeeded)
            {
                
                ICollection<string> userRoles = await _userManager.GetRolesAsync(appUser);
                String token = Utilities.GenerateToken(
                    _config["Jwt:Key"],
                    _config["Jwt:Issuer"],
                    _config["Jwt:Audience"],
                    appUser,
                    userRoles
                    );

                UserDto dto = _mapper.Map<UserDto>(appUser);
                dto.Roles = userRoles.ToArray();

                UserResponseDto response = new UserResponseDto() { user = dto, token = token };
               
                return Ok(new Response<UserResponseDto>(response));
            }
            else
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    Classes.Response<UserLoginDto>.GetError<UserLoginDto>(
                        userLoginDto,
                        "Cannot login | Invalid credentials"
                        ));
            }
        }


        [HttpPost("register")]
        public async Task<IActionResult> RegisterAsync(UserRegisterDto userDto)
        {

            var user = await _userManager.FindByEmailAsync(userDto.Email);

            if (user != null)
            {
                return StatusCode(
                        StatusCodes.Status500InternalServerError,
                        Classes.Response<UserRegisterDto>.GetError<UserRegisterDto>(userDto,"Email already taken")
                    ); 
            }

            //create new user here
            MailAddress mail = new MailAddress(userDto.Email);
            ApplicationUser applicationUser = new ApplicationUser()
            {
                FirstName = userDto.FirstName,
                LastName = userDto.LastName,
                Email= userDto.Email,
                UserName = mail.User
            };

            IdentityResult resutl = await _userManager.CreateAsync(applicationUser, userDto.Password);
            if(resutl.Succeeded)
            {
                _logger.LogInformation("User created a new account with password.");
                await _userManager.AddToRoleAsync(applicationUser, Enums.Roles.Guest.ToString());

                ApplicationUser appUser = await _userManager.FindByEmailAsync(userDto.Email);
                ICollection<string> userRoles = await _userManager.GetRolesAsync(appUser);
                String token = Utilities.GenerateToken(
                    _config["Jwt:Key"],
                    _config["Jwt:Issuer"],
                    _config["Jwt:Audience"],
                    applicationUser,
                    userRoles
                    );

                UserDto dto = _mapper.Map<UserDto>(appUser);
                dto.Roles = userRoles.ToArray();

                UserResponseDto response = new UserResponseDto() { user = dto, token = token };
                //generate jwt token and return back here

                return Ok(new Response<UserResponseDto>(response));

            
            }
            else
            {
                return StatusCode(
                        StatusCodes.Status500InternalServerError,
                        Classes.Response<UserRegisterDto>.GetError<UserRegisterDto>(
                            userDto, 
                            "Cannot create user", 
                            resutl.Errors.Select(x => x.Description).ToArray()
                        )
                    );
            }

            
        }

        [Authorize(AuthenticationSchemes = AuthSchemes)]
        [HttpGet("profile")]
        public async Task<IActionResult> ProfileAsync()
        {

            ApplicationUser user = await _userManager.FindByNameAsync(this.User?.Identity?.Name);
            var userRoles = await _userManager.GetRolesAsync(user);

            UserDto userDto = _mapper.Map<UserDto>(user);
            userDto.Roles = userRoles.ToArray();

            return Ok(new Response<UserDto>(userDto));
        }
    }
}
