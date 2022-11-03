using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace nxPinterest.Data.Models;

public class UserAlbumMedia
{
    [Key]
    public int AlbumMediaId { get; set; }

    public int UserMediaId { get; set; }

    public int AlbumId { get; set; }

    public string UserMediaName { get; set; }

    public int ContainerId { get; set; }

    public DateTime? AlbumMediaCreatedat { get; set; }

    public DateTime? AlbumMediaUpdatedat { get; set; }

    public DateTime? AlbumMediaDeletedat { get; set; }

    [ForeignKey("AlbumId")]
    public virtual UserAlbum UserAlbum { get; set; }
    [ForeignKey("ContainerId")]

    public virtual UserContainer UserContainer { get; set; }
    [ForeignKey("UserMediaId")]
    public virtual UserMedia UserMedia { get; set; }
}