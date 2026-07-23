using Microsoft.EntityFrameworkCore;

public class KickFiveContext(DbContextOptions<KickFiveContext> options) : DbContext(options)
{
    public DbSet<KickFive.Models.Field> Field { get; set; } = default!;
}
