using Microsoft.EntityFrameworkCore;

namespace UserManagementAPI.Data;

public class UserDbContext : DbContext
{
    public UserDbContext(DbContextOptions<UserDbContext> options)
        : base(options)
    {
    }
}
