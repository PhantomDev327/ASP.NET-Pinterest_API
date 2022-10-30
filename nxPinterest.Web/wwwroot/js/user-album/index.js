var userAlbumController = function () {

    this.initialize = function () {
        registerEvents();
    }

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
                alert("Album Name is required !");
            }
            var $associateAlbum = $mediaid.map(function (v, k, a) {
                return { UserMediaId: v, MediaUrl: $links[k] };
            });

            $.ajax({
                url: "/UserAlbum/Create",
                type: 'post',
                dataType: 'json',
                data: {
                    AlbumName: $albumName,
                    UserAlbumMedias: $associateAlbum
                },
                cache: false,
                success: function (data) {
                    alert("success");
                },
                error: function () {
                    alert("error");
                }
            });


        });
    };


}