var shareUserAlbumMediaController = function() {

    this.initialize = function() {
        registerDatepicker();
        showHiddenFooterModalShare(false);
        registerEvents();
    };

    function registerEvents() {

        $('body').on('click', '#sharedAlbumMedia', function (e) {
            e.preventDefault();
            var $links = window.selectedMediaSrcList;
            var $mediaid = window.selectedMediaIdList;
            if (!window.selectedMediaIdList || $mediaid.length === 0 ||
                !window.selectedMediaSrcList || $links.length === 0) {
                alert("please choose the image !");
                return;
            }

            var currentDate = new Date();
            var $getExpireDate = $("#expireDate").val();
            var $chooseDate = new Date($getExpireDate);

            if (formatDate($chooseDate) < formatDate(currentDate)) {
                alert("Cannot be selected less than current date");
                return;
            }
            var $associateAlbum = $mediaid.map(function (v, k, a) {
                return { UserMediaId: v, MediaUrl: $links[k] };
            });

            $.ajax({
                url: "/UserAlbum/CreateShareUserMedia",
                type: "post",
                dataType: "json",
                data: {
                    AlbumExpireDate: formatDate($chooseDate.toLocaleString()),
                    UserAlbumMedias: $associateAlbum
                },
                cache: false,
                success: function (result) {
                    if (result.success) {
                        document.getElementById("shareUserMediaFileLink").value = result.data;
                        showHiddenFooterModalShare(true);
                        document.getElementById("shareUserMediaFileLink").readOnly = true;
                    } else {
                        alert("保存できない.");
                    }
                },
                error: function () {
                    alert("保存できない.");
                }
            });

        });


        $("body").on("click", "#copyLinkMedia", function(e) {
                e.preventDefault();
                const $copyLink = document.getElementById("shareUserMediaFileLink");

                $copyLink.select();
                $copyLink.setSelectionRange(0, 99999);
                navigator.clipboard.writeText($copyLink.value);
        });
    };
   

    function registerDatepicker() {
        $(document).ready(function() {
            $.fn.datepicker.defaults.language = "ja";
            if ($.fn.datepicker) {
                $("#expireDate").datepicker({ todayHighlight: true });
                $("#expireDate").datepicker().datepicker("setDate", new Date());
            }

        });
    }

    function showHiddenFooterModalShare(parameters) {
      if (parameters) {
          document.getElementById("footerUserMediaLink").style.display = 'block';
      } else {
          document.getElementById("footerUserMediaLink").style.display = 'none';
      }
    }

    function formatDate(date) {
        var d = new Date(date),
            month = '' + (d.getMonth() + 1),
            day = '' + d.getDate(),
            year = d.getFullYear();

        if (month.length < 2)
            month = '0' + month;
        if (day.length < 2)
            day = '0' + day;

        return [year, month, day].join('-');
    }

}