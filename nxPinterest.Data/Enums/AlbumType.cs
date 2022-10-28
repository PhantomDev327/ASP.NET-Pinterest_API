using System.ComponentModel;
namespace nxPinterest.Data.Enums;
public enum AlbumType
{
    [Description("共有")] 
    AlbumShare = 0,
    [Description("アルバム")] 
    Album = 1
}