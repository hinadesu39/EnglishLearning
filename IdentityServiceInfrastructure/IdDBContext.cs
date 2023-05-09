using IdentityServiceDomain.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IdentityServiceInfrastructure
{
    public class IdDBContext : IdentityDbContext<User, Role, Guid>
    {
        public IdDBContext(DbContextOptions<IdDBContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfigurationsFromAssembly(this.GetType().Assembly);
            //modelBuilder.EnableSoftDeletionGlobalFilter();
            modelBuilder.Entity<User>().HasQueryFilter(p => !p.IsDeleted);

        }
    }
}
