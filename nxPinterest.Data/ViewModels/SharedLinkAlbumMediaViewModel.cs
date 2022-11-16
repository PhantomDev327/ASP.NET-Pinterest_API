using System;

namespace nxPinterest.Data.ViewModels;

public class SharedLinkAlbumMediaViewModel
{
    public int MediaId { get; set; }
    public string MediaUrl { get; set; }
    public int AlbumId { get; set; }
    public int ContainerId { get; set; }
    public string MediaFileName { get; set; }
    public string MediaSmallUrl { get; set; }
    public string MediaThumbnailUrl { get; set; }
    public string MediaTitle { get; set; }
    public string MediaDescription { get; set; }
    public int PageIndex { get; set; } = 1;
    public DateTime? AlbumMediaCreatedat { get; set; }
}