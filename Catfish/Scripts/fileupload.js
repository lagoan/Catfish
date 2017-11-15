﻿function updateFileListView(data, deleteApiUrl, thumbnailPanelCssId, messageBoxCssId) {
    data = JSON.parse(data, thumbnailPanelCssId);
    for (var i = 0; i < data.length; ++i) {
        var d = data[i];
        var eleId = d.Guid.substr(0, d.Guid.length - 4);
        var ele = '<div class="fileThumbnail" id="' + eleId + '" > <img src="' + d.Thumbnail + '" alt="' + d.FileName + '" />' +
            '<button class="glyphicon glyphicon-remove" onclick="deleteFile(\'' + d.Guid + '\',\'' + deleteApiUrl + '\',\'' + messageBoxCssId + '\');"></button>' +
            '<div class="label"><a href="' + d.Url + '">' + d.FileName + '</a></div>' +
            '</div>';

        $(thumbnailPanelCssId).append(ele);
    }
}

function uploadFile(itemId, uploadApiUrl, deleteApiUrl, uploadFieldCssId, uploadButtonCssId, progressBarCssId, messageBoxCssId, thumbnailPanelCssId) {
    var myFrm = new FormData();     //create a new form
    myFrm.append("itemId", itemId);

    var uploadField = $(uploadFieldCssId)[0];     //grab the FileUpload object

    for (var i = 0; i < uploadField.files.length; i++) {
        myFrm.append("inputFile" + i, uploadField.files[i]);
    }

    $(progressBarCssId).show();
    $(uploadButtonCssId).attr('disabled', 'disabled');
    $(uploadFieldCssId).attr('disabled', 'disabled');

    var oReg = new XMLHttpRequest();

    var stateChange = function (data) {
        if (oReg.readyState === 4) {  //after successfull execute the function then it will execute what ever inside this if {}
            if (oReg.status === 200) {
                //Updating the value of the hidden field which carries the ID of this FileUpload object in the page
                updateFileListView(data, deleteApiUrl, thumbnailPanelCssId, messageBoxCssId);
                $(messageBoxCssId).text("");
            }
            else {
                //Error
                $(messageBoxCssId).text("File upload failed: " + oReg.statusText);
            }
            $(uploadFieldCssId).val("");
            $(progressBarCssId).hide();
            $(uploadButtonCssId).prop('disabled', false);
            $(uploadFieldCssId).prop('disabled', false);
        }
    };

    if (navigator.userAgent.toLowerCase().indexOf('firefox') > -1) {
        oReg.onload = function (event) { stateChange(event.target.response); };
    } else {
        oReg.onreadystatechange = function (data) { stateChange(data.srcElement.responseText); };
    }
    oReg.open('POST', uploadApiUrl);
    oReg.send(myFrm);

}// END function uploadFile()

function deleteFile(guidName, deleteApiUrl, messageBoxCssId) {
    if (confirm("Delete file?") == false)
        return;

    var myFrm = new FormData();     //create a new form
    myFrm.append("guidName", guidName);

    var oReg = new XMLHttpRequest();
    var stateChange = function (data) {
        if (oReg.readyState === 4) {  //after successfull execute the function then it will execute what ever inside this if {}
            if (oReg.status === 200) {
                var guid = JSON.parse(data)[0];
                var eleId = guid.substr(0, guid.length - 4)
                $("#" + eleId).remove();
            }
            else {
                //Error
                $(messageBoxCssId).text("File deletion failed: " + oReg.statusText);
            }
        }
    };

    if (navigator.userAgent.toLowerCase().indexOf('firefox') > -1) {
        oReg.onload = function (event) { stateChange(event.target.response); };
    } else {
        oReg.onreadystatechange = function (data) { stateChange(data.srcElement.responseText); };
    }

    oReg.open('POST', deleteApiUrl);
    oReg.send(myFrm);
}