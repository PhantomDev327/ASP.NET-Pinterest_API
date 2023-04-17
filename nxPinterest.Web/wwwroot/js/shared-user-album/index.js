var shareUserAlbumMediaController = function() {

    this.initialize = function() {
        registerDatepicker();
        showHiddenFooterModalShare(false);
        registerEvents();
    };

    function registerEvents() {

        $('body').on('click', '#sharedAlbumMedia', function (e) {
            e.preventDefault();
            var $albumMedias = window.selectedAlbumMediaList;
            if (!window.selectedAlbumMediaList || $albumMedias.length === 0) {
                alert("写真を選択してください。");
                return;
            }

            var currentDate = new Date();
            var $getExpireDate = $("#expireDate").val();
            var $chooseDate = new Date($getExpireDate);

            if (formatDate($chooseDate) < formatDate(currentDate)) {
                alert("共有期限は未来の日付を設定してください。");
                return;
            }

            $.ajax({
                url: "/UserAlbum/CreateShareUserMedia",
                type: "post",
                dataType: "json",
                data: {
                    AlbumExpireDate: formatDate($chooseDate.toLocaleString()),
                    UserAlbumMedias: $albumMedias
                },
                cache: false,
                success: function (result) {
                    if (result.success) {
                        document.getElementById("shareUserMediaFileLink").value = result.data;
                        showHiddenFooterModalShare(true);
                        document.getElementById("shareUserMediaFileLink").readOnly = true;
                    } else {
                        alert("保存できませんでした。");
                    }
                },
                error: function () {
                    alert("エラーが発生し、保存できませんでした。");
                }
            });

        });

        $("body").on("click", "#shareMedia", function (e) {
            e.preventDefault();
            $('#shareUserMediaFileModal').modal('show');
            showHiddenFooterModalShare(false);
            document.getElementById("shareUserMediaFileLink").value = "";
            $("#expireDate").datepicker().datepicker("setDate", (new Date()).getDate()+7);
        });

        $("body").on("click", "#copyLinkMedia", function(e) {
                e.preventDefault();
                const $copyLink = document.getElementById("shareUserMediaFileLink");
                let listener = (e) => {
                    let clipboard = e.clipboardData;
                    clipboard.setData('text/plain', $copyLink.value);
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
    };
   
    function registerDatepicker() {
        $(document).ready(function() {
            $.fn.datepicker.defaults.language = "ja";
            if ($.fn.datepicker) {
                $("#expireDate").datepicker({ todayHighlight: true });
                $("#expireDate").datepicker().datepicker("setDate", (new Date()).getDate() + 7);
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

    function mesageTooltipCopy() {
        $('#tooltipCopy').tooltip('toggle');
        $('.tooltip').addClass('tooltip-top');
        setTimeout(function () {
            $('#tooltipCopy').tooltip('hide');
        }, 1000);
    };
}