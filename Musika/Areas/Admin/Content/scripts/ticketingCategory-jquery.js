var pageNumber = 1;
$(document).ready(function () {
    $('#myModal').on('hidden.bs.modal', function () {
        ClearForm();
    })
    $("#btnSearch").click(function () {
        FilterRecords(1);
    });

    $("#btnSaveChanges").click(function () {
        UpdateCategory(this);
    });

    $("#btnGlobalConfirm").click(function () {

        var c = new Common();
        var data = {};
        data["id"] = $('#hdnid').val();
        c.AjaxCall("TicketingCategory/Delete", $.param(data), "GET", true, function (d) {
            if (d.Success) {
                $('#tr' + $('#hdnid').val()).remove();
                $('#myGlobalModal').modal('hide');
                c.ShowMessage("Deleted successfully.., ", "success");
                FilterRecords(1);
            }
        });
    });

    FilterRecords(pageNumber);

});


function FilterRecords(page) {
    pageNumber = page;
    var c = new Common();
    var data = {};
    data["Pageindex"] = page;
    data["Name"] = $("[data-element=Name]").val();
    data["SortColumn"] = $("#hfSortColumn").val();
    data["SortOrder"] = $("#hfSortOrder").val();

    $("#tbodyUsers").html("");
    c.AjaxCall("TicketingCategory/GetTicketCategoryList", $.param(data), "GET", true, function (d) {
        CreateTable(d);
        Paginate(d);
    });
}


function CreateTable(data) {
    var html = "";

    for (var i = 0; i < data.Items.length; i++) {
        html += "<tr id='tr" + data.Items[i].CategoryId + "'>"
        + "<td>" + data.Items[i].CategoryName + "</td>"

        + "<td>" + data.Items[i].CategoryPrice + "</td>"
         + "<td>" + data.Items[i].RecordStatus + "</td>"

        + "<td>"
        + "<input type='button' class='btn btn-info btn-sm' onclick='GetRecord(this)'  value='View/Edit' data-id='" + data.Items[i].CategoryId + "' />"
        + "<input type='button' class='btn btn-info btn-sm' onclick='Delete(this)' value='Delete' data-id='" + data.Items[i].CategoryId + "' style='margin-left:5px' />"
        + "</td>"
        + "</tr>";
    }
    $("#tbodyUsers").html(html);
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


function GetRecord(obj) {
    var c = new Common();
    var data = {};
    data['id'] = $(obj).data('id');
    c.AjaxCall("TicketingCategory/GetRecordById", $.param(data), "GET", true, function (d) {
        if (d.Success) {
            $('[data-element=CategoryId]').val(d.Model["CategoryId"]);
            $('[data-element=CategoryName]').val(d.Model["CategoryName"]);
            $('[data-element=CategoryPrice]').val(d.Model["CategoryPrice"]);
            $('[data-element=RecordStatus]').val(d.Model["RecordStatus"]);
            $('#eventdetails').hide();
            $('#myModal').modal('show')
        }
    });
}

function UpdateCategory(btn) {
    var c = new Common();
    if (c.validate("form-category")) {
        var data = {};
        data = c.GetFormValues("#form-category");

        c.AjaxCall("TicketingCategory/UpdateTicket", JSON.stringify(data), "POST", true, function (d) {
            if (d.Success) {
                $('#myModal').modal("hide");
                c.ShowMessage(d.RetMessage, "success");
                FilterRecords(pageNumber);
            } else {
                c.ShowMessage(d.RetMessage, "error");
            }
        }, btn);
    }
}

function Add() {
    $('[data-element=CategoryId]').val(0);
    $('[data-element=CategoryName]').val('');
    $('[data-element=CategoryPrice]').val('');
    $('[data-element=RecordStatus]').val('Active');
    $('#myModal').modal('show');
}

function Delete(obj) {
    var _id = $(obj).data("id");
    $('#hdnid').val(_id)
    $('#myGlobalModal').modal('show');
}