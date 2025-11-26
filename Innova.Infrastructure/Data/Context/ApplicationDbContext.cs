namespace Innova.Infrastructure.Data.Context;

public class ApplicationDbContext : IdentityDbContext<AppUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) 
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Apply entity configurations
        builder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
    
    public DbSet<RefreshToken> RefreshTokens { get; set; } = null!;
    public DbSet<Department> Departments { get; set; }
    public DbSet<Idea> Ideas { get; set; } = null!;
    public DbSet<Comment> Comments { get; set; } = null!;

}
