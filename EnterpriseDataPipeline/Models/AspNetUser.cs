using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;

using System.ComponentModel;
using System.Web.Mvc;
using DynamicMVC.Business.Attributes;

namespace DynamicMVC1.Models
{
    [DynamicEntity]
    [DynamicMenuItem("User", "User")]
    [Bind(Exclude = "Id")]
    public class AspNetUser
    {
        [ScaffoldColumn(false)]
        public string Id { get; set; }

        [StringLength(256)]
        public string Email { get; set; }

        public bool EmailConfirmed { get; set; }

        public string PasswordHash { get; set; }

        public string SecurityStamp { get; set; }

        public string PhoneNumber { get; set; }

        public bool PhoneNumberConfirmed { get; set; }

        public bool TwoFactorEnabled { get; set; }

        public DateTime? LockoutEndDateUtc { get; set; }

        public bool LockoutEnabled { get; set; }

        public int AccessFailedCount { get; set; }

        [Required]
        [StringLength(256)]
        public string UserName { get; set; }
    }
}
