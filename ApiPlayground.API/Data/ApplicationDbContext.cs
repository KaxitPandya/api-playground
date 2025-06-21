using Microsoft.EntityFrameworkCore;
using ApiPlayground.Core.Models;
using System.Text.Json;

namespace ApiPlayground.API.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) {}

        public DbSet<Integration> Integrations { get; set; } = null!;
        public DbSet<Request> Requests { get; set; } = null!;
        public DbSet<RequestResult> RequestResults { get; set; } = null!;
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            // Configure relationships
            modelBuilder.Entity<Request>()
                .HasOne(r => r.Integration)
                .WithMany(i => i.Requests)
                .HasForeignKey(r => r.IntegrationId)
                .OnDelete(DeleteBehavior.Cascade);
                
            modelBuilder.Entity<RequestResult>()
                .HasOne(rr => rr.Request)
                .WithMany()
                .HasForeignKey(rr => rr.RequestId)
                .OnDelete(DeleteBehavior.Cascade);
                
            // Configure value converter for Headers dictionary
            modelBuilder.Entity<Request>()
                .Property(r => r.Headers)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null!),
                    v => JsonSerializer.Deserialize<Dictionary<string, string>>(v, (JsonSerializerOptions)null!) ?? new Dictionary<string, string>()
                );
                
            // Configure complex types as JSON to avoid creating separate tables
            modelBuilder.Entity<Integration>()
                .Property(i => i.Authentication)
                .HasConversion(
                    v => v != null ? JsonSerializer.Serialize(v, (JsonSerializerOptions)null!) : null,
                    v => v != null ? JsonSerializer.Deserialize<AuthenticationConfig>(v, (JsonSerializerOptions)null!) : null
                );
                
            modelBuilder.Entity<Request>()
                .Property(r => r.RetryConfig)
                .HasConversion(
                    v => v != null ? JsonSerializer.Serialize(v, (JsonSerializerOptions)null!) : null,
                    v => v != null ? JsonSerializer.Deserialize<RetryConfig>(v, (JsonSerializerOptions)null!) : null
                );
                
            modelBuilder.Entity<Request>()
                .Property(r => r.ConditionalRules)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null!),
                    v => JsonSerializer.Deserialize<List<ConditionalRule>>(v, (JsonSerializerOptions)null!) ?? new List<ConditionalRule>()
                );
                
            modelBuilder.Entity<Request>()
                .Property(r => r.DependsOn)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null!),
                    v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions)null!) ?? new List<string>()
                );
        }
    }
}
