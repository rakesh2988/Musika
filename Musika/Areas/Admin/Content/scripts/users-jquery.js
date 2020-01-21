var pageNumber = 1;

$(document).ready(function () {
    $('#myModal').on('hidden.bs.modal', function () {
        ClearForm();
    });
    $("#btnSearch").click(function () {
        FilterRecords(1);
    });
    $("#btnSaveChanges").click(function () {
        UpdateUser(this);
    });
    $("#btnShowPassword").click(function () {
        $("#pnlPassword").show();
        $("[data-element=Password]").addClass("validate[required]");
        $("#confirmPassword").addClass("validate[required, equals[password]]");
    });

    FilterRecords(pageNumber);

    $("#btnDownloadCSVFile").click(function () {
        downloadCSVFile(this);
    });
    $("#btnImportCSVFile").click(function () {
        AddNewAds(this);
    });

});

function AddNewAds(btn) {
    var objCommon = new Common();
    var ImageSelected = $("#fileImage")[0].files[0];
    if (!ImageSelected) {
        objCommon.ShowMessage("Please select CSV File.", "error");
        return;
    }
    var fd = new FormData();
    fd.append("File", $("#fileImage")[0].files[0]);
    objCommon.AjaxCallFormData("Admin/Users/AddNewAdsWithImage", fd, true, function (response) {
        if (response == "success") {
            objCommon.ShowMessage("Uploaded successfully.., ", "success");
        }
        else {
            objCommon.ShowMessage(response, "error");
        }
    }, btn);
}


function downloadCSVFile(btn) {
    var c = new Common();
    var ddltables = $("#ddlTables").val();
    if (c.validate("form-download-csvfiles")) {
        var data = c.getValues("#form-download-csvfiles");
        data["ddlTableName"] = ddltables;
        c.AjaxCall("AdminAPI/downloadCSVFile", JSON.stringify(data), "POST", true, function (d) {
            var win = window.open(d, '_blank');
            win.focus();
        }, btn);
    }
}

function SortData(obj) {
    var span = $(obj).find("span");
    $("#hfSortColumn").val(span.attr('data-col'));
    if (span.hasClass('sortup')) {
        $("#hfSortOrder").val("DESC");
        $(".sortup").attr("class", "sort");
        $(".sortdown").attr("class", "sort");
        span.attr("class", "sortdown");
    }
    else {
        $("#hfSortOrder").val("ASC");
        $(".sortup").attr("class", "sort");
        $(".sortdown").attr("class", "sort");
        span.attr("class", "sortup");
    }
    FilterRecords(1);
}

function FilterRecords(page) {
    pageNumber = page;
    var c = new Common();
    var data = c.getValues("#form-search-users", "");
    data["Pageindex"] = page;
    data["Pagesize"] = 30;

    data["sortColumn"] = $("#hfSortColumn").val();
    data["sortOrder"] = $("#hfSortOrder").val();

    $("#tbodyUsers").html("");
    c.AjaxCall("AdminAPI/GetUsers", $.param(data), "GET", true, function (d) {
        CreateTable(d);
        Paginate(d);
    });
}

function CreateTable(data) {
debugger;
    var html = "";

    for (var i = 0; i < data.Items.length; i++) {
        html += "<tr id=tr" + data.Items[i].UserID + ">"
            + "<td>" + data.Items[i].Email + "</td>"
            + "<td>" + data.Items[i].UserName + "</td>"
            + "<td><span class='" + (data.Items[i].RecordStatus == "Active" ? "text-success" : "text-info") + "'>" + data.Items[i].RecordStatus + "</span></td>"

            + "<td>" + data.Items[i].ArtistCount + "</td>"
            + "<td>" + data.Items[i].EventCount + "</td>"
            + "<td><table><tr><td>"
            + "<input type='button' class='btn btn-info btn-sm view' value='View/Edit' data-userid='" + data.Items[i].UserID + "' />"
            + "<input type='button' class='btn btn-info btn-sm Artists' value='Artists' data-email='" + data.Items[i].Email + "' data-userid='" + data.Items[i].UserID + "' style='margin-left:5px;' />"
            + "<input type='button' class='btn btn-info btn-sm Events' value='Events' data-email='" + data.Items[i].Email + "' data-userid='" + data.Items[i].UserID + "' style='margin-left:5px;' /></td>"


            + "</td></tr><tr><td>"
            + "<input type='button' class='btn btn-info btn-sm Delete' value='Delete'   data-userid='" + data.Items[i].UserID + "' style='margin-top:5px;' />"
            + "</td></tr></table></td>";
        + "</tr>";

    }
    $("#tbodyUsers").html(html);
    BindEvents();
}

function Paginate(data) {
    var pagination = "";
    var counter = 0;
    if (data.PageNumber > 1) {
        pagination += '<li><a href="javascript:FilterRecords(' + (data.PageNumber - 1) + ')" aria-label="Previous"><span aria-hidden="true">&laquo;</span></a></li>';
    }
    else {
        pagination += '<li class="disabled"><a href="javascript:void(0);" aria-label="Previous"><span aria-hidden="true">&laquo;</span></a></li>';
    }

    var start = 0;
    if (data.PageCount > 5) {
        if (data.PageNumber > 2) {
            pagination += '<li><a href="javascript:FilterRecords(' + (data.PageNumber - 2) + ')">' + (data.PageNumber - 2) + '</a></li>';
            pagination += '<li><a href="javascript:FilterRecords(' + (data.PageNumber - 1) + ')">' + (data.PageNumber - 1) + '</a></li>';
        }
        else if (data.PageNumber == 2) {
            pagination += '<li><a href="javascript:FilterRecords(' + (data.PageNumber - 1) + ')">' + (data.PageNumber - 1) + '</a></li>';
        }
        for (var i = data.PageNumber; i < data.PageCount; i++) {
            counter++;
            pagination += '<li class=' + (data.PageNumber == i ? "active" : "") + '><a href="javascript:FilterRecords(' + i + ')">' + i + '</a></li>';
            if (data.PageNumber > 2 && counter == 3) {
                break;
            }
            else if (data.PageNumber == 2 && counter == 4) {
                break;
            }
            else if (counter == 5) {
                break;
            }
            else { }
        }
    }
    else {
        for (var i = 1; i <= data.PageCount; i++) {
            pagination += '<li class=' + (data.PageNumber == i ? "active" : "") + '><a href="javascript:FilterRecords(' + i + ')">' + i + '</a></li>';
        }
    }

    if (data.PageNumber < data.PageCount) {
        pagination += '<li><a href="javascript:FilterRecords(' + (data.PageNumber + 1) + ')" aria-label="Next"><span aria-hidden="true">&raquo;</span></a></li>';
    }
    else {
        pagination += '<li class="disabled"><a href="javascript:void(0);" aria-label="Next"><span aria-hidden="true">&raquo;</span></a></li>';
    }

    $("#pagination").html(pagination);
    $("#page").text("Page " + data.PageNumber + " of " + data.PageCount);
}

function roundTo2Decimals(numberToRound) {
    return Math.round(numberToRound * 100) / 100;
}

function BindEvents() {
    var cmn = new Common();
    $(".view").click(function () {

        cmn.AjaxCall("AdminAPI/GetUserByID?ID=" + $(this).data("userid"), {}, "GET", true, function (d) {
            console.log(d);
            $("#myModal").modal("show");
            $("#lblRecordStatus").html(d.RecordStatus);
            $("#btnActive").val(d.RecordStatus == 'Active' ? 'InActive' : 'Active');
            $("[data-element=Email]").val(d.Email);
            $("[data-element=UserName]").val(d.UserName);
            $("[data-element=InactivePeriod]").val(d.InactivePeriod == null ? -1 : d.InactivePeriod);



            html = "";
            html += '<div class="sliderimg" style="position:relative; float:left; margin-right:15px; margin-top:15px;">'
                + '<img src=' + d.ThumbnailURL + ' style="width:124px; height:124px"/>'
                + '<img src="23.111.138.246/Areas/Admin/Content/img/cross.png" data-userid="' + d.UserID + '" class="deletephoto" style="position:absolute; top:-5px; right:-5px; width:24px; display:none; cursor:pointer;" />'
                + '</div>';

            //html += '<span><img src=' + d.users.ThumbnailURL + ' /></span>' +
            //       '<span><img src="23.111.138.246/Areas/Admin/Content/img/cross.png" data-userid="' + d.users.UserID + '" class="deletephoto" style="position:absolute; top:-5px; right:-5px; width:24px;  cursor:pointer;" /></span>'

            $("#imgProfile").html(html);
            $("[data-userid=UserId]").val(d.UserID);
            $("[data-element=UserId]").val(d.UserID);

            if (d.UserID != 1) {
                $("#btnActive").show();
            } else {
                $("#btnActive").hide();
            }

            //$("#lblfrat").val(fratname);
            //$("#idrole").val(d.Role);
            //$("[data-element=FirstName]").val(d.FirstName);
            //$("[data-element=LastName]").val(d.LastName);
            //$("[data-element=UserState]").val(d.State);
            //$("[data-element=UserSchool]").val(d.SchoolId);
            //$("[data-element=UserStatus]").val(status);
            //$("[data-element=UserId]").val(d.UserId);
            // $("#imgProfile").html("<img src='" + d.ThumbnailURL + "' />");
            var html = "";
            //for (var i = 0; i < d.PhotoGallery.length; i++) {
            //    html += '<div class="sliderimg" style="position:relative; float:left; margin-right:15px; margin-top:15px;">'
            //                 + '<img src=' + d.PhotoGallery[i].ThumbnailURL + ' />'
            //                  + '<img src="http://localhost:60737/Areas/Admin/Content/img/cross.png" data-photoid="' + d.PhotoGallery[i].PhotoId + '" class="deletephoto" style="position:absolute; top:-5px; right:-5px; width:24px; display:none; cursor:pointer;" />'
            //                + '</div>';
            //}
            //$("#slider").html(html);

            $(".sliderimg").hover(function () {
                $(this).children().eq(1).show()
                //var index = $(".sliderimg").index();
                //$(this).children().eq(1).show().click(function () {
                //    d.PhotoGallery.splice(index, 1);
                //    $(this).parent().remove();
                //});
            }, function () {
                $(this).children().eq(1).hide();
            });

            //$(".sliderimg").hover(function () {
            //    var index = $(".sliderimg").index();
            //    $(this).children().eq(1).show().click(function () {
            //        d.PhotoGallery.splice(index, 1);
            //        $(this).parent().remove();
            //    });
            //}, function () {
            //    $(this).children().eq(1).hide();
            //});

            InitDeletePhoto();
            InitChangeStatus();
        }, this);

    });
    //$(".delete").click(function () {
    //    cmn.Delete("api/users/delete/" + $(this).data("userid"), {}, "DELETE", true, function (d) {
    //        if (d) {
    //            FilterRecords(pageNumber);
    //        }
    //    }, this);
    //});

    $(".Artists").click(function () {
        var url = "Admin/UserArtists/index/" + $(this).data("userid") + "/" + $(this).data("email") + "/";
        location.href = url;
    });

    $(".Events").click(function () {
        var url = "Admin/UserEvents/index/" + $(this).data("userid") + "/" + $(this).data("email") + "/";
        location.href = url;
    });

    $(".Delete").click(function () {
        var _userid = $(this).data("userid");
        cmn.ConfirmAjaxCall("AdminAPI/DeleteUser/" + $(this).data("userid"), {}, "DELETE", true, function (d) {
            if (d) {
                $('#tr' + _userid).remove();
                cmn.ShowMessage("Deleted successfully.., ", "success");
            }
        }, this);
    });

}

function InitChangeStatus() {
    $("#btnActive").click(function () {
        new Common().AjaxCall("AdminAPI/ChangeUserStatus/?ID=" + $("[data-userid=UserId]").val() + "&InactivePeriod=1", {}, "POST", true, function (d) {
            if (d) {
                var c = new Common();
                $("#btnActive").hide();
                c.ShowMessage("Status Changed successfully.", "success");
                $("#lblRecordStatus").html() == "Active" ? $("#lblRecordStatus").html("InActive") : $("#lblRecordStatus").html("Active");
            }
        }, this);

    });
}

function InitDeletePhoto() {
    $(".deletephoto").click(function () {
        new Common().AjaxCall("AdminAPI/deletephoto/" + $(this).data("userid"), {}, "DELETE", true, function (d) { });
        var c = new Common();
        c.ShowMessage("Photo Removed successfully", "success");
        $("#imgProfile").html("");
    });
}

function ClearForm() {
    $("#lblRecordStatus").html("");
    $("#lblRecordStatus2").html("");
    $("[data-element=Email]").val("");
    $("[data-element=UserName]").val("");

    $("[data-element=Password]").val("").removeClass("validate[required]");
    $("#confirmPassword").val("").removeClass("validate[required, equals[password]]");
    $("#imgProfile").html("");
    $("#pnlPassword").hide();
    $("#btnActive").hide();
    $("#btnActive").unbind();

}

function UpdateUser(btn) {
    var c = new Common();
    if (c.validate("form-user")) {
        var data = c.getValues("#form-user");
        if ($("#pnlPassword").is(":visible")) {
            data["Password"] = $("[data-element=Password]").val();
        }
        c.AjaxCall("AdminAPI/updateuser", JSON.stringify(data), "POST", true, function (d) {
            if (d.Status) {
                $('#myModal').modal("hide");
                c.ShowMessage(d.RetMessage, "success");
                FilterRecords(pageNumber);
            } else {
                c.ShowMessage(d.RetMessage, "error");
            }
        }, btn);
    }
}