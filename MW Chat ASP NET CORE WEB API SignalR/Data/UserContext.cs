using Microsoft.EntityFrameworkCore;
using MW_Chat_ASP_NET_CORE_WEB_API_SignalR.Models;

namespace MW_Chat_ASP_NET_CORE_WEB_API_SignalR.Data
{
    public class UserContext : DbContext
    {
        public DbSet<User> Users { get; set; }

        public UserContext(DbContextOptions options) : base(options)
        {
            Database.EnsureCreated();
        }
    }
}
