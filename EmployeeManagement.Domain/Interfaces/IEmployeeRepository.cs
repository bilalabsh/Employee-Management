using EmployeeManagement.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmployeeManagement.Domain.Interfaces
{
    public interface IEmployeeRepository
    {
        Task<IEnumerable<Employee>> GetAllAsync();  
        Task<Employee> GetByIdAsync(Guid id);
        Task AddAsync(Employee employee);

        Task UpdateAsync(Employee employee);
        Task DeleteAsync(Guid id);
        Task<Employee?> GetEmployeeEntityByEmailAsync(string email);
    }
}
