using LctHack.Models;
using Microsoft.EntityFrameworkCore;

namespace LctHack;

public class ApplicationDbContext: DbContext
{
    public DbSet<Video> Videos { get; set; }
    public DbSet<Match> Matches { get; set; }

    public ApplicationDbContext(DbContextOptions options) : base(options){ }
}