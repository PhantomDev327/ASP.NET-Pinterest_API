function viewItemList() {
    var totalImage = document.getElementById("upload_file").files.length;
    allFile = document.getElementById("upload_file").files;

    var exiteImage = document.getElementById('imageInfoListSize').value;
    if (exiteImage > 0) {
        for (var i = 0; i < exiteImage; i++) {
            var title = document.getElementById("exitTitle+" + i).value;;
            var description = document.getElementById("exitDescription+" + i).value;
            var tags = document.getElementById("exitTags+" + i).value;
            var url = document.getElementById("exitUrl+" + i).value;
            var imgName = document.getElementById("exitImgName+" + i).value;
            $('#item-hidden').append("<input hidden type='file' name='ImageInfoList[" + i + "].Images' id='imageFileInputHidden" + i + "'/><input hidden type='text' name='ImageInfoList[" + i + "].Title' value='" + title + "'/><input hidden type='text' name='ImageInfoList[" + i + "].Description' value='" + description + "'/><input hidden data-role='tagsinput' name='ImageInfoList[" + i + "].ProjectTags' value='" + tags + "'/><input type='hidden' name='ImageInfoList[" + i + "].url' value='" + url + "'/><input type='hidden' name='ImageInfoList[" + i + "].imgName' value='" + imgName +"'/>");
        }
    }

    for (var i = 0; i < totalImage; i++) {
        var id = i + parseInt(exiteImage);
        $('#item-hidden').append("<input hidden type='file' name='ImageInfoList[" + id + "].Images' id='imageFileInputHidden" + id + "'/><input hidden type='text' name='ImageInfoList[" + id + "].Title'/><input hidden type='text' name='ImageInfoList[" + id + "].Description' /><input hidden data-role='tagsinput' name='ImageInfoList[" + id + "].ProjectTags'/>");
        let container = new DataTransfer();
        container.items.add(allFile[i]);
        inputFile = document.querySelector('#imageFileInputHidden' + id);
        inputFile.files = container.files;
    }


    document.getElementById('hiddenForm').submit();

    //for (var i = 0; i < totalImage; i++) {
    //    $('#item-list').append("<hr><div class='row'><div class='col-4'><input hidden type='file' name='ImageInfoList[" + i + "].Images' id='imageFileInput" + i + "'/><img src='" + URL.createObjectURL(event.target.files[i]) + "' class='img-thumbnail img-thum' alt='ホーム画像'/></div><div class='col-8'><div class='tag_list_wrapper'><input type='text' name='ImageInfoList[" + i + "].Title' class='form-control input-control input-title px-0' placeholder='タイトル'/><input type='text' class='form-control input-control input-title px-0' placeholder='説明' name='ImageInfoList[" + i + "].Description' /><div class='tag-control-container' id='style'><p class='tag-text' style='color: #adb5bd; '>Project Tag</p><input data-role='tagsinput' name='ImageInfoList[" + i + "].ProjectTags'/></div></div></div></div>");
    //    let container = new DataTransfer();
    //    container.items.add(allFile[i]);
    //    inputFile = document.querySelector('#imageFileInput'+i);
    //    inputFile.files = container.files;
    //}
}

