using AngularAuthAPI.Enums;
using AngularAuthAPI.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace AngularAuthAPI.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            #region seed roles data
            List<string> guids = new List<string>();
            for (int i = 0; i < 8; i++)
                guids.Add(Guid.NewGuid().ToString());

            //seed all roles
            int x = 0;
            foreach (Roles role in Enum.GetValues(typeof(Roles)))
            {
                builder.Entity<IdentityRole>().HasData(new IdentityRole()
                {
                    Id = guids[x++],
                    Name = role.ToString(),
                    NormalizedName = role.ToString().ToUpper()
                });
            }

            #endregion

            #region seed super user
            ApplicationUser user = new ApplicationUser()
            {
                Id = guids[7],
                FirstName = "super",
                LastName = "admin",
                UserName = "superadmin",
                NormalizedUserName = "SUPERADMIN",
                Email = "admin@gmail.com",
                NormalizedEmail = "ADMIN@GMAIL.COM",
                EmailConfirmed = true
            };
            PasswordHasher<ApplicationUser> hasher = new PasswordHasher<ApplicationUser>();
            string hash = hasher.HashPassword(user, "admin@test");
            user.PasswordHash = hash;
            builder.Entity<ApplicationUser>().HasData(user);

            //assign super admin role
            builder.Entity<IdentityUserRole<String>>().HasData(new IdentityUserRole<string>()
            {
                RoleId = guids[0],
                UserId = user.Id
            });


            #endregion
        }
    }
}