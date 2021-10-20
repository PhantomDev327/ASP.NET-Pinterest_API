using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using nxPinterest.Data.Models;

namespace nxPinterest.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public virtual DbSet<UserMedia> UserMedia { get; set; }
        public virtual DbSet<UserMediaThumbnails> UserMediaThumbnails { get; set; }
        public virtual DbSet<MediaId> MediaId { get; set; }
        public virtual DbSet<EditTags> EditTags { get; set; }
        public virtual DbSet<EditTag> EditTag { get; set; }
        public virtual DbSet<SearchResultUserMedia> SearchResultUserMedia { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasAnnotation("Relational:Collation", "SQL_Latin1_General_CP1_CI_AS");

        
            modelBuilder.Entity<UserMedia>(entity =>
            {
                entity.HasKey(e => e.MediaId)
                    .HasName("PK__UserMedia");

                entity.Property(e => e.UserId).HasColumnName("user_id");

                entity.Property(e => e.MediaId)
                    //.ValueGeneratedNever()
                    .ValueGeneratedOnAdd()
                    .HasColumnName("media_id");

                entity.Property(e => e.MediaTitle)
                    .HasColumnType("nvarchar(max)")
                    .IsUnicode(true)
                    .HasColumnName("media_title");

                entity.Property(e => e.MediaDescription)
                    .HasColumnType("nvarchar(max)")
                    .IsUnicode(true)
                    .HasColumnName("media_description");

                entity.Property(e => e.MediaUrl)
                    .HasMaxLength(1000)
                    .IsUnicode(true)
                    .HasColumnName("media_url");

                entity.Property(e => e.MediaFileName)
                    .HasMaxLength(1000)
                    .IsUnicode(true)
                    .HasColumnName("media_file_name");

                entity.Property(e => e.MediaFileType)
                    .HasMaxLength(1000)
                    .IsUnicode(false)
                    .HasColumnName("media_file_type");

                entity.Property(e => e.DateTimeUploaded)
                    .HasDefaultValueSql("getdate()");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.UserMedia)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("UserMedia_fk_User");
            });

            modelBuilder.Entity<UserMediaThumbnails>(entity =>
            {

                entity.HasKey(e => e.Id)
                .HasName("PK_UserMediaThumbnails");

                entity.Property(e => e.Id)
                    .ValueGeneratedOnAdd()
                    .HasColumnName("id");

                entity.Property(e => e.MediaId)
                    .HasColumnName("media_id");

                entity.Property(e => e.MediaFileName)
                    .HasMaxLength(1000)
                    .IsUnicode(true)
                    .HasColumnName("media_file_name");

                entity.Property(e => e.MediaFileType)
                    .HasMaxLength(1000)
                    .IsUnicode(false)
                    .HasColumnName("media_file_type");

                entity.Property(e => e.MediaUrl)
                    .HasMaxLength(1000)
                    .IsUnicode(true)
                    .HasColumnName("media_url");

                entity.Property(e => e.DateTimeUploaded)
                    .HasDefaultValueSql("getdate()");

                entity.HasOne(e => e.UserMedia)
                    .WithMany(e => e.UserMediaThumbnails)
                    .HasForeignKey(e => e.MediaId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_UserMediaThumbnails_UserMedia");
            });

            base.OnModelCreating(modelBuilder);
        }
    }
}
