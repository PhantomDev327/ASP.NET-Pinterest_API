using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using nxPinterest.Data.Models;

namespace nxPinterest.Data.Configrations;

public class UserAlbumConfiguration : IEntityTypeConfiguration<UserAlbum>
{
    public void Configure(EntityTypeBuilder<UserAlbum> builder)
    {
        builder.ToTable("UserAlbum", "UserAlbum");

        builder.HasKey(e => e.AlbumId);

        builder.Property(e => e.AlbumId)
            .ValueGeneratedOnAdd()
            .HasColumnName("album_id");

        builder.Property(e => e.AlbumName).HasColumnName("album_name").HasMaxLength(100);

        builder.HasIndex(x => x.AlbumName)
            .IsUnique();

        builder.Property(e => e.AlbumType).HasColumnName("album_type");

        builder.Property(e => e.ContainerId).HasColumnName("container_id");

        builder.Property(e => e.AlbumUrl).HasColumnName("album_url").HasMaxLength(1000);

        builder.Property(e => e.AlbumVisibility).HasColumnName("album_visibility");

        builder.Property(e => e.AlbumExpireDate).HasColumnName("album_expiredate").HasDefaultValueSql("getdate()");

        builder.Property(e => e.AlbumCreatedat).HasColumnName("album_createdat").HasDefaultValueSql("getdate()");

        builder.Property(e => e.AlbumUpdatedat).HasColumnName("album_updatedat").HasDefaultValueSql("getdate()");

        builder.Property(e => e.AlbumDeletedat).HasColumnName("album_deletedat").HasDefaultValueSql("getdate()");
    }
}