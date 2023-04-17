using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using nxPinterest.Data.Models;

namespace nxPinterest.Data.Configrations;

public class UserAlbumConfiguration : IEntityTypeConfiguration<UserAlbum>
{
    public void Configure(EntityTypeBuilder<UserAlbum> builder)
    {
        builder.ToTable("UserAlbum");

        builder.HasKey(e => e.AlbumId);

        builder.HasOne(x => x.ApplicationUser)
            .WithMany(x => x.UserAlbums)
            .HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Restrict); ;


        builder.HasOne(x => x.UserContainer)
            .WithMany(x => x.UserAlbums)
            .HasForeignKey(x => x.ContainerId).OnDelete(DeleteBehavior.Restrict); ;

        builder.Property(e => e.AlbumId)
            .ValueGeneratedOnAdd()
            .HasColumnName("album_id");

        builder.Property(e => e.AlbumName).HasColumnName("album_name").HasMaxLength(100);

        builder.Property(e => e.UserId).HasColumnName("user_id").HasMaxLength(450);

        builder.HasIndex(x => x.AlbumName)
            .IsUnique();

        builder.Property(e => e.AlbumType).HasColumnName("album_type").HasColumnType("tinyint");

        builder.Property(e => e.ContainerId).HasColumnName("container_id");

        builder.Property(e => e.AlbumCount).HasColumnName("album_count");

        builder.Property(e => e.AlbumThumbnailUrl).HasColumnName("album_thumbnail_url").HasMaxLength(1000);

        builder.Property(e => e.AlbumUrl).HasColumnName("album_url").HasMaxLength(1000);

        builder.Property(e => e.AlbumVisibility).HasColumnName("album_visibility");

        builder.Property(e => e.AlbumExpireDate).HasColumnName("album_expiredate").HasDefaultValueSql("getdate()");

        builder.Property(e => e.AlbumCreatedat).HasColumnName("album_createdat").HasDefaultValueSql("getdate()");

        builder.Property(e => e.AlbumUpdatedat).HasColumnName("album_updatedat").HasDefaultValueSql("getdate()");

        builder.Property(e => e.AlbumDeletedat).HasColumnName("album_deletedat").HasDefaultValueSql("getdate()");
    }
}