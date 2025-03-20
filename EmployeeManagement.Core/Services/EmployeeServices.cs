using AutoMapper;
using EmployeeManagement.Common;
using EmployeeManagement.Core.DTOs;
using EmployeeManagement.Core.Interfaces;
using EmployeeManagement.Domain.Entities;
using EmployeeManagement.Domain.Interfaces;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EmployeeManagement.Core.Services
{
    public class EmployeeService : IEmployeeService
    {
        private readonly IEmployeeRepository _employeeRepository;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IMapper _mapper;

        public EmployeeService(IEmployeeRepository employeeRepository, IMapper mapper, UserManager<IdentityUser> userManager)
        {
            _employeeRepository = employeeRepository;
            _userManager = userManager;
            _mapper = mapper;
        }

        public async Task<ServiceResult<EmployeeDTO>> CreateEmployeeWithUserAsync(EmployeeDTO employeeDTO)
        {
            // Check if user already exists
            var existingUser = await _userManager.FindByEmailAsync(employeeDTO.Email);
            if (existingUser != null)
            {
                return ServiceResult<EmployeeDTO>.FailureResult("User already exists.");
            }

            // Generate a GUID and use it for both IdentityUser and Employee
            var newGuid = Guid.NewGuid();
            var user = new IdentityUser { Id = newGuid.ToString(), UserName = employeeDTO.Email, Email = employeeDTO.Email };
            var identityResult = await _userManager.CreateAsync(user, employeeDTO.Password);
            if (!identityResult.Succeeded)
            {
                return ServiceResult<EmployeeDTO>.FailureResult("Failed to create user.");
            }

            // Assign "Employee" role
            await _userManager.AddToRoleAsync(user, "Employee");

            // Create Employee entry using the same Id as IdentityUser
            var employee = _mapper.Map<Employee>(employeeDTO);
            employee.Id = newGuid;

            await _employeeRepository.AddAsync(employee);

            return ServiceResult<EmployeeDTO>.SuccessResult(_mapper.Map<EmployeeDTO>(employee), "Employee created successfully");
        }

        public async Task<ServiceResult<List<EmployeeDTO>>> GetEmployeesAsync()
        {
            var employees = await _employeeRepository.GetAllAsync();
            var employeeDtos = _mapper.Map<List<EmployeeDTO>>(employees);
            return ServiceResult<List<EmployeeDTO>>.SuccessResult(employeeDtos);
        }

        public async Task<ServiceResult<EmployeeDTO>> GetEmployeeByIdAsync(Guid id)
        {
            var employee = await _employeeRepository.GetByIdAsync(id);
            if (employee == null)
                return ServiceResult<EmployeeDTO>.FailureResult("Employee not found");

            return ServiceResult<EmployeeDTO>.SuccessResult(_mapper.Map<EmployeeDTO>(employee));
        }

        public async Task<ServiceResult<EmployeeDTO>> AddEmployeeAsync(EmployeeDTO employeeDTO)
        {
            var employee = _mapper.Map<Employee>(employeeDTO);
            await _employeeRepository.AddAsync(employee);
            return ServiceResult<EmployeeDTO>.SuccessResult(_mapper.Map<EmployeeDTO>(employee), "Employee added successfully");
        }

        public async Task<ServiceResult<EmployeeDTO>> UpdateEmployeeAsync(Guid id, EmployeeDTO employeeDto)
        {
            var employee = await _employeeRepository.GetByIdAsync(id);
            if (employee == null)
            {
                return ServiceResult<EmployeeDTO>.FailureResult("Employee not found");
            }

            _mapper.Map(employeeDto, employee); // Update entity with DTO data
            await _employeeRepository.UpdateAsync(employee);

            return ServiceResult<EmployeeDTO>.SuccessResult(_mapper.Map<EmployeeDTO>(employee), "Employee updated successfully");
        }

        public async Task<ServiceResult<bool>> DeleteEmployeeAsync(Guid id)
        {
            var employee = await _employeeRepository.GetByIdAsync(id);
            if (employee == null)
            {
                return ServiceResult<bool>.FailureResult("Employee not found");
            }

            await _employeeRepository.DeleteAsync(id);
            return ServiceResult<bool>.SuccessResult(true, "Employee deleted successfully");
        }

        public async Task<Employee?> GetEmployeeByEmailAsync(string email)
        {
            return await _employeeRepository.GetEmployeeEntityByEmailAsync(email);
        }
    }
}
