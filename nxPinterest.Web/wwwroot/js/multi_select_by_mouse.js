function MultiImageSelectByMouse(opts) {
    "use strict";

    // 初期オプションパラメーターを設定
    this.options = {
        targetArea: "body",
        elements: "img",
        selectedClass: "is-selected",
        holdKey: "ctrlKey",
        onItemSelect: null,
        onItemDeselect: null,
        onMultiSelectDone: null
    };
    if (opts) {
        for (var key in opts) {
            this.options[key] = opts[key];
        }
    }

    // 対象element取得
    this.targetArea = document.querySelector(this.options.targetArea);
    if (!this.targetArea) {
        throw new Error("複数画像選択エリアを設定しません！");
    }

    this.itemElements = this.options.targetArea + " " + this.options.elements;
    var targetItems = function (itemElelementName) {
        return document.querySelectorAll(itemElelementName);
    }

    // mouseエリア
    var selectArea = function () {
        return document.getElementById("multiImageSelect-area");
    };
    var self = this;

    var removeAllSelectedClass = function (self, e) {
        for (var el of targetItems(self.itemElements)) {
            if (!e[self.options.holdKey]) {
                el.classList.remove(self.options.selectedClass);
            }
        }
    }

    var isClickPointPreviewEl = function (e) {
        if (!e.target || !e.target.nodeName) {
            return;
        }
        var is_preview_span_el = e.target.nodeName.toLowerCase() === "span" && e.target.className === "previewButton";
        var is_preview_i_el = e.target.nodeName.toLowerCase() === "i" && e.target.parentElement.nodeName.toLowerCase() === "span" && e.target.parentElement.className === "previewButton";
        return is_preview_span_el || is_preview_i_el
    }

    // mouseイベントをリスナー
    this.areaOpen = function (e) {
        if (e.detail != 1) {
            // ワンクリック以外の場合
            return;
        }
        if (isClickPointPreviewEl(e)) {
            return;
        }
        removeAllSelectedClass(self, e);
        self.ipos = [e.pageX, e.pageY];
        if (!selectArea()) {
            var selectAreaEl = document.createElement("div");
            selectAreaEl.id = "multiImageSelect-area";
            selectAreaEl.style.left = e.pageX + "px";
            selectAreaEl.style.top = e.pageY + "px";
            document.body.appendChild(selectAreaEl);
        }
        document.body.addEventListener("mousemove", self.areaDraw);
        window.addEventListener("mouseup", self.select);
    };
    this.targetArea.addEventListener("mousedown", self.areaOpen, false);

    var isClickPointImgEl = function (e) {
        return e.target
            && e.target.nodeName
            && e.target.nodeName.toLowerCase() === "img"
            && e.target.parentElement
            && e.target.parentElement.tagName
            && e.target.parentElement.tagName.toLowerCase() === "figure"
            && e.target.parentElement.id;
    }

    // document.body.addEventListener("mouseup", function(e) {
    //     if (!isClickPointImgEl(e) && !isClickTopBar(e)) {
    //         removeAllSelectedClass(self, e);
    //     }
    // });

    document.querySelector("#navbarSearchAndTag").addEventListener("click", function (e) {
        removeAllSelectedClass(self, e);
    })

    // Wクリックで詳細画面へ遷移する
    this.targetArea.addEventListener("dblclick", function (e) {
        if (isClickPointImgEl(e)) {
            var mediaId = e.target.parentElement.id;
            window.location.href = "/UserMedia/Details?media_id=" + mediaId;
        }
    });

    // mouse計算
    this.areaDraw = function (e) {
        var selectAreaEl = selectArea();
        if (!self.ipos || selectAreaEl === null) {
            return;
        }
        var tmp, x1 = self.ipos[0],
            y1 = self.ipos[1],
            x2 = e.pageX,
            y2 = e.pageY;
        if (x1 > x2) {
            tmp = x2, x2 = x1, x1 = tmp;
        }
        if (y1 > y2) {
            tmp = y2, y2 = y1, y1 = tmp;
        }
        selectAreaEl.style.left = x1 + "px";
        selectAreaEl.style.top = y1 + "px";
        selectAreaEl.style.width = (x2 - x1) + "px";
        selectAreaEl.style.height = (y2 - y1) + "px";
    }

    var offset = function (el) {
        var r = el.getBoundingClientRect();
        return {
            top: r.top + document.body.scrollTop,
            left: r.left + document.body.scrollLeft
        }
    };
    var cross = function (a, b) {
        var aTop = offset(a).top,
            aLeft = offset(a).left,
            bTop = offset(b).top,
            bLeft = offset(b).left;
        return !(((aTop + a.offsetHeight) < (bTop))
            || (aTop > (bTop + b.offsetHeight))
            || ((aLeft + a.offsetWidth) < bLeft)
            || (aLeft > (bLeft + b.offsetWidth)));
    };
    this.select = function (e) {
        var a = selectArea();
        if (!a) {
            return;
        }
        delete (self.ipos);
        // document.body.classList.remove("multiImageSelect-noselect");
        document.body.removeEventListener("mousemove", self.areaDraw);
        window.removeEventListener("mouseup", self.select);
        var s = self.options.selectedClass;
        var targetEles = targetItems(self.itemElements);
        for (var el of targetEles) {
            if (cross(a, el) === true) {
                if (el.classList.contains(s)) {
                    el.classList.remove(s);
                    self.options.onItemDeselect && self.options.onItemDeselect(el);
                } else {
                    el.classList.add(s);
                    self.options.onItemSelect && self.options.onItemSelect(el);
                }
            }
        }
        a.parentNode.removeChild(a);
        self.options.onMultiSelectDone && self.options.onMultiSelectDone();
    }

    return this;
}

function multiSelect() {
    var multiSelectParams = {
        targetArea: "#blockGallery",
        elements: "figure",
        selectedClass: "selection-border"
    }
    new MultiImageSelectByMouse({
        targetArea: multiSelectParams.targetArea,
        elements: multiSelectParams.elements,
        selectedClass: multiSelectParams.selectedClass,
        onMultiSelectDone: function () {
            var multiSelectedClass = multiSelectParams.targetArea + " " + multiSelectParams.elements + "." + multiSelectParams.selectedClass;
            var multiSelectedImages = document.querySelectorAll(multiSelectedClass);
            window.selectedMediaIdList = [];
            window.selectedMediaSrcList = [];
            window.selectedAlbumMediaList = [];
            for (var el of multiSelectedImages) {
                var selectedMediaId;
                var selectedMediaSrc;
                var albumImage = {};
                if (el.id && !window.selectedMediaIdList.includes(el.id)) {
                    selectedMediaId = parseInt(el.id);
                    albumImage = { ...albumImage, UserMediaId: selectedMediaId };
                }
                if (el.childNodes) {
                    for (var childNode of el.childNodes) {
                        if (childNode.nodeName
                            && childNode.nodeName.toLowerCase() === "img"
                            && childNode.getAttribute("data-media-url")
                        ) {
                            selectedMediaSrc = childNode.getAttribute("data-media-url");
                            albumImage = { ...albumImage, MediaUrl: selectedMediaSrc, MediaThumbnailUrl: childNode.currentSrc, MediaFileName: childNode.alt };
                            break;
                        }
                    }
                }
                if (selectedMediaId && selectedMediaSrc) {
                    window.selectedMediaIdList.push(selectedMediaId);
                    window.selectedMediaSrcList.push(selectedMediaSrc);
                    window.selectedAlbumMediaList.push(albumImage);
                }
            }
            var selectedImageText = "";
            if (window.selectedMediaIdList.length > 0) {
                selectedImageText = window.selectedMediaIdList.length + "項目を選択中";
                document.getElementById("createMediaFolder").className = "active-link";
                document.getElementById("shareMedia").className = "active-link";
                document.getElementById("downloadMedia").className = "active-link";
                document.getElementById("deleteMedia").className = "active-link";
            } else {
                document.getElementById("createMediaFolder").className = "disabled-link";
                document.getElementById("shareMedia").className = "disabled-link";
                document.getElementById("downloadMedia").className = "disabled-link";
                document.getElementById("deleteMedia").className = "disabled-link";
            }
            document.getElementById("selectedImageNumberShow").innerText = selectedImageText;
        },
    });
}

window.selectedMediaIdList = [];
window.selectedMediaSrcList = [];
window.selectedAlbumMediaList = [];
multiSelect();


function createUserMediaFolder() {
    $('#createUserMediaFolderModal').modal('show');
}


function downloadUserMediaFile() {
    if (!window.selectedMediaSrcList || window.selectedMediaSrcList.length === 0) {
        return;
    }
    if (window.selectedMediaSrcList.length > 3) {
        generateDownloadZip();
    } else {
        var i = 1;
        for (var selectedMediaSrc of window.selectedMediaSrcList) {
            document.getElementById("iframeDownload" + i).setAttribute("src", selectedMediaSrc);
            i++;
        }
    }
}

var getFilenameFromUrl = function (imageUrl) {
    return imageUrl.split('/').pop().replace(/[\/\*\|\:\<\>\?\"\\]/gi, '');
}

function generateDownloadZip() {
    if (showLoadingIndicator) {
        showLoadingIndicator();
    }
    var links = window.selectedMediaSrcList;
    var zip = new JSZip();

    var zipFilenameUnique = (Math.random() + 1).toString(36).substring(7);
    var zipFilenameDate = `${(new Date().toJSON().slice(0, 10).replace(/-/g, ""))}`;
    var zipFilename = "download_" + zipFilenameDate + "_" + zipFilenameUnique + ".zip";

    var count = 0;
    links.forEach(function (url, i) {
        var filename = links[i];
        filename = getFilenameFromUrl(filename);
        JSZipUtils.getBinaryContent(url, function (err, data) {
            if (err) {
                if (hideLoadingIndicator) {
                    hideLoadingIndicator();
                }
                throw err;
            }
            zip.file(filename, data, { binary: true });
            count++;
            if (count == links.length) {
                zip.generateAsync({ type: 'blob' }).then(function (content) {
                    saveAs(content, zipFilename);
                    if (hideLoadingIndicator) {
                        hideLoadingIndicator();
                    }
                });
            }
        });
    });
}

function deleteUserMediaFile(media_ids) {
    if (!window.selectedMediaIdList || window.selectedMediaIdList.length === 0) {
        return;
    }
    var media_ids = window.selectedMediaIdList.toString();
    var deleteUrl = "/UserMedia/DeleteByIds?ids=" + media_ids;
    $.ajax({
        url: deleteUrl,
        method: "POST",
        processData: false,
        contentType: false,
        success: function (sResponse) {
            window.location = '/';
        }
    });
}

function showDeleteConfirmDialog() {
    $('#deleteConfirmModal').modal('show');
}

function showPreviewImage(mediaUrl) {
    if (!mediaUrl) {
        return;
    }
    $('#showPreviewImageModal').modal('show');
    $('#previewImage').attr('src', mediaUrl);
}