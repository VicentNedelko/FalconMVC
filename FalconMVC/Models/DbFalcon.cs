using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FalconMVC.Models
{
    public class DbFalcon : IdentityDbContext<User>
    {
        public DbSet<GA> GAs { get; set; }
        public DbFalcon(DbContextOptions<DbFalcon> options) : base (options)
        {
        }
    }
}
