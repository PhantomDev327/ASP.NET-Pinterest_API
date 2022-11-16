var userAlbumController = function () {

    this.initialize = function () {
        registerEvents();
    };

    function registerEvents() {

        $('body').on('click', '#createUserMediaFolderButton', function (e) {
            e.preventDefault();
            var $albumMedias = window.selectedAlbumMediaList;
            if (!window.selectedAlbumMediaList || $albumMedias.length === 0) {
                alert("please choose the image !");
                return;
            }
            var $albumName = $('#createUserMediaFolderName').val();
            if ($albumName === '' || $albumName == null) {
                alert("アルバムは、必ず指定してください。 !");
                return;
            }

            $.ajax({
                url: "/UserAlbum/Create",
                type: "post",
                dataType: "json",
                data: {
                    AlbumName: $albumName,
                    UserAlbumMedias: $albumMedias
                },
                cache: false,
                success: function (result) {
                    if (result) {
                        $('#createUserMediaFolderModal').modal('hide');
                    }
                },
                error: function () {
                    alert("保存できない.");
                }
            });

        });

        $('body').on('click', '#createMediaFolder, .createMediaFolder', function (e) {
            e.preventDefault();
            LoadAlbums();

        });

        $("body").on("click", "#listalbum .list-group-item", function(e) {
                e.preventDefault();
                var $valueAlbumName = $(this).data("albumname");
                document.getElementById("createUserMediaFolderName").value = $valueAlbumName != null && $valueAlbumName != '' ? $valueAlbumName : '';
            });
    };

    function LoadAlbums() {
        $.ajax({
            url: "/UserAlbum/GetAlbums",
            type: "GET",
            cache: !0,
            datatype: "html",
            success: function (response) {
                $("#mediaFolderContent").html(response);
                $('#createUserMediaFolderModal').modal('show');
          
            },
            failure: function (response) {
                alert(response.responseText);
            },
            error: function (response) {
                alert(response.responseText);
            }
        });
    }

}