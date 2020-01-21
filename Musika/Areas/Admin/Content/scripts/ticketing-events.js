var pageNumber = 1;

$(document).ready(function () {
    console.log(window.location.origin);
    $('#myModal').on('hidden.bs.modal', function () {
        ClearForm();
    });
    $("#btnSearch").click(function () {
        FilterRecords(1);
    });
    $("#btnSaveChanges").click(function () {
        UpdateUser(this);
    });

    FilterRecords(pageNumber);

});

function downloadCSVFile(btn) {
    var c = new Common();
    c.AjaxCall("AdminAPI/downloadEventCSVFile?Eventid=" + btn.dataset.eventid, {}, "GET", true, function (d) {
        var win = window.open(d, '_blank');
        win.focus();
        c.ShowMessage("File Downloaded successfully.", "success");
    }, btn);
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
    var data = c.getValues("#form-search-UserEvents", "");
    data["Pageindex"] = page;
    data["Pagesize"] = 30;

    data["sortColumn"] = $("#hfSortColumn").val();
    data["sortOrder"] = $("#hfSortOrder").val();

    $("#tbodyArtists").html("");
    c.AjaxCall("TicketingAPI/GetTicketingUserEventsSearch", $.param(data), "GET", true, function (d) {
        CreateTable(d);
        Paginate(d);
    });
}


function CreateTable(data) {
    var html = "";
    debugger;
    for (var i = 0; i < data.Items.length; i++) {
        if (data.Items[i].IsApproved == 0) {
            html += "<tr>"
                + "<td>" + data.Items[i].Email + "</td>"

                + "<td>" + data.Items[i].StartDate.split("T")[0] + " " + data.Items[i].StartTime + "</td>"

                + "<td>" + data.Items[i].EventTitle + "</td>"
                + "<td>" + data.Items[i].VenueName + "</td>"
                + "<td>" + data.Items[i].ArtistName + "</td>"
                + "<td><span class=" + data.Items[i].EventID + ">" + (data.Items[i].IsApproved === 0 ? "In Active" : "Active") + "</span></td>"

                + "<td><input type='button' class='btn btn-info btn-sm Remove' value='ViewDetail' data-eventid='" + data.Items[i].EventID + "' />"

                + "<input type='button' class='btn btn-info btn-sm Activate1' value='Activate' data-eventid='" + data.Items[i].EventID + "' style='margin-left:5px;margin-top:3px;' />"

                + "<input type='button' class='btn btn-info btn-sm DownloadCSVFile' value='Export CSV' data-eventid='" + data.Items[i].EventID + "' style='margin-left:5px;margin-top:3px;' />"

                + "</tr>";
        }
        else {
            html += "<tr>"
                + "<td>" + data.Items[i].Email + "</td>"

                + "<td>" + data.Items[i].StartDate.split("T")[0] + " " + data.Items[i].StartTime + "</td>"

                + "<td>" + data.Items[i].EventTitle + "</td>"
                + "<td>" + data.Items[i].VenueName + "</td>"
                + "<td>" + data.Items[i].ArtistName + "</td>"
                + "<td><span class=" + data.Items[i].EventID + ">" + (data.Items[i].IsApproved === 0 ? "In Active" : "Active") + "</span></td>"

                + "<td><input type='button' class='btn btn-info btn-sm Remove' value='ViewDetail' data-eventid='" + data.Items[i].EventID + "' />"

                + "<input type='button' class='btn btn-info btn-sm Activate1' value='In Activate' data-eventid='" + data.Items[i].EventID + "' style='margin-left:5px;margin-top:3px;' />"

                + "<input type='button' class='btn btn-info btn-sm DownloadCSVFile' value='Export CSV' data-eventid='" + data.Items[i].EventID + "' style='margin-left:5px;margin-top:3px;' />"
                + "</tr>";
        }
    }
    $("#tbodyUserEvents").html(html);
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
            pagination += '<li class=' + (data.PageNumber == i ? "Activate" : "") + '><a href="javascript:FilterRecords(' + i + ')">' + i + '</a></li>';
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
            pagination += '<li class=' + (data.PageNumber == i ? "Activate" : "") + '><a href="javascript:FilterRecords(' + i + ')">' + i + '</a></li>';
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


function BindEvents() {
    var cmn = new Common();

    //$(".view").click(function () {

    //    cmn.AjaxCall("AdminAPI/GetTicketingEventByID?ID=" + $(this).data("eventid"), {}, "GET", true, function (d) {
    //        $("#myModal").modal("show");
    //        $("#lblRecordStatus").html(d.RecordStatus);
    //        $("#btnActivate").val(d.RecordStatus == 'Activate' ? 'InActivate' : 'Activate');
    //        $("[data-element=Email]").val(d.Email);
    //        $("[data-element=UserName]").val(d.UserName);
    //        $("[data-element=InActivatePeriod]").val(d.InActivatePeriod == null ? -1 : d.InActivatePeriod);
    //        html = "";
    //        html += '<div class="sliderimg" style="position:relative; float:left; margin-right:15px; margin-top:15px;">'
    //            + '<img src=' + d.ThumbnailURL + ' style="width:124px; height:124px"/>'
    //            + '<img src="23.111.138.246/Areas/Admin/Content/img/cross.png" data-userid="' + d.UserID + '" class="deletephoto" style="position:absolute; top:-5px; right:-5px; width:24px; display:none; cursor:pointer;" />'
    //            + '</div>';
    //        $("#imgProfile").html(html);
    //        $("[data-userid=UserId]").val(d.UserID);
    //        $("[data-element=UserId]").val(d.UserID);

    //        if (d.UserID != 1) {
    //            $("#btnActivate").show();
    //        } else {
    //            $("#btnActivate").hide();
    //        }
    //        var html = "";
    //        $(".sliderimg").hover(function () {
    //            $(this).children().eq(1).show()
    //        }, function () {
    //            $(this).children().eq(1).hide();
    //        });
    //        //InitDeletePhoto();
    //        //InitChangeStatus();
    //    }, this);

    //    //$(".Remove").click(function () {
    //    //    cmn.AjaxCall("AdminAPI/UserEventRemove/" + $(this).data("tourdateid") + "/" + $(this).data("userid"), {}, "DELETE", true, function (d) {
    //    //        var c = new Common();
    //    //        c.ShowMessage("Artist Tracking Removed successfully", "success");
    //    //        FilterRecords(pageNumber);
    //    //    }, this);
    //    //});


    $(".Remove").click(function () {
        var url = window.location.origin + "/Admin/TicketingEventDetail/index?EventID=" + $(this).data("eventid");
        location.href = url;
    });
    $('.Activate1').click(function () {
        var classobj = $(this);
        new Common().AjaxCall("AdminAPI/ChangeTicketingEventStatus?ID=" + $(this).data("eventid") + "&InactivePeriod=1", {}, "POST", true, function (d) {
            if (d) {
                var c = new Common();
                var oldText = classobj.val();
                if (oldText === "Activate") {
                    classobj.val("In Activate");
                    $('.' + classobj.data("eventid")).text("Active");
                }
                else {
                    classobj.val("Activate");
                    $('.' + classobj.data("eventid")).text("In Active");
                }
                c.ShowMessage("Status Changed successfully.", "success");
            }
        }, this);

    });

    $(".DownloadCSVFile").click(function () {
        downloadCSVFile(this);
    });


    //}
}

function ClearForm() {
    $("[data-element=Email]").val("");
    $("[data-element=UserName]").val("");
}