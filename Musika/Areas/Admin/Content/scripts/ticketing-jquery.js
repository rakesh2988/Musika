var pageNumber = 1;
$(document).ready(function () {
    $('#myModal').on('hidden.bs.modal', function () {
        ClearForm();
    })
    $("#btnSearch").click(function () {
        FilterRecords(1);
    });

    $("#btnSaveChanges").click(function () {
        UpdateEvent(this);
    });

    $("#btnGlobalConfirm").click(function () {

        var c = new Common();
        var data = {};
        data["id"] = $('#hdnid').val();
        c.AjaxCall("TicketInventory/Delete", $.param(data), "GET", true, function (d) {
            if (d.Success) {
                $('#tr' + $('#hdnid').val()).remove();
                $('#myGlobalModal').modal('hide');
                c.ShowMessage("Deleted successfully.., ", "success");
                FilterRecords(1);
            }
        });
    });
    //GetEvents();
    FilterRecords(pageNumber);
    
    $("[data-element=TourDate]").autocomplete({
        source: function (request, response) {
            $.ajax({
                url: "23.111.138.246/Admin/TicketInventory/GetEvents",
                data: { query: request.term },
                dataType: 'json',
                type: 'GET',
                success: function (data) {
                    if (data.length == 0) {
                        //$("[data-element=artistid]").val(0);
                    }
                    response($.map(data.Items, function (item) {;
                        return {
                            label: item.ArtistName + ' - ' + item.VenueName + ' - ' + dateFormat(item.EventDate, "mm/dd/yyyy, h:MM:ss TT"),
                            value: item.TourDateID
                        }
                    }));
                }
            })
        },
        select: function (event, ui) {
            $("[data-element=TourDate]").val(ui.item.label);
            $("[data-element=EventId]").val(ui.item.value);
            return false;
        },
        minLength: 1,
        delay: 300,
        focus: function (event, ui) { },
        change: function (event, ui) {
            if (ui.item == null) {
                // alert("manual");
                $("[data-element=EventId]").val(0);
                $("[data-element=TourDate]").val('');
            }
            //else
            //alert("selected");
        }
    });
});


function FilterRecords(page) {
    pageNumber = page;
    var c = new Common();
    var data = {};
    data["Pageindex"] = page;
    //data["Pagesize"] = 30;
    data["EventName"] = $("[data-element=Src_EventName]").val();
    data["ArtistName"] = $("[data-element=Src_ArtistName]").val();
    data["GenreName"] = $("[data-element=Src_GenreName]").val();
    data["SortColumn"] = $("#hfSortColumn").val();
    data["SortOrder"] = $("#hfSortOrder").val();

    $("#tbodyUsers").html("");
    c.AjaxCall("TicketInventory/GetTicketingList", $.param(data), "GET", true, function (d) {
        CreateTable(d);
        Paginate(d);
    });
}

function GetEvents() {
    var c = new Common();
    c.AjaxCall("TicketInventory/GetEvents", {}, "GET", true, function (d) {
        $(d.Items).each(function () {
            $('[data-element=TourDate]')
                .append(
                        "<option value='" + this.TourDateID + "'>" +
                            this.ArtistName + " - " + this.VenueName + " - " +
                            dateFormat(this.EventDate, "mm/dd/yyyy, h:MM:ss TT") +
                         "</option>"
                         );
        })
    });
}

function CreateTable(data) {
    var html = "";

    for (var i = 0; i < data.Items.length; i++) {
        html += "<tr id='tr" + data.Items[i].TicketId + "'>"
        + "<td>" + data.Items[i].EventName + "</td>"

        + "<td>" + data.Items[i].ArtistName + "</td>"
         + "<td>" + data.Items[i].Genre + "</td>"
        + "<td>" + data.Items[i].InAppTicketing + "</td>"

        + "<td>" + data.Items[i].TotalSeats + "</td>"

        + "<td>"
        + "<input type='button' class='btn btn-info btn-sm' onclick='GetRecord(this)'  value='View/Edit' data-id='" + data.Items[i].TicketId + "' />"
        + "<input type='button' class='btn btn-info btn-sm' onclick='Delete(this)' value='Delete' data-id='" + data.Items[i].TicketId + "' style='margin-left:5px' />"
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
    data['TicketId'] = $(obj).data('id');
    c.AjaxCall("TicketInventory/GetRecordById", $.param(data), "GET", true, function (d) {
        if (d.Success) {
            $('#eventdetails').hide();
            $('[data-element=TotalSeats]').val(d.TotalSeats);
            $('[data-element=TourDate]').val(d.TourDate);
            $('[data-element=EventId]').val(d.TourDateId);
            $('[data-element=TicketId]').val(d.Id);
            $("#tblbody").empty();
            $(d.TicketsList).each(function (i, v) {
                AddTicketsList(v["SeatFrom"], v["SeatTo"], v["SectionType"], v["SectionPrice"])
            })
            //GetEventDetails($("[data-element=TourDate]"))

            $('#myModal').modal('show')
        }
    });
}


function GetEventDetails(obj) {
    var c = new Common();
    var data = {};
    data['eventid'] = ($(obj).find('option:selected').val() == "" ? 0 : parseInt($(obj).find('option:selected').val()));
    c.AjaxCall("TicketInventory/GetEventDetails", $.param(data), "GET", true, function (d) {
        if (d.Success) {

            $("[data-element=txt_HashTag]").val(d.EventDetails.HashTag);
            $("[data-element=txt_TicketURL]").val(d.EventDetails.TicketURL);

            $("[data-element=txt_EventName]").val(d.EventDetails.EventName);
            $("[data-element=txt_SeatGeek_TourID]").val(d.EventDetails.SeatGeek_TourID);
            $("[data-element=txt_ArtistName]").val(d.EventDetails.ArtistName);

            //$("[data-element=txt_Datetime_Local]").val(dateFormat(d.EventDetails.Datetime_Local, "mm/dd/yyyy HH:MM"));
            //$("[data-element=Datetime_Local]").val(d.Datetime_Local);
            $("[data-element=txt_VenueName]").val(d.EventDetails.VenueName);

            $("[data-element=txt_SeatGeek_VenuID]").val(d.EventDetails.SeatGeek_VenuID);
            $("[data-element=txt_Extended_Address]").val(d.EventDetails.Extended_Address);
            $("[data-element=txt_Address]").val(d.EventDetails.Address);
            $("[data-element=txt_VenueCountry]").val(d.EventDetails.VenueCountry);
            $("[data-element=txt_VenueCity]").val(d.EventDetails.VenueCity);
            $("[data-element=txt_VenueState]").val(d.EventDetails.VenueState);
            $("[data-element=txt_Postal_Code]").val(d.EventDetails.Postal_Code);
            $("[data-element=txt_VenueLat]").val(d.EventDetails.VenueLat);
            $("[data-element=txt_VenueLong]").val(d.EventDetails.VenueLong);
            $("[data-element=txt_Timezone]").val(d.EventDetails.Timezone);

            $("[data-element=txt_InAppTicketing]").prop('checked', d.EventDetails.InAppTicketing);
            $("[data-element=txt_TotalSeats]").val(d.EventDetails.TotalSeats);

            if (d.IsDeleted) {
                $("#btnDeleteEvent").attr('style', 'background-color:blue; border-color:blue;');
                $("#btnDeleteEvent").attr('value', 'Undo Delete');
            }
            else {
                $("#btnDeleteEvent").attr('style', 'background-color:Red; border-color:Red;');
                $("#btnDeleteEvent").attr('value', 'Delete Event');
            }



            html = "";
            html += '<div class="sliderimg" style="position:relative; float:left; margin-right:15px; margin-top:0px;">'
                             + '<img src=' + d.ImageURL + ' style="width:124px; height:124px;"/>'
                              + '<img src="23.111.138.246/Areas/Admin/Content/img/cross.png" data-venueid="' + d.VenueID + '" class="deletephoto" style="position:absolute; top:-5px; right:-5px; width:24px; display:none; cursor:pointer;" />'
                            + '</div>';
            $("#eventdetails :input").attr("disabled", true);
            $('#eventdetails').slideDown(1000);
        }
    });
}


function UpdateEvent(btn) {
    var c = new Common();
    if (c.validate("form-Event")) {
        var data = {};
        data["TicketId"] = parseInt($("[data-element=TicketId]").val());
        data["TotalSeats"] = $("[data-element=TotalSeats]").val();
        data["TourDate"] = $("[data-element=EventId]").val()
        data["List"] = new Array();
        $("#tblbody tr").each(function () {
            var item = {};
            item["SeatFrom"] = $(this).find('input.seatfrom').val();
            item["SeatTo"] = $(this).find('input.seatto').val();
            item["SectionType"] = $(this).find('input.section').val();
            item["SectionPrice"] = $(this).find('input.price').val();
            data.List.push(item);
        })
        c.AjaxCall("TicketInventory/UpdateTicket", JSON.stringify(data), "POST", true, function (d) {
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
    $('[data-element=TicketId]').val(0);
    $('[data-element=TotalSeats]').val('');
    $('[data-element=TourDate]').val('');
    $('[data-element=FromSeat]').val('');
    $('[data-element=ToSeat]').val('');
    $('[data-element=Section]').val('');
    $('#tblbody').empty();
    $('#eventdetails').hide();
    $('#myModal').modal('show');
}

function Delete(obj) {
    var _id = $(obj).data("id");
    $('#hdnid').val(_id)
    $('#myGlobalModal').modal('show');
}

function AddTicketDetails() {
    var c = new Common();
    var seatfrom = $('[data-element=FromSeat]').val();
    var seatto = $('[data-element=ToSeat]').val();
    var sectionval = $('[data-element=Section]').val();
    var sectiontext = $('[data-element=Section] option:selected').text();
    var price = $('[data-element=Section] option:selected').data('price');

    if (seatfrom == '' || parseInt(seatfrom) <= 0) {
        c.ShowMessage("from seat should be greater then 0 (Required)", "warning");
        return false;
    }

    if (seatto == '' || parseInt(seatto) <= 0) {
        c.ShowMessage("To seat should be greater then 0 (Required)", "warning");
        return false;
    }

    if (sectionval == '' || sectionval == undefined) {
        c.ShowMessage("Select Section (Required)", "warning");
        return false;
    }

    //check duplication
    var $rowcheck = true;
    $("#tblbody tr").each(function () {
        var gseatfrom = parseInt($(this).find('input.seatfrom').val());
        var gseatto = parseInt($(this).find('input.seatto').val());
        if ((parseInt(seatfrom) >= gseatfrom && parseInt(seatfrom) <= gseatto)) {
            c.ShowMessage("Ticket duplication is not allowed.", "warning");
            $rowcheck = false;
            return false;
        }

        if ((parseInt(seatto) >= gseatfrom && parseInt(seatto) <= gseatto)) {
            c.ShowMessage("Ticket duplication is not allowed.", "warning");
            $rowcheck = false;
            return false;
        }

    })
    if ($rowcheck) {
        AddTicketsList(seatfrom, seatto, sectiontext, price);
    }
}

function AddTicketsList(seatfrom, seatto, sectiontext, price) {
    var html = "<tr>";
    html += "<td><input type='hidden' class='seatfrom' value='" + seatfrom + "' />" + seatfrom + "</td>";
    html += "<td><input type='hidden' class='seatto' value='" + seatto + "' />" + seatto + "</td>";
    html += "<td><input type='hidden' class='section' value='" + sectiontext + "' />" + sectiontext + "</td>";
    html += "<td><input type='hidden' class='price' value='" + price + "' />" + price + "</td>";
    html += "<td><input type=button' class='btn btn-primary btn-sm' onclick='RemoveRow(this)' value='Remove'></td>";
    html += "</tr>";
    $("#tblbody").append(html);
}

function RemoveRow(obj) {
    $(obj).parent().parent().remove();
}