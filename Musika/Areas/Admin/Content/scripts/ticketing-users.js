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
        //var $text = $(".Activate").val();
        //alert($text);
        //console.log($text);
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

});

function downloadCSVFile(btn) {
    var c = new Common();
    var ddltables = $("#ddlTables").val();
    if (c.validate("form-download-csvfiles")) {
        var data = c.getValues("#form-download-csvfiles");
        data["ddlTableName"] = ddltables;
        c.AjaxCall("AdminAPI/downloadTicketingCSVFile", JSON.stringify(data), "POST", true, function (d) {
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
    c.AjaxCall("TicketingAPI/GetTicketingUsers", $.param(data), "GET", true, function (d) {
        CreateTable(d);
        Paginate(d);
    });
}

function CreateTable(data) {
    var html = "";
    for (var i = 0; i < data.Items.length; i++) {
        if (data.Items[i].RecordStatus == "InActive") {
            html += "<tr id=tr" + data.Items[i].UserID + ">"
                + "<td>" + data.Items[i].UserType + "</td>"
                + "<td>" + data.Items[i].Email + "</td>"
                + "<td>" + data.Items[i].UserName + "</td>"
                + "<td><span class='" + (data.Items[i].RecordStatus == "Active" ? "text-success" : "text-info") + "'>" + data.Items[i].RecordStatus + "</span></td>"
                + "<td><i class='" + (data.Items[i].IsNewUser == true ? "fa fa-check-circle" : "fa fa-times-circle") + "' style='font-size:24px;color:" + (data.Items[i].IsNewUser == true ? "green" : "red") + "' ></i></td>"
                + "<td><img src='" + data.Items[i].ThumbnailURL + "' width=75 height=75></td>"
                + "<td><table><tr><td>"
                + "<input type='button' class='btn btn-info btn-sm view' value='View/Edit' data-userid='" + data.Items[i].UserID + "' />"
                + "<input type='button' class='btn btn-info btn-sm Events' value='Events' data-email='" + data.Items[i].Email + "' data-userid='" + data.Items[i].UserID + "' style='margin-left:5px;' />"
                + "<input type='button' class='btn btn-info btn-sm Delete' value='Delete'   data-userid='" + data.Items[i].UserID + "' style='margin-left:5px;' />"
                + "<input type='button' class='btn btn-info btn-sm Activate' value='Activate'   data-userid='" + data.Items[i].UserID + "' style='margin-left:5px;margin-top:3px;' />"
                + "</td></tr></table></td>";
            + "</tr>";
        }
        else {
            html += "<tr id=tr" + data.Items[i].UserID + ">"
                + "<td>" + data.Items[i].UserType + "</td>"
                + "<td>" + data.Items[i].Email + "</td>"
                + "<td>" + data.Items[i].UserName + "</td>"
                + "<td><span class='" + (data.Items[i].RecordStatus == "Active" ? "text-success" : "text-info") + "'>" + data.Items[i].RecordStatus + "</span></td>"
                + "<td><i class='" + (data.Items[i].IsNewUser == true ? "fa fa-check-circle" : "fa fa-times-circle") + "' style='font-size:24px;color:" + (data.Items[i].IsNewUser == true ? "green" : "red") + "' ></i></td>"
                + "<td><img src='" + data.Items[i].ThumbnailURL + "' width=75 height=75></td>"
                + "<td><table><tr><td>"
                + "<input type='button' class='btn btn-info btn-sm view' value='View/Edit' data-userid='" + data.Items[i].UserID + "' />"
                + "<input type='button' class='btn btn-info btn-sm Events' value='Events' data-email='" + data.Items[i].Email + "' data-userid='" + data.Items[i].UserID + "' style='margin-left:5px;' />"
                + "<input type='button' class='btn btn-info btn-sm Delete' value='Delete'   data-userid='" + data.Items[i].UserID + "' style='margin-left:5px;' />"
                + "<input type='button' class='btn btn-info btn-sm Activate' value='In Activate'   data-userid='" + data.Items[i].UserID + "' style='margin-left:5px;margin-top:3px;' />"
                + "</td></tr></table></td>";
            + "</tr>";
        }
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

        cmn.AjaxCall("AdminAPI/GetTicketingUserByID?ID=" + $(this).data("userid"), {}, "GET", true, function (d) {
            $("#myModal").modal("show");
            $("#lblRecordStatus").html(d.RecordStatus);
            $("#btnActive").val(d.RecordStatus == 'Active' ? 'InActive' : 'Active');
            $("[data-element=Email]").val(d.Email);
            $("[data-element=UserName]").val(d.UserName);
            $("[data-element=InactivePeriod]").val(d.InactivePeriod == null ? -1 : d.InactivePeriod);
            html = "";
            html += '<div class="sliderimg" style="position:relative; float:left; margin-right:15px; margin-top:15px;">'
                + '<img src=' + d.ThumbnailURL + ' style="width:124px; height:124px"/>'
                + '<img src=' + window.location.origin + '/Areas/Admin/Content/img/cross.png" data-userid="' + d.UserID + '" class="deletephoto" style="position:absolute; top:-5px; right:-5px; width:24px; display:none; cursor:pointer;" />'
                + '</div>';
            $("#imgProfile").html(html);
            $("[data-userid=UserId]").val(d.UserID);
            $("[data-element=UserId]").val(d.UserID);

            if (d.UserID != 1) {
                $("#btnActive").show();
            } else {
                $("#btnActive").hide();
            }
            var html = "";
            $(".sliderimg").hover(function () {
                $(this).children().eq(1).show()
            }, function () {
                $(this).children().eq(1).hide();
            });
            InitDeletePhoto();
            InitChangeStatus();
        }, this);

    });

    $(".Events").click(function () {
        var _userid = $(this).data("userid");
        var url = window.location.origin + "/Admin/TicketingEvent/index/"+$(this).data("userid") + "/" + $(this).data("email") + "/";
        //+ "/" + $(this).data("email") + "/";
        location.href = url;
    });

    $(".Delete").click(function () {
        var _userid = $(this).data("userid");
        cmn.ConfirmAjaxCall("AdminAPI/DeleteTicketingUser/" + $(this).data("userid"), {}, "DELETE", true, function (d) {
            if (d) {
                $('#tr' + _userid).remove();
                cmn.ShowMessage("Deleted successfully.., ", "success");
            }
        }, this);
    });

    $(".Activate").click(function () {
        var classobj = $(this);
        new Common().AjaxCall("TicketingAPI/ChangeTicketingUserStatus/?ID=" + $(this).data("userid") + "&InactivePeriod=1", {}, "POST", true, function (d) {
            if (d) {
                var c = new Common();
                var oldText = classobj.val();
                if (oldText === "Activate") {
                    classobj.val("In Activate");
                }
                else {
                    classobj.val("Activate");
                }
                //$("#btnActive").val("In Active");
                c.ShowMessage("Status Changed successfully.", "success");
                FilterRecords(pageNumber);

                //CreateTable(d);
                //Paginate(d);

                //c.AjaxCall("TicketingAPI/GetTicketingUsers", $.param(data), "GET", true, function (d) {
                //    debugger;
                //    CreateTable(d);
                //    Paginate(d);
                //});

            }
        }, this);

    });

}

function InitChangeStatus() {
    $("#btnActive").click(function () {
        new Common().AjaxCall("AdminAPI/ChangeTicketingUserStatus/?ID=" + $("[data-userid=UserId]").val() + "&InactivePeriod=1", {}, "POST", true, function (d) {
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
        new Common().AjaxCall("AdminAPI/DeleteTicketingUserPhoto/" + $(this).data("userid"), {}, "DELETE", true, function (d) { });
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
        c.AjaxCall("AdminAPI/UpdateTicketingUser", JSON.stringify(data), "POST", true, function (d) {
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