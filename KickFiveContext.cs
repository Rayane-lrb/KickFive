using Microsoft.EntityFrameworkCore;
using KickFive.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

public class KickFiveContext(DbContextOptions<KickFiveContext> options) : IdentityDbContext<User>(options)
{
    public DbSet<KickFive.Models.Field> Field { get; set; } = default!;
}
