namespace Remp.DataAccess.Data;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Remp.Models.Entities;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext (DbContextOptions<ApplicationDbContext> options): base(options)
    {

    }

    public DbSet<ListingCase> ListingCases { get; set; }
    public DbSet<CaseContact> CaseContacts { get; set; }
    public DbSet<MediaAsset> MediaAssets { get; set; }
    public DbSet<Agent> Agents { get; set; }
    public DbSet<PhotographyCompany> PhotographyCompanies { get; set; }
    public DbSet<AgentListingCase> AgentListingCases { get; set; }  
    public DbSet<AgentPhotographyCompany> AgentPhotographyCompanies { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<CaseContact>().HasKey(contact => contact.ContactId);
        modelBuilder.Entity<AgentListingCase>().HasKey(link => new {link.AgentId, link.ListingCaseId});
        modelBuilder.Entity<AgentPhotographyCompany>().HasKey(link => new {link.AgentId, link.PhotographyCompanyId});
        
        modelBuilder.Entity<CaseContact>().HasOne(contact =>contact.ListingCase)
                                          .WithMany(listing => listing.CaseContacts)
                                          .HasForeignKey(contact => contact.ListingCaseId)
                                          .OnDelete(DeleteBehavior.NoAction);
    
        modelBuilder.Entity<MediaAsset>().HasOne(media => media.ListingCase)
                                         .WithMany(listing => listing.MediaAssets)
                                         .HasForeignKey(media => media.ListingCaseId)
                                         .OnDelete(DeleteBehavior.NoAction);
    
        modelBuilder.Entity<AgentListingCase>().HasOne(link => link.Agent)
                                               .WithMany(agent => agent.AgentListingCases)
                                               .HasForeignKey(link => link.AgentId)
                                               .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<AgentListingCase>().HasOne(link => link.ListingCase)
                                               .WithMany(listing => listing.AgentListingCases)
                                               .HasForeignKey(link => link.ListingCaseId)
                                               .OnDelete(DeleteBehavior.NoAction);                                       
    
        modelBuilder.Entity<AgentPhotographyCompany>().HasOne(link => link.Agent)
                                                      .WithMany(agent => agent.AgentPhotographyCompanies)
                                                      .HasForeignKey(link => link.AgentId)
                                                      .OnDelete(DeleteBehavior.NoAction);
    
        modelBuilder.Entity<AgentPhotographyCompany>().HasOne(link => link.PhotographyCompany)
                                                      .WithMany(company => company.AgentPhotographyCompanies)
                                                      .HasForeignKey(link => link.PhotographyCompanyId)
                                                      .OnDelete(DeleteBehavior.NoAction);
    
        modelBuilder.Entity<ListingCase>().HasOne(listing => listing.User)
                                          .WithMany(user => user.CreatedListingCases)
                                          .HasForeignKey(listing => listing.UserId)
                                          .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<MediaAsset>().HasOne(media => media.User)
                                          .WithMany(user => user.UploadedMediaAssets)
                                          .HasForeignKey(media => media.UserId)
                                          .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<Agent>().HasOne(agent => agent.User)
                                    .WithOne(user => user.Agent)
                                    .HasForeignKey<Agent>(agent => agent.Id)
                                    .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<PhotographyCompany>().HasOne(company => company.User)
                                    .WithOne(user => user.PhotographyCompany)
                                    .HasForeignKey<PhotographyCompany>(company => company.Id)
                                    .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<ListingCase>().Property(listing => listing.Longitude).HasPrecision(9, 6);
        modelBuilder.Entity<ListingCase>().Property(listing => listing.Latitude).HasPrecision(9, 6);

        modelBuilder.Entity<ListingCase>().Property(listing => listing.Title).HasMaxLength(200);
        modelBuilder.Entity<ListingCase>().Property(listing => listing.Street).HasMaxLength(250);
        modelBuilder.Entity<ListingCase>().Property(listing => listing.City).HasMaxLength(100);
        modelBuilder.Entity<ListingCase>().Property(listing => listing.State).HasMaxLength(100);

        modelBuilder.Entity<CaseContact>().Property(contact => contact.FirstName).HasMaxLength(100);
        modelBuilder.Entity<CaseContact>().Property(contact => contact.LastName).HasMaxLength(100);
        modelBuilder.Entity<CaseContact>().Property(contact => contact.CompanyName).HasMaxLength(200);
        modelBuilder.Entity<CaseContact>().Property(contact => contact.Email).HasMaxLength(256);
        modelBuilder.Entity<CaseContact>().Property(contact => contact.PhoneNumber).HasMaxLength(30);
    
        modelBuilder.Entity<Agent>().Property(agent => agent.AgentFirstName).HasMaxLength(100);
        modelBuilder.Entity<Agent>().Property(agent => agent.AgentLastName).HasMaxLength(100);
        modelBuilder.Entity<Agent>().Property(agent => agent.CompanyName).HasMaxLength(200);
        modelBuilder.Entity<Agent>().Property(agent => agent.AvatarUrl).HasMaxLength(2048);

        modelBuilder.Entity<PhotographyCompany>().Property(company => company.PhotographyCompanyName).HasMaxLength(200);
        modelBuilder.Entity<CaseContact>().Property(contact => contact.ProfileUrl).HasMaxLength(2048);        
        modelBuilder.Entity<MediaAsset>().Property(media => media.MediaUrl).HasMaxLength(2048);   
    
        modelBuilder.Entity<ListingCase>().HasQueryFilter(listing => !listing.IsDeleted);
        modelBuilder.Entity<MediaAsset>().HasQueryFilter(media => !media.IsDeleted && !media.ListingCase!.IsDeleted);
        modelBuilder.Entity<CaseContact>().HasQueryFilter(contact => !contact.ListingCase!.IsDeleted);
        modelBuilder.Entity<AgentListingCase>().HasQueryFilter(link => !link.ListingCase!.IsDeleted);

        modelBuilder.Entity<ApplicationUser>().Property(user => user.CreatedAt).HasDefaultValueSql("SYSUTCDATETIME()");
        modelBuilder.Entity<ListingCase>().Property(listing => listing.CreatedAt).HasDefaultValueSql("SYSUTCDATETIME()");
        modelBuilder.Entity<MediaAsset>().Property(media => media.UploadedAt).HasDefaultValueSql("SYSUTCDATETIME()");
    }
}