using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using nxPinterest.Data.Configrations;
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

        public virtual DbSet<UserAlbum> UserAlbums { get; set; }
        public virtual DbSet<UserAlbumMedia> UserAlbumMedias { get; set; }

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
                    .HasColumnType("nvarchar(100)")
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
                    .HasColumnName("media_created");

                entity.Property(e => e.Uploaded)
                    .HasDefaultValueSql("getdate()")
                    .HasColumnName("media_uploaded");

                entity.Property(e => e.Modified)
                    .HasColumnName("media_modified");

                entity.Property(e => e.Deleted)
                    .HasColumnName("media_deleted");

                entity.Property(e => e.Status)
                    .HasColumnName("media_status");

                entity.Property(e => e.SearchText)
                    .HasColumnName("media_searchtext");

                entity.Property(e => e.Tags)
                    .HasColumnName("media_tags");

                entity.Property(e => e.OriginalTags)
                    .HasColumnName("media_originaltags");

                entity.Property(e => e.AITags)
                    .HasColumnName("media_aitags");

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
                    .HasColumnName("tags_type")
                    .HasColumnType("tinyint");

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

            // configuration table UserAlbum and UserAlbumMedia
            modelBuilder.ApplyConfiguration(new UserAlbumConfiguration());
            modelBuilder.ApplyConfiguration(new UserAlbumMediaConfiguration());
        }
    }
}
