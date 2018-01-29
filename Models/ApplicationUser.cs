using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ForeverFrameChat.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string Login { get; set; }
        public ApplicationUser()
        {
        }
    }
}