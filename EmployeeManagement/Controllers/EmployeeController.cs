using EmployeeManagement.Common;
using EmployeeManagement.Core.DTOs;
using EmployeeManagement.Core.Interfaces;
using EmployeeManagement.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Runtime.CompilerServices;

namespace EmployeeManagement.API.Controllers
{
    [Authorize]

    [Route("api/[controller]")]
    [ApiController]
    public class EmployeeController: ControllerBase
    {   
        private readonly IEmployeeService _employeeService;
        private readonly UserManager<IdentityUser> _userManager;
        public EmployeeController(IEmployeeService employeeService , UserManager<IdentityUser> userManager)
        {
            _employeeService = employeeService;
            _userManager = userManager;
        }
        [Authorize(Roles = "Employee,Admin")]
        [HttpGet]
        [Route("GetEmployees")]
        public async Task<IActionResult> GetEmployees()
        {
            var result = await _employeeService.GetEmployeesAsync();
            if (!result.Success)
                return BadRequest(result.Message);

            return Ok(new { result.Data, result.Message });
        }
        [Authorize(Roles = "Employee,Admin")]
        [HttpGet]
        [Route("GetEmployeeById/{id}")]
        public async Task<IActionResult> GetEmployeeById(Guid id)
        {
            var result = await _employeeService.GetEmployeeByIdAsync(id);
            if (!result.Success)
                return NotFound(result.Message);

            return Ok(result.Data);
        }


        [Authorize(Roles = "Admin")]
        [HttpPost]
        [Route("createEmployee")]
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [Route("CreateEmployeeWithUser")]
        public async Task<IActionResult> CreateEmployeeWithUserAsync([FromBody] EmployeeDTO employeeDTO)
        {
            var result = await _employeeService.CreateEmployeeWithUserAsync(employeeDTO);

            if (!result.Success)
                return BadRequest(new { result.Message });

            return Ok(new { result.Data, result.Message });
        }
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [Route("AddEmployee")]
        public async Task<IActionResult> AddEmployee(EmployeeDTO employeeDTO)
        {
            var result = await _employeeService.AddEmployeeAsync(employeeDTO);
            if (!result.Success)
                return BadRequest(result.Message);

            return Ok(new { result.Data, result.Message });
        }
        [Authorize(Roles = "Admin")]
        [HttpPut]
        [Route("UpdateEmployee/{id}")]
        public async Task<IActionResult> UpdateEmployee(Guid id, EmployeeDTO employeeDTO)
        {
            var result = await _employeeService.UpdateEmployeeAsync(id, employeeDTO);
            if (!result.Success)
                return NotFound(result.Message);

            return Ok(new { result.Data, result.Message });
        }
        [Authorize(Roles = "Admin")]
        [HttpDelete]
        [Route("DeleteEmployee/{id}")]
        public async Task<IActionResult> DeleteEmployee(Guid id)
        {
            var result = await _employeeService.DeleteEmployeeAsync(id);
            if (!result.Success)
                return NotFound(result.Message);

            return Ok(result.Message);
        }

        [HttpGet]
        [Route("GetEmployeeByEmail/{email}")]
        public async Task<IActionResult> GetEmployeeByEmail(string email)
        {
            var employee = await _employeeService.GetEmployeeByEmailAsync(email);
            if (employee is null)
            {
                return NotFound();
            }
            return Ok(employee);
        }
    }
}
     