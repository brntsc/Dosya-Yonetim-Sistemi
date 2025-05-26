using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Uyg.API.Models;

namespace Uyg.API.Data
{
    public class AppDbContext : IdentityDbContext<AppUser, AppRole, string>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<FileModel> Files { get; set; }
        public DbSet<FileTag> FileTags { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Comment> Comments { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // FileModel - FileTag many-to-many relationship
            builder.Entity<FileModel>()
                .HasMany(f => f.FileTags)
                .WithMany(t => t.Files)
                .UsingEntity(j => j.ToTable("FileFileTags"));

            // FileModel - Category relationship
            builder.Entity<FileModel>()
                .HasOne(f => f.Category)
                .WithMany(c => c.Files)
                .HasForeignKey(f => f.CategoryId)
                .OnDelete(DeleteBehavior.SetNull);

            // FileModel - Comment relationship
            builder.Entity<Comment>()
                .HasOne(c => c.File)
                .WithMany(f => f.Comments)
                .HasForeignKey(c => c.FileId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
} 