using Microsoft.EntityFrameworkCore;
using KickFive.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

public class KickFiveContext(DbContextOptions<KickFiveContext> options) : IdentityDbContext<User>(options)
{
    public DbSet<KickFive.Models.Field> Field { get; set; } = default!;
    public DbSet<KickFive.Models.User> User { get; set; } = default!;
    public DbSet<KickFive.Models.Booking> Booking { get; set; } = default!;
    public DbSet<KickFive.Models.Review> Review { get; set; } = default!;   
}
