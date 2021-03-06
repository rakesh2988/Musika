﻿var pageNumber = 1;

$(document).ready(function () {

    $("#datetimepicker1").datetimepicker({ minDate: new Date() });

    $.ajax({
        type: "GET",
        url: "api/TicketingAPI/GetEventNames",
        dataType: "json",
        contentType: "application/json",
        success: function (res) {
            $.each(res, function (res, value) {
                $("#sEventName").append($("<option></option>").val(value.EventTitle).html(value.EventTitle));
            });
        }
    });

    $.ajax({
        type: "GET",
        url: "api/TicketingAPI/GetEventNames",
        dataType: "json",
        contentType: "application/json",
        success: function (res) {
            $.each(res, function (res, value) {
                $("#sEventNameEdit").append($("<option></option>").val(value.EventTitle).html(value.EventTitle));
            });
        }
    });

    $(".Add").click(function () {
        $("#myModal").modal("show");
        //$("[data-TourDateID=TourDateID]").val(0);
        //$("[data-element=TourDateID]").val(0);
        //$("[data-element=artistid]").val(0);
        //$("[data-element=venueid]").val(0);

        //BindLookUp();
    });

    $("#btnSaveChanges").click(function () {
        UpdateCoupons(this);
    });

    $("#btnSearch").click(function () {
        FilterRecords(1);
    });
    FilterRecords(pageNumber);
});

function PopulatePackages(event) {
    var data = event.target.value;
    var c = new Common();
    $.ajax({
        type: "GET",
        url: "api/TicketingAPI/GetPackageNames/" + data,
        dataType: "json",
        contentType: "application/json",
        success: function (res) {
            $("#sPackagerName")
                .find('option')
                .remove()
                .end()
                .append($("<option value=\"\">Select Package</option>"));
            $.each(res, function (res, value) {
                if (value.TicketCategory !== undefined && value.TicketCategory !== '') {
                    $("#sPackagerName").append($("<option>" + value.TicketCategory + "</option>").val(value.TicketCategory).html(value.TicketCategory));
                }
            });
        }
    });
}


function PopulatePackagesForEdit(event) {
    var data = event.target.value;
    var c = new Common();
    $.ajax({
        type: "GET",
        url: "api/TicketingAPI/GetPackageNames/" + data,
        dataType: "json",
        contentType: "application/json",
        success: function (res) {
            $("#sPackagerNameEdit")
                .find('option')
                .remove()
                .end()
                .append($("<option value=\"\">Select Package</option>"));
            $.each(res, function (res, value) {
                if (value.TicketCategory !== undefined && value.TicketCategory !== '') {
                    $("#sPackagerNameEdit")
                        .append($("<option>" + value.TicketCategory + "</option>")
                            .val(value.TicketCategory)
                            .html(value.TicketCategory));
                }
            });
        }
    });
}

function UpdateCoupons(btn) {
    var c = new Common();
    if (c.validate("form-Event")) {
        var data = new FormData();
        data = c.getFormObj("form-Event");
        c.AjaxCallFormData("AdminAPI/UpdateCoupon", data, true, function (d) {
            if (d.Status) {
                $('#myModal').modal("hide");
                c.ShowMessage(d.RetMessage, "success");
                FilterRecords(pageNumber);
            }
            else {
                c.ShowMessage(d.RetMessage, "error");
            }
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
    var data = c.getValues("#form-search-Coupons", "");
    data["Pageindex"] = page;
    data["Pagesize"] = 30;

    data["sortColumn"] = $("#hfSortColumn").val();
    data["sortOrder"] = $("#hfSortOrder").val();

    $("#tbodyCoupons").html("");
    c.AjaxCall("AdminAPI/GetCoupons", $.param(data), "GET", true, function (d) {
        CreateTable(d);
        Paginate(d);
    });
}

function CreateTable(data) {
    var html = "";
    for (var i = 0; i < data.Items.length; i++) {

        html += "<tr id=tr" + data.Items[i].ID + ">"
            + "<td>" + data.Items[i].EventName + "</td>"
            + "<td>" + data.Items[i].CouponCode + "</td>"
            + "<td>" + data.Items[i].TicketCategory + "</td>"
            + "<td>" + data.Items[i].Discount + "</td>"
            + "<td>" + dateFormat(data.Items[i].ExpiryDate, "mm/dd/yyyy", "Standard") + "</td>"
            + "<td>" + dateFormat(data.Items[i].CreatedOn, "mm/dd/yyyy", "Standard") + "</td>"
            + "</tr>";
    }
    $("#tbodyCoupons").html(html);
    //BindEvents();
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