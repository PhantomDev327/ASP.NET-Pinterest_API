var userAlbumController = function () {

    this.initialize = function () {
        registerEvents();
    };

    function registerEvents() {

        $('body').on('click', '#createUserMediaFolderButton', function (e) {
            e.preventDefault();
            var $links = window.selectedMediaSrcList;
            var $mediaid = window.selectedMediaIdList;
            if (!window.selectedMediaIdList || $mediaid.length === 0 ||
                !window.selectedMediaSrcList || $links.length === 0) {
                alert("please choose the image !");
                return;
            }
            var $albumName = $('#createUserMediaFolderName').val();
            if ($albumName === '' || $albumName == null) {
                alert("アルバムは、必ず指定してください。 !");
                return;
            }
            var $associateAlbum = $mediaid.map(function (v, k, a) {
                return { UserMediaId: v, MediaUrl: $links[k] };
            });

            $.ajax({
                url: "/UserAlbum/Create",
                type: "post",
                dataType: "json",
                data: {
                    AlbumName: $albumName,
                    UserAlbumMedias: $associateAlbum
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

        $('body').on('click', '#createMediaFolder', function (e) {
            e.preventDefault();
            LoadAlbums();

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