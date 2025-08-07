using Microsoft.EntityFrameworkCore;

namespace Assignment.Models
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }
        public DbSet<Users> Users { get; set; }
        public DbSet<Categories> Categories { get; set; }
        public DbSet<Products> Products { get; set; }
        public DbSet<Vouchers> Vouchers { get; set; }
        public DbSet<Files> Files { get; set; }
        public DbSet<Orders> Orders { get; set; }
        public DbSet<OrderDetail> OrderDetails { get; set; }
        public DbSet<Evaluates> Evaluates { get; set; }
        public DbSet<Refund> Refunds { get; set; }
        public DbSet<Redeems> Redeems { get; set; }
        public DbSet<ForgotPassword> ForgotPasswords { get; set; }
        public DbSet<VerifyAccount> VerifyAccounts { get; set; }
        public DbSet<ChatRoom> ChatRooms { get; set; }
        public DbSet<ChatMessage> ChatMessages { get; set; }
    }
}
