using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.EntityFrameworkCore;
using MovieToHLS.Entities;

namespace MovieToHLS.Storage;

public class AppDBContext : DbContext
{
    public DbSet<User> Users { get; set; }
    public DbSet<Torrent> Torrents { get; set; }
    public DbSet<TorrentAccess> TorrentAccesses { get; set; }
    public AppDBContext(DbContextOptions opt) : base(opt)
    {
        Database.EnsureCreated();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AssemblyMark).Assembly);
    }
}