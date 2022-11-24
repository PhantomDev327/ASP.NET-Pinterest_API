var pageSharedAlbumController = function () {

	window.page = 1;
	window.itemWidth = 250;
	window.images = new Array();
	var pathSharedUrl = window.location.pathname.split("/").pop();

	this.initialize = function () {
		registerEvents();
	};

	function registerEvents() {

		$(document).ready(function () {
			$(window).css('min-width', 500);
			initCol();
			loadImgaeAlbum(pathSharedUrl);
			$(window).scroll(function () {
				if ($(window).scrollTop() + $(window).height() == $(document).height()) {
					loadImgaeAlbum(pathSharedUrl);
				}
			});
			var timer = null;
			$(window).on('resize', function () {
				clearTimeout(timer);
				timer = setTimeout(function () {
					reloadImage();
				}, 300);
			});
		});
		$('body').on('click', '.previewButton', function (e) {
			e.preventDefault();
			var mediaUrl = $(this).data('mediaurl');
			$('#showPreviewImageModal').modal('show');
			$('#previewImage').attr('src', mediaUrl);

		});


		$('body').on('click', '#downloadMedia', function (e) {
			e.preventDefault();
			downloadUserMediaFile();

		});


		$('body').on('click', '#changeZoomOut', function (e) {
			e.preventDefault();
			var zoomSize = getCurrentZoomRange();
			if (zoomSize <= 0) {
				return;
			}
			document.getElementById("changeZoomRange").value = zoomSize - 1;
			changeZoom();

		});


		$('body').on('click', '#changeZoomIn', function (e) {
			e.preventDefault();
			var zoomSize = getCurrentZoomRange();
			if (zoomSize >= 6) {
				return;
			}
			document.getElementById("changeZoomRange").value = zoomSize + 1;
			changeZoom();

		});

		$('body').on('change', '#changeZoomRange', function (e) {
			e.preventDefault();			
			changeZoom();

		});
	};
	function reloadImage() {
		initCol();
		window.images.forEach(function (data) {
			data.forEach(function (value) {
				appendData(value);
			});
		});
	}
	function changeZoom() {
		var zoomSize = getCurrentZoomRange();
		var rangeWidth = 50;
		window.itemWidth = 100 + rangeWidth * zoomSize;

		var timer = null;
		clearTimeout(timer);
		timer = setTimeout(function () {
			reloadImage();
		}, 300);
	}
	function getCurrentZoomRange() {
		var changeZoomRange = document.getElementById("changeZoomRange");
		return Number(changeZoomRange.value);
	}
	function getColNum(itemWidth) {
		var col_num = Math.floor($(window).width() / itemWidth);
		return col_num;
	}

	function initCol() {
		var itemWidth = window.itemWidth;
		$("#thumbnail-container").empty();
		var col_num = getColNum(itemWidth);
		var width = window.innerWidth || document.documentElement.clientWidth || document.body.clientWidth;
		var space = Math.floor((width % itemWidth) / 2) + 8;
		for (var i = 1; i <= col_num; i++) {
			var paddingItem = 4;
			var padding = paddingItem * 2;
			var leftSize = ((itemWidth - paddingItem) * (i - 1) + space);
			$('#thumbnail-container').append('<div id="column-' + i + '" class="image-col" style="position:absolute; width:' + itemWidth + 'px; padding:' + padding + 'px;left:' + leftSize + 'px;"></div>');
		}
	}

	function findShortestCol() {
		var count = $("#thumbnail-container").children().length;
		var shortest = $("#column-1");
		for (var i = 1; i <= count; i++) {
			if (Math.floor(shortest.height()) > Math.floor($("#column-" + i + "").height())) {
				shortest = $("#column-" + i + "");
			}
		}
		return shortest;
	}

	function appendData(value) {
		var newImgEl = document.createElement('img');
		var previewButtonEl = '<span class="previewButton" data-mediaurl="' + value.mediaSmallUrl + '"><i class="bi bi-zoom-in fa-lg px-2"></i></span>';
		newImgEl.addEventListener('load', function () {
			shortest = findShortestCol();
			shortest.append('<figure id="' + value.mediaId + '"><figcaption><p class="title">' + value.mediaTitle + '</p><p class="description">' + value.mediaDescription + '</p></figcaption></figure>');
			var figureItemEle = $('figure[id="' + value.mediaId + '"]');
			figureItemEle.append(newImgEl);
			figureItemEle.append(previewButtonEl);
		});
		newImgEl.setAttribute('src', value.mediaThumbnailUrl);
		newImgEl.setAttribute('alt', value.mediaFileName);
		newImgEl.setAttribute('data-media-url', value.mediaUrl);
		newImgEl.setAttribute('data-smallmedia-url', value.mediaSmallUrl);
	}

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
			return !(((aTop + a.offsetHeight) < (bTop)) ||
				(aTop > (bTop + b.offsetHeight)) ||
				((aLeft + a.offsetWidth) < bLeft) ||
				(aLeft > (bLeft + b.offsetWidth)));
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
						albumImage = {
							...albumImage,
							UserMediaId: selectedMediaId
						};
					}
					if (el.childNodes) {
						for (var childNode of el.childNodes) {
							if (childNode.nodeName &&
								childNode.nodeName.toLowerCase() === "img" &&
								childNode.getAttribute("data-media-url")
							) {
								selectedMediaSrc = childNode.getAttribute("data-media-url");
								albumImage = {
									...albumImage,
									MediaUrl: selectedMediaSrc,
									MediaThumbnailUrl: childNode.currentSrc,
									MediaFileName: childNode.alt
								};
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
				if (window.selectedMediaIdList.length > 0) {
					selectedImageText = window.selectedMediaIdList.length + "項目を選択中";

					document.getElementById("downloadMedia").className = "active-link";

				} else {

					document.getElementById("downloadMedia").className = "disabled-link";

				}

			},
		});
	}

	window.selectedMediaIdList = [];
	window.selectedMediaSrcList = [];
	window.selectedAlbumMediaList = [];
	multiSelect();


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
					throw err;
					console.info(err);
				}
				zip.file(filename, data, {
					binary: true
				});
				count++;
				if (count == links.length) {
					zip.generateAsync({
						type: 'blob'
					}).then(function (content) {
						saveAs(content, zipFilename);
					});
				}
			});
		});
	}

	function loadImgaeAlbum(url) {
		showLoader();
		$.ajax({
			url: "/UserAlbum/GetAlbumSharedLink/",
			method: "POST",
			data: {
				pageIndex: window.page,
				pathUrl: url
			},
			success: function (result) {
				if (result.statusCode === 200) {
					window.images.push(result.data);
					$.each(result.data, function (index, value) {
						appendData(value);
					});
					
				} else {

					document.getElementById("content__error").innerText = result.message;
					document.getElementById("error").innerText = "メッセージ";
				}
				window.page = window.page + 1;
				hideLoader();
			},
			error: function () {
				window.images.forEach(function (data) {
					data.forEach(function (value) {
						appendData(value);
					});
				});
				window.page = window.page + 1;
				hideLoader();
			}
		});
	}

}