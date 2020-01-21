var pageNumber = 1;
$(document).ready(function () {
    $('#myModal').on('hidden.bs.modal', function () {
        ClearForm();
    });
    $("#btnSearch").click(function () {
        FilterRecords(1);
    });
    $("#btnApprove").click(function () {
        var c = new Common();
        new Common().AjaxCall("AdminAPI/Approve?ID=" + $("#eventid").val(), {}, "POST", true, function (d) {
            if (d) {
                $('.aprrovebtn').remove();
                c.ShowMessage("Event Changes Approved successfully.", "success");
            }
            else {
                c.ShowMessage("Error occcured", "error");
            }
        }, this);
        
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
    //GetEventDetail();
    //FilterRecords(pageNumber);
    //GetEventDetails($("[data-element=EventID]"));
    
    BindEvents();
});

function BindEvents() {
    //debugger;
    var cmn = new Common();

    // cmn.AjaxCall("api/TicketingAPI/GetTicketingEventDetails?EventID=" + $("#eventid").val(), {}, "GET", true, function
    cmn.AjaxCall("api/TicketingAPI/GetTicketingEventsDetailById?eventId=" + $("#eventid").val(), {}, "GET", true, function 
    (data) {
        var d = data;
        var StaffName = "";
       
        $("[data-element=CreatedBy]").val(d.CreatedBy);
        $("[data-element=CreatedOn]").val(d.CreatedOn);
        $("[data-element=EventName]").val(d.EventTitle);
        $("[data-element=VenueName]").val(d.VenueName);
        $("[data-element=Address]").val(d.Address1);
        $("[data-element=Extended_Address]").val(d.Address2);
        $("[data-element=VenueCity]").val(d.City);
        $("[data-element=VenueState]").val(d.State);
        $("[data-element=ZipCode]").val(d.ZipCode);
        $("[data-element=StartDateTime]").val(d.StartDate.split("T")[0] + ' ' + d.StartTime);
        $("[data-element=EndDateTime]").val(d.EndDate.split("T")[0] + ' ' + d.EndTime);
        for (i = 0; i < d.lstStaff.length; i++) {
            StaffName += d.lstStaff[i].UserName + "  ";
        }
        $("[data-element=Staff]").val(StaffName);
        //$("[data-element=EventImage]").val(d.EventImage);
        $("[data-src=EventImage]").attr('src', document.location.origin + "/Content/EventImages/"+d.EventImage);
        $("[data-element=EventDescription]").append(d.EventDescription);
        $("[data-element=OrganizerName]").val(d.OrganizerName);
        $("[data-element=OrganizerDescription]").append(d.OrganizerDescription);
        $("[data-element=TicketType]").val(d.TicketType);
        $("[data-element=TicketPackage]").val(d.TicketPackage);
        $("[data-element=ListingPrivacy]").val(d.ListingPrivacy);
        $("[data-element=EventType]").val(d.EventType);
        $("[data-element=EventTopic]").append(d.EventTopic);
        $("[data-element=ArtistName]").val(d.ArtistName);
        $('[data-element=EventLocation]').val(d.EventLocation);
        var strTable = "<tr><th>TicketCategory</th><th>TicketType</th><th>Quantity</th><th>Price</th><th>Package StartDate</th><th>Package EndDate</th></tr>";
        for (var i = 0; i < d.Ticket.lstTicketData.length; i++) {
            strTable += "<tr>";
            //for (var j = 0; j < d.Ticket.lstTicketData.length; j++) {
                strTable += "<td>";
                strTable += d.Ticket.lstTicketData[i].TicketCategory;
                strTable += "</td>";
                strTable += "<td>";
                strTable += d.Ticket.lstTicketData[i].TicketType;
                strTable += "</td>";
                strTable += "<td>";
                strTable += d.Ticket.lstTicketData[i].Quantity;
                strTable += "</td>";
                strTable += "<td>";
                strTable += d.Ticket.lstTicketData[i].Price;
            strTable += "</td>";
            strTable += "<td>";
            strTable += d.Ticket.lstTicketData[i].PackageStartDate;
            strTable += "</td>";
            strTable += "<td>";
            strTable += d.Ticket.lstTicketData[i].PackageEndDate;
            strTable += "</td>";

               
          //  }
            strTable += "</tr>";
        }
        $('#model_table').append(strTable);

        html = "";
        html += '<div class="sliderimg" style="position:relative; float:left; margin-right:15px; margin-top:0px;">'
            + '<img src=' + d.EventImage + ' style="width:124px; height:124px;"/>'
            + '<img src= ' + window.location.origin + '"/Areas/Admin/Content/img/cross.png" data-venueid="' + d.VenueID + '" class="deletephoto" style="position:absolute; top:-5px; right:-5px; width:24px; display:none; cursor:pointer;" />'
            + '</div>';

        $("#imgProfile").html(html);

        //BindLookUp();

        //$(".sliderimg").hover(function () {
        //    $(this).children().eq(1).show()

        //}, function () {
        //    $(this).children().eq(1).hide();
        //});
    });
}

function GetEventDetails(obj) {
    var c = new Common();
    var data = {};
    data['eventid'] = ($(obj).find('option:selected').val() == "" ? 0 : parseInt($(obj).find('option:selected').val()));
    c.AjaxCall("TicketInventory/GetEventDetails", $.param(data), "GET", true, function (d) {
        if (d.Success) {
            
            //$("[data-element=txt_TicketPackage]").val(d.TicketPackage.HashTag);
            //$("[data-element=txt_TicketType]").val(d.TicketPackage.HashTag);

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
                + '<img src="' + window.location.origin + '/Areas/Admin/Content/img/cross.png" data-venueid="' + d.VenueID + '" class="deletephoto" style="position:absolute; top:-5px; right:-5px; width:24px; display:none; cursor:pointer;" />'
                + '</div>';
            $("#eventdetails :input").attr("disabled", true);
            $('#eventdetails').slideDown(1000);
        }
    });
}


function UpdateEvent(btn) {
    debugger;
    var c = new Common();
    if (c.validate("form-Event")) {
        var data = {};
        data["TicketId"] = parseInt($("[data-element=TicketId]").val());
        data["TotalSeats"] = $("[data-element=TotalSeats]").val();
        data["TourDate"] = $("[data-element=EventId]").val();
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