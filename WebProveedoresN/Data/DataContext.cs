using Microsoft.EntityFrameworkCore;
using WebProveedoresN.Models;
using WebProveedoresN.Entities;

namespace WebProveedoresN.Data
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options)
        {
                
        }

        public DbSet<Order> Orders { get; set; }
        
        public DbSet<Supplier> Suppliers { get; set; }
        
        public DbSet<Usuario> Usuarios { get; set; }

        public DbSet<FileDTO> Files { get; set; }
        
        public DbSet<StatusDTO> Status { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
    }
}
