using nxPinterest.Data.Enums;
using System;
using System.Collections.Generic;

namespace nxPinterest.Data.Models;

public class UserAlbum
{
    public int AlbumId { get; set; }

    public string AlbumName { get; set; }

    public string UserId { get; set; }

    public int ContainerId { get; set; }

    public string AlbumThumbnailUrl { get; set; }

    public int AlbumCount { get; set; }

    public string AlbumUrl { get; set; }

    public AlbumType AlbumType { get; set; }

    public DateTime? AlbumExpireDate { get; set; }

    public bool AlbumVisibility { get; set; }

    public DateTime? AlbumCreatedat { get; set; }

    public DateTime? AlbumUpdatedat { get; set; }

    public DateTime? AlbumDeletedat { get; set; }

    public virtual ApplicationUser ApplicationUser { get; set; }

    public virtual UserContainer UserContainer { get; set; }

    public List<UserAlbumMedia> UserAlbumMedias { get; set; } = new List<UserAlbumMedia>();


}