$.urlParam = function (name) {
    var results = new RegExp('[\?&]' + name + '=([^&#]*)')
        .exec(window.location.search);

    return (results !== null) ? results[1] || 0 : false;
}

$(document).ready(function () {
    hideLoader();
    $('#SearchKey').tagsinput({
        confirmKeys: [13, 44]
    });

    var tagsinputEle = getTagsinputEle();
    tagsinputEle.keydown(function (event) {
        if (event.keyCode == 13) {
            var typingInputValue = tagsinputEle.val();
            if (!typingInputValue) {
                searchByTags();
                return;
            } else {
                addTags(typingInputValue);
            }
        }
    });
    tagsinputEle.on("paste", function (event) {
        var content = '';
        if (isIE()) {
            //IE allows to get the clipboard data of the window object.
            content = window.clipboardData.getData('text');
        } else {
            //This works for Chrome and Firefox.
            content = event.originalEvent.clipboardData.getData('text/plain');
        }
        tagsinputEle.attr('size', content.length);
    });
});
function isIE() {
    var ua = window.navigator.userAgent;
    return ua.indexOf('MSIE ') > 0 || ua.indexOf('Trident/') > 0 || ua.indexOf('Edge/') > 0
}
function getTagsinputEle() {
    return $('.navbar-header .bootstrap-tagsinput input[type=text]');
}
function addTags(value) {
    $('#SearchKey').tagsinput('add', value);
    searchByTags();
}
function searchByTags() {
    showLoader();
    var pageIndex = $.urlParam('pageIndex');
    if (pageIndex == false) {
        pageIndex = 1;
    }
    var searchKey = $('#SearchKey').val();
    var url = "/?pageIndex=" + pageIndex;
    if (searchKey) {
        url += "&searchKey=" + searchKey;
    }
    window.location = url;
}
function changePlaceholderText() {
    var searchKey = $('#SearchKey').val();
    var tagsinputEle = getTagsinputEle();
    if (searchKey) {
        tagsinputEle.removeAttr('placeholder');
    } else {
        tagsinputEle.attr('placeholder', '検索');
    }
    tagsinputEle.attr('size', searchKey.length);
}
$('#SearchKey').on('itemAdded', function (event) {
    changePlaceholderText();
});
$('#SearchKey').on('itemRemoved', function (event) {
    changePlaceholderText();
});