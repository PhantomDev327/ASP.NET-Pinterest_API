using nxPinterest.Data.Enums;
using System;

namespace nxPinterest.Data.Models;

public class UserAlbum
{
    public int AlbumId { get; set; }

    public string AlbumName { get; set; }

    public int ContainerId { get; set; }

    public string AlbumUrl { get; set; }

    public AlbumType AlbumType { get; set; }

    public DateTime? AlbumExpireDate { get; set; }

    public bool AlbumVisibility { get; set; }

    public DateTime? AlbumCreatedat { get; set; }

    public DateTime? AlbumUpdatedat { get; set; }

    public DateTime? AlbumDeletedat { get; set; }
}