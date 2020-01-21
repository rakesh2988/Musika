var pageNumber = 1;

$(document).ready(function () {
    $('#myModal').on('hidden.bs.modal', function () {
        ClearForm();
    })
    $("#btnSearch").click(function () {
        FilterRecords(1);
    });
    $("#btnSaveChanges").click(function () {
        UpdateUser(this);
    });
    
    FilterRecords(pageNumber);

});


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
    var data = c.getValues("#form-search-Artists", "");
    data["Pageindex"] = page;
    data["Pagesize"] = 30;

    data["sortColumn"] = $("#hfSortColumn").val();
    data["sortOrder"] = $("#hfSortOrder").val();

    $("#tbodyArtists").html("");
    c.AjaxCall("AdminAPI/GetUserArtists", $.param(data), "GET", true, function (d) {
        CreateTable(d);
        Paginate(d);
    });
}


function CreateTable(data) {
    var html = "";

    for (var i = 0; i < data.Items.length; i++) {
        html += "<tr>"
        + "<td>" + data.Items[i].Email + "</td>"
        + "<td>" + '<img src=' + data.Items[i].ImageURL + ' style="width:64px; height:64px"/>' + "</td>"

        + "<td>" + data.Items[i].ArtistName + "</td>"

        + "<td>" + data.Items[i].Main_Genre + "</td>"
        + "<td>" + data.Items[i].EventCount + "</td>"

        + "<td><input type='button' class='btn btn-info btn-sm Remove' value='Remove Tracking' data-userid='" + data.Items[i].UserID + "' data-artistid='" + data.Items[i].ArtistID + "' />"
        + "</tr>";
    }
    $("#tbodyArtists").html(html);
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
        for (var i = 1 ; i <= data.PageCount; i++) {
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


function BindEvents() {
    var cmn = new Common();

    $(".Remove").click(function () {
        cmn.AjaxCall("AdminAPI/UserArtistRemove/" + $(this).data("artistid") + "/" + $(this).data("userid") , {}, "DELETE", true, function (d) {
        var c = new Common();
        c.ShowMessage("Artist Tracking Removed successfully", "success");
        FilterRecords(pageNumber);
        },this);
    });



    $(".Events").click(function () {
        var url = "23.111.138.246/Admin/Events/index/" + $(this).data("userid") + "|user";
        location.href = url;
    });


}


function ClearForm() {
    $("[data-element=Email]").val("");
    $("[data-element=UserName]").val("");
}

