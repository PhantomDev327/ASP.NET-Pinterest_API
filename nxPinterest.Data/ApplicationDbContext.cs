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
        public virtual DbSet<MediaId> MediaId { get; set; }
        //public virtual DbSet<EditTags> EditTags { get; set; }
        //public virtual DbSet<EditTag> EditTag { get; set; }
        public virtual DbSet<SearchResultUserMedia> SearchResultUserMedia { get; set; }
        public virtual DbSet<UserContainer> UserContainer { get; set; }
        public virtual DbSet<UserMediaTags> UserMediaTags { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasAnnotation("Relational:Collation", "SQL_Latin1_General_CP1_CI_AS");

        
            modelBuilder.Entity<UserMedia>(entity =>
            {
                entity.HasKey(e => e.MediaId)
                    .HasName("PK__UserMedia");

                entity.Property(e => e.UserId).HasColumnName("user_id");
                entity.Property(e => e.ContainerId).HasColumnName("container_id");

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

                entity.Property(e => e.MediaSmallUrl)
                    .HasMaxLength(1000)
                    .IsUnicode(true)
                    .HasColumnName("media_small_url");

                entity.Property(e => e.MediaThumbnailUrl)
                    .HasMaxLength(1000)
                    .IsUnicode(true)
                    .HasColumnName("media_thumbnail_url");

                entity.Property(e => e.MediaFileName)
                    .HasMaxLength(1000)
                    .IsUnicode(true)
                    .HasColumnName("media_file_name");

                entity.Property(e => e.MediaFileType)
                    .HasMaxLength(1000)
                    .IsUnicode(false)
                    .HasColumnName("media_file_type");

                entity.Property(e => e.Created)
                    .HasDefaultValueSql("getdate()")
                    .HasColumnName("media_created");

                entity.Property(e => e.Updated)
                    .HasColumnName("media_updated");

                entity.Property(e => e.Deleted)
                    .HasColumnName("media_deleted");

                entity.Property(e => e.Status)
                    .HasColumnName("media_status");

                entity.Property(e => e.DateTimeUploaded)
                    .HasDefaultValueSql("getdate()");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.UserMedia)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("UserMedia_fk_User");
            });

            modelBuilder.Entity<UserContainer>(entity =>
            {
                entity.HasKey(e => e.container_id)
                    .HasName("PK__UserContainer");

                entity.Property(e => e.container_id)
                .ValueGeneratedOnAdd()
                .HasColumnName("container_id");
            });

            modelBuilder.Entity<UserMediaTags>(entity =>
            {
                entity.HasKey(e => new { e.UserMediaTagsId })
                    .HasName("PK__UserMediaTags");

                entity.Property(e => e.UserMediaTagsId)
                    .HasColumnName("user_media_tags_id");

                entity.Property(e => e.UserMediaName)
                    .HasColumnName("user_media_name");

                entity.Property(e => e.ContainerId)
                    .HasColumnName("container_id");

                entity.Property(e => e.TagsType)
                    .HasColumnName("tags_type");

                entity.Property(e => e.Tag)
                    .HasMaxLength(100)
                    .IsUnicode(true)
                    .HasColumnName("tag");

                entity.Property(e => e.Confidence)
                    .HasColumnName("confidence");

                entity.Property(e => e.Created)
                   .HasDefaultValueSql("getdate()")
                   .HasColumnName("tags_created");
            });


            base.OnModelCreating(modelBuilder);
        }
    }
}
