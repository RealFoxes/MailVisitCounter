using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MailVisitCounter.Models
{
    public class MSSQlContext : DbContext
    {
        public DbSet<Door> Doors { get; set; }
        public DbSet<Stat> Stats { get; set; }
        public DbSet<MallVisitors> MallVisitors { get; set; }
        public MSSQlContext(DbContextOptions<MSSQlContext> options)
            : base(options)
        {
            Database.EnsureCreated();  
        }
    }
}
