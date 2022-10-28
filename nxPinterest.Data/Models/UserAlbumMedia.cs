using System;

namespace nxPinterest.Data.Models;

public class UserAlbumMedia
{
    public int AlbumMediaId { get; set; }

    public int UserMediaId { get; set; }

    public string UserMediaName { get; set; }

    public int ContainerId { get; set; }

    public DateTime? AlbumMediaCreatedat { get; set; }

    public DateTime? AlbumMediaUpdatedat { get; set; }

    public DateTime? AlbumMediaDeletedat { get; set; }
}