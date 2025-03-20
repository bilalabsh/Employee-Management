using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

//data annotation
namespace EmployeeManagement.Core.DTOs
{
    public class EmployeeDTO
    {
        public string Name { set; get; }
        [Required]
        [EmailAddress]
        public required string Email { set; get; }
        public string? Phone { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal Salary { get; set; }


        public string Password { get; set; }
    }
}
