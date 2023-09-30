﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;




namespace jae.Areas.Identity.Data;


public class jaeUser : IdentityUser
{
    
    public string? FirstName { get; set; }

    public string? LastName { get; set; }
    public string? Code { get; set; }
}

