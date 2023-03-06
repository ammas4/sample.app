using Microsoft.EntityFrameworkCore;

namespace AppSample.Api;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }
}