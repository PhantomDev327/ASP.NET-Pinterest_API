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
        $('body').on('click', '#editMediaFolder', function (e) {
            e.preventDefault();
            $('#editUserMediaFolderModal').modal('show');
            var $albumName = $('#album__name').text();
            if ($albumName != null && $albumName != undefined) {
                document.getElementById('oldAlbumName').value = $albumName;
                document.getElementById('newAlbumName').value = $albumName;
            }
          
        });

        $('body').on('click', '#btnEditAlbum', function (e) {
            e.preventDefault();
            var $newAlbumName = document.getElementById('newAlbumName').value;
            var $oldAlbumName = document.getElementById('oldAlbumName').value;
            if ($newAlbumName != null && $newAlbumName != undefined) {
                $.ajax({
                    url: "/UserAlbum/Update",
                    type: "post",
                    dataType: "json",
                    data: {
                        oldAlbumName: $oldAlbumName,
                        newAlbumName: $newAlbumName
                    },
                    cache: false,
                    success: function (result) {
                        if (result.success) {
                            document.getElementById('album__name').innerText = result.data;
                            $('#editUserMediaFolderModal').modal('hide');
                        } else {
                            document.getElementById('error_album').innerText = result.message;
                            $('#editUserMediaFolderModal').modal('show');
                        }
                    },
                    error: function () {
                        alert("保存できない.");
                    }
                });
            }

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