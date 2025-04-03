using EmployeeManagement.Core.DTOs;
using EmployeeManagement.Core.Interfaces;
using EmployeeManagement.Core.Services;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace EmployeeManagement.Web.Controllers
{
    public class AuthenticationController : Controller
    {
        private readonly IAuthService _authService;
        private readonly IEmployeeService _employeeService;

        public AuthenticationController(IAuthService authService, IEmployeeService employeeService)
        {
            _authService = authService;
            _employeeService = employeeService;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginDTO model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var (success, message, accessToken, refreshToken) = await _authService.LoginAsync(model);

            if (!success)
            {
                ModelState.AddModelError("", message);
                return View(model);
            }

            // Store the token 
            HttpContext.Session.SetString("AccessToken", accessToken);
            HttpContext.Session.SetString("RefreshToken", refreshToken);

            return RedirectToAction("Index", "Home"); // Redirect to home after successful login
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(EmployeeDTO model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var result = await _employeeService.CreateEmployeeWithUserAsync(model);

            if (!result.Success)
            {
                ModelState.AddModelError("", result.Message);
                return View(model);
            }

            TempData["SuccessMessage"] = "Registration successful! You can now login.";
            return RedirectToAction("Login");
        }
    }
}
