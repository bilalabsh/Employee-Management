using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class Employee
{
    public Guid Id { get; set; }  // Same ID as AspNetUsers

    public required string Name { get; set; }
    public required string Email { get; set; }
    public string? Phone { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal Salary { get; set; }
}
