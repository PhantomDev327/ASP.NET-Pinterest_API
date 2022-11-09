using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace nxPinterest.Services.Models.Request;

public class CreateUserAlbumSharedRequest
{
    public string AlbumName { get; set; } = "noname";

    public string AlbumUrl { get; set; }

    [Required]
    public DateTime? AlbumExpireDate { get; set; }

    public List<UserAlbumMediaRequest> UserAlbumMedias { get; set; }

}