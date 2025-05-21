using Microsoft.EntityFrameworkCore;
using FileStoringService.Models;

namespace FileStoringService.Data
{
    public class FileDbContext : DbContext
    {
        public FileDbContext(DbContextOptions<FileDbContext> options) : base(options) { }

        public DbSet<StoredFile> Files => Set<StoredFile>();
    }
}