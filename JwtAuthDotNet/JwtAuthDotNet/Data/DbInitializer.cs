using JwtAuthDotNet.Model;
using Microsoft.EntityFrameworkCore;

namespace JwtAuthDotNet.Data
{
    public static class DbInitializer
    {
        public static async Task SeedRolesAsync(UserDbContext context)
        {
            if (!await context.Roles.AnyAsync())
            {
                context.Roles.AddRange(
                    new Role { Name = "User" },
                    new Role { Name = "Admin" },
                    new Role { Name = "Manager" }
                );
                await context.SaveChangesAsync();
            }
        }

    }
}
