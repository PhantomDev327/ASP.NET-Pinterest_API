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
                alert("アルバム名を指定してください");
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
                    alert("保存に失敗しました");
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
            var deleteButton = document.getElementById('removeFolder');
            if (image_count == 0) {
                deleteButton.removeAttribute('disabled');
            } else {                
                deleteButton.setAttribute('disabled', 'disabled');
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
                        alert("保存に失敗しました");
                    }
                });
            }

        });

        $('body').on('click', '#createMediaFolder, .createMediaFolder', function (e) {
            e.preventDefault();
            LoadAlbums();
        });

        $('body').on('click', '#editMultiMedia, .editMultiMedia', function (e) {
            e.preventDefault();
            var media_id = window.selectedMediaIdList.toString();
            var url = "/UserMedia/UpdateSameTitleMediaFile?mediaId=" + media_id;
            window.location.href = url;
        });

        $("body").on("click", "#listalbum .list-group-item", function(e) {
                e.preventDefault();
                var $valueAlbumName = $(this).data("albumname");
                document.getElementById("createUserMediaFolderName").value = $valueAlbumName != null && $valueAlbumName != '' ? $valueAlbumName : '';
        });


        $("body").on("click", "#copyAlbum", function (e) {
            e.preventDefault();
            let url = '';
            const $valueLink = $(this).data("albumurl");
            if ($valueLink != '') {
                url = `${window.location.protocol}//${window.location.hostname}:${window.location.port}/shared/${$valueLink}`;
            }
            let listener = (e) => {
                let clipboard = e.clipboardData;
                clipboard.setData('text/plain', url);
                e.preventDefault();
            };
            document.addEventListener('copy', listener, false);
            const copyResult = document.execCommand('copy');
            document.removeEventListener('copy', listener, false);
            mesageTooltipCopy();
            // remove all ranges after copy
            window.getSelection().removeAllRanges();
            return copyResult;
        });


        $('body').on('click', '#removeFolder', function (e) {
            e.preventDefault();
            var $albumName = $('#album__name').text();
            
            $('#removeUserMediaFolderModal').modal('show');
            
            document.getElementById('album_name').innerHTML = $albumName + 'を削除します。よろしいですか？';

        });
        $('body').on('click', '#btnRemoveAlbum', function (e) {
            e.preventDefault();
            var $removeAlbumName = $('#album__name').text();
            $.ajax({
                url: "/UserAlbum/Remove",
                type: "post",
                dataType: "json",
                data: {
                    removeAlbumName: $removeAlbumName
                },
                cache: false,
                success: function (result) {
                    if (result.success) {
                        document.getElementById('album__name').innerText = '';
                        $('#removeUserMediaFolderModal').modal('hide');
                        window.location = '/';
                    } else {
                        document.getElementById('error_album').innerText = result.message;
                        $('#removeUserMediaFolderModal').modal('hide');
                        
                    }
                },
                error: function () {
                    alert("保存に失敗しました");
                }
            });
        });

    };

    function mesageTooltipCopy() {
        $('#tooltipAlbumCopy').tooltip('toggle');
        $('.tooltip').addClass('tooltip-top');
        setTimeout(function () {
            $('#tooltipAlbumCopy').tooltip('hide');
        }, 1000);
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