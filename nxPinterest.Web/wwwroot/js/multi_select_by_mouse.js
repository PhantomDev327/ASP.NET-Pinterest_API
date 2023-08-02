const image = new Image();
const MAX_SCALE = 5;
const SCALE_STEP = 0.2;
let imageScale = 1, imageScaleIndex = 0, prevImageScaleIndex = 0, imageLoaded = false;

// マウス関連
let mouseX, mouseY, press = false;
let mouseMoveX, mouseMoveY, mouseDragX, mouseDragY;

// 拡大・縮小後の画像表示領域
let zoomWidth, zoomHeight, zoomLeft = 0, prevZoomLeft = 0, zoomTop = 0, prevZoomHeight = 0;
let zoomLeftBuf = 0, zoomTopBuf = 0;

const animationDuration = 500; // Duration of the zoom animation in milliseconds
let startTime; // Start time of the animation

function initCol() {
    $("#thumbnail-container").empty();
    var width = window.innerWidth || document.documentElement.clientWidth || document.body.clientWidth;
    if (width > 550) {
        var itemWidth = window.itemWidth;
        var col_num = getColNum(itemWidth);
        var space = Math.floor((width % itemWidth) / 2) + 8;
        for (var i = 1; i <= col_num; i++) {
            var paddingItem = 4;
            var padding = paddingItem * 2;
            var leftSize = ((itemWidth - paddingItem) * (i - 1) + space);
            $('#thumbnail-container').append('<div id="column-' + i + '" class="image-col" style="position:absolute; width:' + itemWidth + 'px; padding:' + padding + 'px;left:' + leftSize + 'px;"></div>');
        }
    } else {
        var col_num = 6 - (window.itemWidth - 100) / 50;
        var space = (col_num == 6 || col_num == 5) ? 8 : 10;
        var itemWidth = (width - (space - 8) * (col_num - 1) - (space - 4) * 2) / col_num;
        for (var i = 1; i <= col_num; i++) {
            var paddingItem = 4;
            var padding = paddingItem * 2;
            var leftSize = (itemWidth * (i - 1) + (space - 4) + (space - 8) * (i - 1));
            $('#thumbnail-container').append('<div id="column-' + i + '" class="image-col" style="position:absolute; width:' + itemWidth + 'px; padding:' + padding + 'px;left:' + leftSize + 'px;"></div>');
        }
    }
}

function getColNum(itemWidth) {
    var col_num = Math.floor($(window).width() / itemWidth);
    return col_num;
}

$(document).ready(function () {
    var timer = null;
    $(window).off('resize');
    $(window).on('resize', function () {
        if ($(window).width() >= 415) {
            clearTimeout(timer);
            timer = setTimeout(function () {
                if (window.isFullImage) {
                    reloadImage();
                } else {
                    reloadImageAlbum();
                }
            }, 300);
        }
    });
    $(window).on('orientationchange', function () {
        clearTimeout(timer);
        timer = setTimeout(function () {
            if (window.isFullImage) {
                reloadImage();
            } else {
                reloadImageAlbum();
            }
        }, 300);
    });
})

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
            let sizeIndex = document.getElementById("changeZoomRange").value;
            if (e.target.parentElement.dataset.url) {
                selectedAlbum(mediaId, e.target.parentElement.dataset.url);
            } else {
                window.location.href = "/UserMedia/Details?media_id=" + mediaId + "&size_index=" + sizeIndex;
            }
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

    $(this.options.targetArea).keyup(function (e) {
        if (window.images && e.ctrlKey && (e.keyCode == 65 || e.keyCode == 97)) {
            window.images.forEach(function (data) {
                data.forEach(function (image) {
                    var figureItemEle = $('figure[id="' + image.mediaId + '"]');
                    figureItemEle.addClass(self.options.selectedClass);
                });
            });
            self.options.onMultiSelectDone && self.options.onMultiSelectDone();
        }
    });

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
            window.selectedSmallMediaSrcList = [];
            window.selectedAlbumMediaList = [];
            for (var el of multiSelectedImages) {
                var selectedMediaId;
                var selectedMediaSrc;
                var selectedSmallMediaSrc;
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
                            if (childNode.getAttribute("data-smallmedia-url")) {
                                selectedSmallMediaSrc = childNode.getAttribute("data-smallmedia-url");
                            }
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
                if (selectedMediaId && selectedSmallMediaSrc) {
                    window.selectedSmallMediaSrcList.push(selectedSmallMediaSrc);
                }
            }
            var selectedImageText = "";
            if (window.selectedMediaIdList.length > 0) {
                selectedImageText = window.selectedMediaIdList.length + "項目を選択中";
                console.log(selectedImageText);
                // 要素を取得
                var createMediaFolder = document.getElementById("createMediaFolder");

                // 要素が存在するかどうかをチェック
                if (createMediaFolder) {
                    // 要素が存在する場合、classNameを変更
                    createMediaFolder.className = "active-link";
                }
                // 同様のチェックを shareMedia、downloadMedia、editMultiMedia、deleteMedia にも適用
                var shareMedia = document.getElementById("shareMedia");
                if (shareMedia) {
                    shareMedia.className = "active-link";
                }
                var downloadMedia = document.getElementById("downloadMedia");
                if (downloadMedia) {
                    downloadMedia.className = "active-link";
                }
                var editMultiMedia = document.getElementById("editMultiMedia");
                if (editMultiMedia) {
                    editMultiMedia.className = "active-link";
                }
                var deleteMedia = document.getElementById("deleteMedia");
                if (deleteMedia) {
                    deleteMedia.className = "active-link";
                }

            } else {
                // 要素を取得
                var createMediaFolder = document.getElementById("createMediaFolder");

                // 要素が存在するかどうかをチェック
                if (createMediaFolder) {
                    // 要素が存在する場合、classNameを変更
                    createMediaFolder.className = "disabled-link";
                }
                // 同様のチェックを shareMedia、downloadMedia、editMultiMedia、deleteMedia にも適用
                var shareMedia = document.getElementById("shareMedia");
                if (shareMedia) {
                    shareMedia.className = "disabled-link";
                }
                var downloadMedia = document.getElementById("downloadMedia");
                if (downloadMedia) {
                    downloadMedia.className = "disabled-link";
                }
                var editMultiMedia = document.getElementById("editMultiMedia");
                if (editMultiMedia) {
                    editMultiMedia.className = "disabled-link";
                }
                var deleteMedia = document.getElementById("deleteMedia");
                if (deleteMedia) {
                    deleteMedia.className = "disabled-link";
                }
            }
            // 要素を取得
            var selectedImageNumberShow = document.getElementById("selectedImageNumberShow");

            // 要素が存在するかどうかをチェック
            if (selectedImageNumberShow) {
                // 要素が存在する場合、classNameを変更
                selectedImageNumberShow.className = selectedImageText;
            }

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
    showLoadingIndicator();
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
                hideLoadingIndicator();
                throw err;
            }
            zip.file(filename, data, { binary: true });
            count++;
            if (count == links.length) {
                zip.generateAsync({ type: 'blob' }).then(function (content) {
                    saveAs(content, zipFilename);
                    hideLoadingIndicator();
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
    if ($('#album__name').text() === '' || $('#album__name').text() === undefined) {
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
    } else {
        console.log($('#album__name').text());
        console.log(media_ids);
        var removeUrl = "/UserMedia/RemoveMediaFromAlbumByIds?albumName=" + $('#album__name').text() + "&ids=" + media_ids;
        console.log(removeUrl);
        $.ajax({
            url: removeUrl,
            method: "POST",
            processData: false,
            contentType: false,
            success: function (sResponse) {
                window.location = '/';
            }
        });
    }

}

function showDeleteConfirmDialog() {
    $('#deleteConfirmModal').modal('show');
}

$(document).ready(function () {
    $("#previousPreviewImage").prop("onclick", null).off("click");
    $("#previousPreviewImage").removeAttr("role");
    $("#nextPreviewImage").prop("onclick", null).off("click");
    $("#nextPreviewImage").removeAttr("role");
    $(".carousel-control-prev-icon").click(() => showOtherPreviewImage('previous'));
    $(".carousel-control-prev-icon").attr("role", "button");
    $(".carousel-control-next-icon").click(() => showOtherPreviewImage('next'));
    $(".carousel-control-next-icon").attr("role", "button");
});

function showPreviewImage(mediaId, mediaUrl) {
    if (!mediaId || !mediaUrl) return;

    window.previewingMediaId = mediaId;
    $('#showPreviewImageModal').modal('show');
    $('#previewImage').attr('src', mediaUrl);
    $('#previewImage').hide();

    $('#myCanvas').remove();
    $('#canvasMinusButton').remove();
    $('#canvasPlusButton').remove();
    initCanvasVariable();

    image.src = mediaUrl;
    image.onload = () => {
        $('#myCanvas').attr('width', image.naturalWidth);
        $('#myCanvas').attr('height', image.naturalHeight);
        imageLoaded = true;
        draw();
    }
    $('#previewImage').closest("#previewImageArea").append("<canvas id='myCanvas'>Test</canvas>");
    $('#previewImage').closest("#previewImageArea").append("<button id='canvasPlusButton'>+</button>");
    $('#previewImage').closest("#previewImageArea").append("<button id='canvasMinusButton'>-</button>");

    checkImageScaleForButtonValidation();

    function checkImageScaleForButtonValidation() {
        $('#canvasPlusButton').removeClass("disabled");
        $('#canvasMinusButton').removeClass("disabled");
        if (imageScale >= MAX_SCALE) {
            $('#canvasPlusButton').addClass("disabled");
        } else if (imageScale <= 1) {
            $('#canvasMinusButton').addClass("disabled");
        }
    }

    const minusButton = document.getElementById('canvasMinusButton');
    minusButton.addEventListener('click', function () {
        decreaseZoomIndex($('#previewImageArea').width() / 2, $('#previewImageArea').height() / 2);
    });
    const plusButton = document.getElementById('canvasPlusButton');
    plusButton.addEventListener('click', function () {
        increaseZoomIndex($('#previewImageArea').width() / 2, $('#previewImageArea').height() / 2);
    });

    const canvas = document.getElementById('myCanvas');

    canvas.addEventListener('mousewheel', canvasZoom);
    canvas.addEventListener('mouseover', disableScroll);
    canvas.addEventListener('mouseout', enableScroll);

    // ドラッグ操作用
    canvas.addEventListener('mousedown', function () {
        // マウスが押下された瞬間の情報を記録
        zoomLeftBuf = zoomLeft;
        zoomTopBuf = zoomTop;
        press = true;
    });
    canvas.addEventListener('mouseup', function () { press = false; });
    canvas.addEventListener('mouseout', function () { press = false; });
    canvas.addEventListener('mousemove', mouseMove);

    function initCanvasVariable() {
        imageScale = 1;
        imageScaleIndex = 0;
        prevImageScaleIndex = 0;
        imageLoaded = false;
        zoomLeft = 0;
        zoomTop = 0;
        zoomLeftBuf = 0;
        zoomTopBuf = 0;
        checkImageScaleForButtonValidation();
    }

    function draw() {
        const ctx = canvas.getContext('2d');
        if (imageLoaded) {
            ctx.drawImage(image, zoomLeft, zoomTop, canvas.width / imageScale, canvas.height / imageScale, 0, 0, canvas.width, canvas.height);
        };
    }

    function zoomInDraw(mouseX, mouseY) {
        const ctx = canvas.getContext('2d');
        if (imageLoaded) {
            if (!startTime) {
                startTime = performance.now();
            }

            // Calculate the elapsed time since the start of the animation
            const currentTime = performance.now();
            const elapsedTime = currentTime - startTime;
            // Calculate the progress of the animation from 0 to 1
            const animationProgress = Math.min(elapsedTime / animationDuration, 1);
            // Apply easing function to the animation progress
            const easingProgress = ease(animationProgress);
            // Calculate the current zoom level based on the easing progress
            const currentZoomLevel = 1 * easingProgress;

            imageScaleIndex = prevImageScaleIndex + currentZoomLevel;
            imageScale = 1 + imageScaleIndex * SCALE_STEP;
            checkImageScaleForButtonValidation();
            let realImageScale = 1 + (prevImageScaleIndex + 1) * SCALE_STEP;
            if (zoomLeft == undefined) {
                prevZoomLeft = 0;
            }
            if (zoomTop == undefined) {
                prevZoomTop = 0;
            }
            if (imageScale > MAX_SCALE) {
                imageScale = MAX_SCALE;
                checkImageScaleForButtonValidation();
                imageScaleIndex = 20;
            } else {
                zoomWidth = canvas.width / imageScale;
                zoomHeight = canvas.height / imageScale;

                zoomLeft = prevZoomLeft + mouseX * SCALE_STEP / (realImageScale * (realImageScale - SCALE_STEP)) * easingProgress;
                zoomLeft = Math.max(0, Math.min(canvas.width - zoomWidth, zoomLeft));

                zoomTop = prevZoomTop + mouseY * SCALE_STEP / (realImageScale * (realImageScale - SCALE_STEP)) * easingProgress;
                zoomTop = Math.max(0, Math.min(canvas.height - zoomHeight, zoomTop));
            }

            ctx.drawImage(image, zoomLeft, zoomTop, canvas.width / imageScale, canvas.height / imageScale, 0, 0, canvas.width, canvas.height);

            // Request the next animation frame if the animation is not finished
            if (animationProgress < 1) {
                requestAnimationFrame(() => zoomInDraw(mouseX, mouseY));
            }
        };
    }

    function zoomOutDraw(mouseX, mouseY) {
        const ctx = canvas.getContext('2d');
        if (imageLoaded) {
            if (!startTime) {
                startTime = performance.now();
            }

            // Calculate the elapsed time since the start of the animation
            const currentTime = performance.now();
            const elapsedTime = currentTime - startTime;
            // Calculate the progress of the animation from 0 to 1
            const animationProgress = Math.min(elapsedTime / animationDuration, 1);
            // Apply easing function to the animation progress
            const easingProgress = ease(animationProgress);
            // Calculate the current zoom level based on the easing progress
            const currentZoomLevel = 1 * easingProgress;

            imageScaleIndex = prevImageScaleIndex - currentZoomLevel;
            imageScale = 1 + imageScaleIndex * SCALE_STEP;
            checkImageScaleForButtonValidation();
            let realImageScale = 1 + (prevImageScaleIndex - 1) * SCALE_STEP;
            if (imageScale < 1) {
                imageScale = 1;
                checkImageScaleForButtonValidation();
                realImageScale = 1;
                zoomLeft = 0;
                zoomTop = 0;
                imageScaleIndex = 0;
            } else {
                zoomWidth = canvas.width / imageScale;
                zoomHeight = canvas.height / imageScale;

                zoomLeft = prevZoomLeft - mouseX * SCALE_STEP / (realImageScale * (realImageScale - SCALE_STEP)) * easingProgress;
                zoomLeft = Math.max(0, Math.min(canvas.width - zoomWidth, zoomLeft));

                zoomTop = prevZoomTop - mouseY * SCALE_STEP / (realImageScale * (realImageScale - SCALE_STEP)) * easingProgress;
                zoomTop = Math.max(0, Math.min(canvas.height - zoomHeight, zoomTop));
            }

            ctx.drawImage(image, zoomLeft, zoomTop, canvas.width / imageScale, canvas.height / imageScale, 0, 0, canvas.width, canvas.height);

            // Request the next animation frame if the animation is not finished
            if (animationProgress < 1) {
                requestAnimationFrame(() => zoomOutDraw(mouseX, mouseY));
            }
        };
    }

    // Easing function (You can use a different easing function for different animation effects)
    function ease(progress) {
        // You can experiment with different easing functions here
        // progress /= 0.5;
        // if (progress < 1) {
        // 	return 0.5 * progress * progress;
        // }
        // progress--;
        // return -0.5 * (progress * (progress - 2) - 1);
        return progress;
    }

    function increaseZoomIndex(mouseX, mouseY) {
        prevImageScaleIndex = imageScaleIndex;
        prevZoomLeft = zoomLeft;
        prevZoomTop = zoomTop;
        startTime = performance.now();
        requestAnimationFrame(() => zoomInDraw(mouseX, mouseY));
    }

    function decreaseZoomIndex(mouseX, mouseY) {
        prevImageScaleIndex = imageScaleIndex;
        prevZoomLeft = zoomLeft;
        prevZoomTop = zoomTop;
        startTime = performance.now();
        requestAnimationFrame(() => zoomOutDraw(mouseX, mouseY));
    }

    function canvasZoom(e) {
        // Canvas上マウス座標の取得
        let rect = e.target.getBoundingClientRect();
        mouseX = e.clientX - rect.left;
        mouseY = e.clientY - rect.top;

        if (e.wheelDelta > 0) {
            increaseZoomIndex(mouseX, mouseY);
        } else {
            decreaseZoomIndex(mouseX, mouseY);
        }
    }

    // マウス移動時の処理
    function mouseMove(e) {
        let rect = e.target.getBoundingClientRect();
        if (press) {
            // ドラッグ処理
            mouseDragX = e.clientX - rect.left;
            mouseDragY = e.clientY - rect.top;

            zoomLeft = zoomLeftBuf + (mouseMoveX - mouseDragX) / imageScale;
            zoomLeft = Math.max(0, Math.min(canvas.width - zoomWidth, zoomLeft));

            zoomTop = zoomTopBuf + (mouseMoveY - mouseDragY) / imageScale;
            zoomTop = Math.max(0, Math.min(canvas.height - zoomHeight, zoomTop));
            draw();
        } else {
            // 移動座標の記録
            mouseMoveX = e.clientX - rect.left;
            mouseMoveY = e.clientY - rect.top;
        }
    }

    // Canvas上ではブラウザのスクロールを無効に
    function disableScroll() {
        document.addEventListener("mousewheel", scrollControl, { passive: false });
    }
    function enableScroll() {
        document.removeEventListener("mousewheel", scrollControl, { passive: false });
    }
    function scrollControl(e) {
        e.preventDefault();
    }

    draw();
}

function showOtherPreviewImage(type) {
    const figureIds = [];
    const cols = $(".image-col");

    for (let i = 0; i < cols.length; i++) {
        const figures = cols.eq(i).find("figure");
        figures.each(function (index) {
            const id = $(this).attr("id");
            figureIds[index * cols.length + i] = id;
        });
    }

    if (!figureIds || !window.previewingMediaId) return;
    var imgIndex = figureIds.indexOf(window.previewingMediaId.toString());
    if (imgIndex == -1) return;

    var newImgIndex = imgIndex;
    if (type == 'previous') {
        newImgIndex = imgIndex - 1;
    } else if (type == 'next') {
        newImgIndex = imgIndex + 1;
    }

    if (newImgIndex < 0 || figureIds.length <= newImgIndex) return;

    const figureSrcList = $('#' + figureIds[newImgIndex] + ' img').attr('data-smallmedia-url');
    showPreviewImage(figureIds[newImgIndex], figureSrcList);
}

$('#main-image').hide();

foo();

$('#main-image').on('load', () => foo());

function foo() {
    $('#myCanvas').remove();
    $('#canvasMinusButton').remove();
    $('#canvasPlusButton').remove();
    initCanvasVariable();

    if (!$("#main-image").attr('src')) return;

    image.src = $("#main-image").attr('src');
    image.onload = () => {
        $('#myCanvas').attr('width', image.naturalWidth);
        $('#myCanvas').attr('height', image.naturalHeight);
        imageLoaded = true;
        draw();
    }
    $('#main-image').closest("figure").append("<canvas id='myCanvas'>Test</canvas>");
    $('#main-image').closest("figure").append("<button id='canvasPlusButton'>+</button>");
    $('#main-image').closest("figure").append("<button id='canvasMinusButton'>-</button>");

    checkImageScaleForButtonValidation();

    function checkImageScaleForButtonValidation() {
        $('#canvasPlusButton').removeClass("disabled");
        $('#canvasMinusButton').removeClass("disabled");
        if (imageScale >= MAX_SCALE) {
            $('#canvasPlusButton').addClass("disabled");
        } else if (imageScale <= 1) {
            $('#canvasMinusButton').addClass("disabled");
        }
    }

    const minusButton = document.getElementById('canvasMinusButton');
    minusButton.addEventListener('click', function () {
        decreaseZoomIndex($('#main-image').width() / 2, $('#main-image').height() / 2);
    });
    const plusButton = document.getElementById('canvasPlusButton');
    plusButton.addEventListener('click', function () {
        increaseZoomIndex($('#main-image').width() / 2, $('#main-image').height() / 2);
    });

    const canvas = document.getElementById('myCanvas');

    canvas.addEventListener('mousewheel', canvasZoom);
    canvas.addEventListener('mouseover', disableScroll);
    canvas.addEventListener('mouseout', enableScroll);

    // ドラッグ操作用
    canvas.addEventListener('mousedown', function () {
        // マウスが押下された瞬間の情報を記録
        zoomLeftBuf = zoomLeft;
        zoomTopBuf = zoomTop;
        press = true;
    });
    canvas.addEventListener('mouseup', function () { press = false; });
    canvas.addEventListener('mouseout', function () { press = false; });
    canvas.addEventListener('mousemove', mouseMove);

    function initCanvasVariable() {
        imageScale = 1;
        imageScaleIndex = 0;
        prevImageScaleIndex = 0;
        imageLoaded = false;
        zoomLeft = 0;
        zoomTop = 0;
        zoomLeftBuf = 0;
        zoomTopBuf = 0;
        checkImageScaleForButtonValidation();
    }

    function draw() {
        const ctx = canvas.getContext('2d');
        if (imageLoaded) {
            ctx.drawImage(image, zoomLeft, zoomTop, canvas.width / imageScale, canvas.height / imageScale, 0, 0, canvas.width, canvas.height);
        };
    }

    function zoomInDraw(mouseX, mouseY) {
        const ctx = canvas.getContext('2d');
        if (imageLoaded) {
            if (!startTime) {
                startTime = performance.now();
            }

            // Calculate the elapsed time since the start of the animation
            const currentTime = performance.now();
            const elapsedTime = currentTime - startTime;
            // Calculate the progress of the animation from 0 to 1
            const animationProgress = Math.min(elapsedTime / animationDuration, 1);
            // Apply easing function to the animation progress
            const easingProgress = ease(animationProgress);
            // Calculate the current zoom level based on the easing progress
            const currentZoomLevel = 1 * easingProgress;

            imageScaleIndex = prevImageScaleIndex + currentZoomLevel;
            imageScale = 1 + imageScaleIndex * SCALE_STEP;
            checkImageScaleForButtonValidation();
            let realImageScale = 1 + (prevImageScaleIndex + 1) * SCALE_STEP;
            if (zoomLeft == undefined) {
                prevZoomLeft = 0;
            }
            if (zoomTop == undefined) {
                prevZoomTop = 0;
            }
            if (imageScale > MAX_SCALE) {
                imageScale = MAX_SCALE;
                checkImageScaleForButtonValidation();
                imageScaleIndex = 20;
            } else {
                zoomWidth = canvas.width / imageScale;
                zoomHeight = canvas.height / imageScale;

                zoomLeft = prevZoomLeft + mouseX * SCALE_STEP / (realImageScale * (realImageScale - SCALE_STEP)) * easingProgress;
                zoomLeft = Math.max(0, Math.min(canvas.width - zoomWidth, zoomLeft));

                zoomTop = prevZoomTop + mouseY * SCALE_STEP / (realImageScale * (realImageScale - SCALE_STEP)) * easingProgress;
                zoomTop = Math.max(0, Math.min(canvas.height - zoomHeight, zoomTop));
            }

            ctx.drawImage(image, zoomLeft, zoomTop, canvas.width / imageScale, canvas.height / imageScale, 0, 0, canvas.width, canvas.height);

            // Request the next animation frame if the animation is not finished
            if (animationProgress < 1) {
                requestAnimationFrame(() => zoomInDraw(mouseX, mouseY));
            }
        };
    }

    function zoomOutDraw(mouseX, mouseY) {
        const ctx = canvas.getContext('2d');
        if (imageLoaded) {
            if (!startTime) {
                startTime = performance.now();
            }

            // Calculate the elapsed time since the start of the animation
            const currentTime = performance.now();
            const elapsedTime = currentTime - startTime;
            // Calculate the progress of the animation from 0 to 1
            const animationProgress = Math.min(elapsedTime / animationDuration, 1);
            // Apply easing function to the animation progress
            const easingProgress = ease(animationProgress);
            // Calculate the current zoom level based on the easing progress
            const currentZoomLevel = 1 * easingProgress;

            imageScaleIndex = prevImageScaleIndex - currentZoomLevel;
            imageScale = 1 + imageScaleIndex * SCALE_STEP;
            checkImageScaleForButtonValidation();
            let realImageScale = 1 + (prevImageScaleIndex - 1) * SCALE_STEP;
            if (imageScale < 1) {
                imageScale = 1;
                checkImageScaleForButtonValidation();
                realImageScale = 1;
                zoomLeft = 0;
                zoomTop = 0;
                imageScaleIndex = 0;
            } else {
                zoomWidth = canvas.width / imageScale;
                zoomHeight = canvas.height / imageScale;

                zoomLeft = prevZoomLeft - mouseX * SCALE_STEP / (realImageScale * (realImageScale - SCALE_STEP)) * easingProgress;
                zoomLeft = Math.max(0, Math.min(canvas.width - zoomWidth, zoomLeft));

                zoomTop = prevZoomTop - mouseY * SCALE_STEP / (realImageScale * (realImageScale - SCALE_STEP)) * easingProgress;
                zoomTop = Math.max(0, Math.min(canvas.height - zoomHeight, zoomTop));
            }

            ctx.drawImage(image, zoomLeft, zoomTop, canvas.width / imageScale, canvas.height / imageScale, 0, 0, canvas.width, canvas.height);

            // Request the next animation frame if the animation is not finished
            if (animationProgress < 1) {
                requestAnimationFrame(() => zoomOutDraw(mouseX, mouseY));
            }
        };
    }

    // Easing function (You can use a different easing function for different animation effects)
    function ease(progress) {
        // You can experiment with different easing functions here
        // progress /= 0.5;
        // if (progress < 1) {
        // 	return 0.5 * progress * progress;
        // }
        // progress--;
        // return -0.5 * (progress * (progress - 2) - 1);
        return progress;
    }

    function increaseZoomIndex(mouseX, mouseY) {
        prevImageScaleIndex = imageScaleIndex;
        prevZoomLeft = zoomLeft;
        prevZoomTop = zoomTop;
        startTime = performance.now();
        requestAnimationFrame(() => zoomInDraw(mouseX, mouseY));
    }

    function decreaseZoomIndex(mouseX, mouseY) {
        prevImageScaleIndex = imageScaleIndex;
        prevZoomLeft = zoomLeft;
        prevZoomTop = zoomTop;
        startTime = performance.now();
        requestAnimationFrame(() => zoomOutDraw(mouseX, mouseY));
    }

    function canvasZoom(e) {
        // Canvas上マウス座標の取得
        let rect = e.target.getBoundingClientRect();
        mouseX = e.clientX - rect.left;
        mouseY = e.clientY - rect.top;

        if (e.wheelDelta > 0) {
            increaseZoomIndex(mouseX, mouseY);
        } else {
            decreaseZoomIndex(mouseX, mouseY);
        }
    }

    // マウス移動時の処理
    function mouseMove(e) {
        let rect = e.target.getBoundingClientRect();
        if (press) {
            // ドラッグ処理
            mouseDragX = e.clientX - rect.left;
            mouseDragY = e.clientY - rect.top;

            zoomLeft = zoomLeftBuf + (mouseMoveX - mouseDragX) / imageScale;
            zoomLeft = Math.max(0, Math.min(canvas.width - zoomWidth, zoomLeft));

            zoomTop = zoomTopBuf + (mouseMoveY - mouseDragY) / imageScale;
            zoomTop = Math.max(0, Math.min(canvas.height - zoomHeight, zoomTop));
            draw();
        } else {
            // 移動座標の記録
            mouseMoveX = e.clientX - rect.left;
            mouseMoveY = e.clientY - rect.top;
        }
    }

    // Canvas上ではブラウザのスクロールを無効に
    function disableScroll() {
        document.addEventListener("mousewheel", scrollControl, { passive: false });
    }
    function enableScroll() {
        document.removeEventListener("mousewheel", scrollControl, { passive: false });
    }
    function scrollControl(e) {
        e.preventDefault();
    }

    draw();
}
