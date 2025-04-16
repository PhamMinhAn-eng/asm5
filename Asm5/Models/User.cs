using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;

namespace ASM5.Models;

public class User
{
    
        public string Id { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public List<string> Roles { get; set; }
    public string Address { get; set; }
    public string PhoneNumber { get; set; }
    public string TinhTrangHoatDong { get; set; }
 


}
