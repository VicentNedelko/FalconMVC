using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FalconMVC.Models
{
    public class DbFalcon : DbContext
    {
        DbSet<User> Users { get; set; }
        public DbFalcon(DbContextOptions<DbFalcon> options) : base (options)
        {
        }
    }
}
