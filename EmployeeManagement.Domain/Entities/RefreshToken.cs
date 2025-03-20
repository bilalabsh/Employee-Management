using System;

namespace EmployeeManagement.Domain.Entities
{
    public class RefreshToken
    {
        public int ID { get; set; }
        public string? Username { get; set; }  
        public string? Token { get; set; } 
        public DateTime ExpiryDate { get; set; } 
    }
}   
