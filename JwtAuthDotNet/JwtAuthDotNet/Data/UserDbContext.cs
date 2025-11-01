using JwtAuthDotNet.Model;
using Microsoft.EntityFrameworkCore;

namespace JwtAuthDotNet.Data
{
    public class UserDbContext(DbContextOptions<UserDbContext> options) : DbContext(options)
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
    }
}
