using System.Data.Entity;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace EnterpriseDataPipeline.Models
{
    // You can add profile data for the user by adding more properties to your ApplicationUser class, please visit http://go.microsoft.com/fwlink/?LinkID=317594 to learn more.
    public class ApplicationUser : IdentityUser
    {
        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser> manager)
        {
            // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
            // Add custom user claims here
            return userIdentity;
        }
    }

    //Added by Anthony Lai on 2015-08-04
    public class ApplicationRole : IdentityRole
    {
        public ApplicationRole() : base() { }
        public ApplicationRole(string name) : base(name) { }
        public string Description { get; set; }
    }

    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext()
            : base("DefaultConnection", throwIfV1Schema: false)
        {
        }

        //Added by Anthony Lai on 2015-08-04
        public DbSet<Storage> Storage { get; set; }
        public DbSet<JobTB> JobTB { get; set; }
        public DbSet<JobStatus> JobStatus { get; set; }
        public DbSet<JobServer> JobServer { get; set; }

        //Added by Anthony Lai on 2015-08-24
        //For installation of module using puppet
        public DbSet<ModuleStatus> ModuleStatus { get; set; }
        public DbSet<ModuleTB> ModuleTB { get; set; }
        public DbSet<PuppetServer> PuppetServer { get; set; }

        //Added by Anthony Lai on 2015-09-09
        //For installation of module using puppet where module server is the server the selected module to be installed
        public DbSet<ModuleServer> ModuleServer { get; set; }


        //Added by Anthony Lai on 2016-02-13
        //For installation of module using puppet where module server is the server the selected module to be installed
        public DbSet<KafkaServer> KafkaServer { get; set; }

        public DbSet<KafkaTopics> KafkaTopics { get; set; }


        //Added by Anthony Lai on 2016-09-07
        //For dataset to store potential customer
        public DbSet<PotentialCustomer> PotentialCustomer { get; set; }


        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }

        //protected override void OnModelCreating(DbModelBuilder modelBuilder)
        //{
        //    base.OnModelCreating(modelBuilder);

        //    //modelBuilder.Entity<Microsoft.AspNet.Identity.EntityFramework.IdentityRole>()
        //    //    .Property(c => c.Name).HasMaxLength(128).IsRequired();

        //    //modelBuilder.Entity<Microsoft.AspNet.Identity.EntityFramework.IdentityUser>().ToTable("AspNetUsers")//I have to declare the table name, otherwise IdentityUser will be created
        //    //    .Property(c => c.UserName).HasMaxLength(128).IsRequired();

        //    modelBuilder.Entity<Microsoft.AspNet.Identity.EntityFramework.IdentityRole>()
        //        .Property(c => c.Name).HasMaxLength(256).IsRequired();

        //    modelBuilder.Entity<Microsoft.AspNet.Identity.EntityFramework.IdentityUser>().ToTable("AspNetUsers")//I have to declare the table name, otherwise IdentityUser will be created
        //        .Property(c => c.UserName).HasMaxLength(256).IsRequired();
        //}
    }
}