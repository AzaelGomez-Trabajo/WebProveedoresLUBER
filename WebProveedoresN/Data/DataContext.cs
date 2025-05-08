using Microsoft.EntityFrameworkCore;
using WebProveedoresN.Models;

namespace WebProveedoresN.Data
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options)
        {
                
        }

        public DbSet<OrderModel> Orders { get; set; }
        
        public DbSet<SupplierModel> Suppliers { get; set; }
        
        public DbSet<UserModel> Usuarios { get; set; }

        public DbSet<FileModel> Files { get; set; }
        
        public DbSet<StatusModel> Status { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
    }
}
