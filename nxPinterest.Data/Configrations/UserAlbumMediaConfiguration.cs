using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using nxPinterest.Data.Models;

namespace nxPinterest.Data.Configrations;

public class UserAlbumMediaConfiguration : IEntityTypeConfiguration<UserAlbumMedia>
{
    public void Configure(EntityTypeBuilder<UserAlbumMedia> builder)
    {
        builder.ToTable("UserAlbumMedias");


        builder.HasOne(x => x.UserContainer)
            .WithMany(x => x.UserAlbumMedias)
            .HasForeignKey(x => x.ContainerId).OnDelete(DeleteBehavior.Restrict); ;

        builder.HasOne(x => x.UserMedia)
            .WithMany(x => x.UserAlbumMedias)
            .HasForeignKey(x => x.UserMediaId).OnDelete(DeleteBehavior.Restrict); ;

        builder.HasOne(x => x.UserAlbum)
            .WithMany(x => x.UserAlbumMedias)
            .HasForeignKey(x => x.AlbumId).OnDelete(DeleteBehavior.Restrict); ;


        builder.Property(e => e.AlbumMediaId)
            .ValueGeneratedOnAdd()
            .HasColumnName("albummedia_id");

        builder.Property(e => e.UserMediaId)
            .HasColumnName("user_media_id").IsRequired();

        builder.Property(e => e.AlbumId)
            .HasColumnName("album_id").IsRequired();

        builder.Property(e => e.UserMediaName).HasColumnName("user_media_name").HasMaxLength(100);

        builder.Property(e => e.ContainerId).HasColumnName("container_id").IsRequired();


        builder.Property(e => e.AlbumMediaCreatedat).HasColumnName("albummedia_createdat")
            .HasDefaultValueSql("getutcdate()");

        builder.Property(e => e.AlbumMediaUpdatedat).HasColumnName("albummedia_updatedat")
            .HasDefaultValueSql("getutcdate()");

        builder.Property(e => e.AlbumMediaDeletedat).HasColumnName("albummedia_deletedat")
            .HasDefaultValueSql("getutcdate()");
    }
}