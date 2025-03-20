using EmployeeManagement.Common;
using EmployeeManagement.Core.DTOs;
using EmployeeManagement.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmployeeManagement.Core.Interfaces
{
    public interface IEmployeeService
    {
        Task<ServiceResult<List<EmployeeDTO>>> GetEmployeesAsync();
        Task<ServiceResult<EmployeeDTO>> GetEmployeeByIdAsync(Guid id);
        Task<ServiceResult<EmployeeDTO>> AddEmployeeAsync(EmployeeDTO employeeDTO);
        Task<ServiceResult<EmployeeDTO>> UpdateEmployeeAsync(Guid id, EmployeeDTO employeeDto);
        Task<ServiceResult<bool>> DeleteEmployeeAsync(Guid id);
        Task<Employee> GetEmployeeByEmailAsync(string email);
        Task<ServiceResult<EmployeeDTO>> CreateEmployeeWithUserAsync(EmployeeDTO employeeDTO);

    }
}
