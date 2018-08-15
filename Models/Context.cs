using Microsoft.EntityFrameworkCore;

namespace Belt.Models {

    public class Context : DbContext {
        //base() calls the parent class' constructor passing the "options" paramater along
        public Context(DbContextOptions<Context> options) : base(options) {}
        public DbSet<User> users {get;set;}
        public DbSet<Auction> auctions {get;set;}
        public DbSet<Bid> bids {get;set;}
        
    }
}