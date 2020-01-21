var pageNumber = 1;


// Read a page's GET URL variables and return them as an associative array.
function getVars(url) {
    var formData = new FormData();
    var split;
    $.each(url.split("&"), function (key, value) {
        split = value.split("=");
        formData.append(split[0], decodeURIComponent(split[1].replace(/\+/g, " ")));
    });

    return formData;
}

// Variable to store your files
var files;

// Grab the files and set them to our variable
function prepareUpload(event) {
    files = event.target.files;
}


$(document).ready(function () {
    $('#myModal').on('hidden.bs.modal', function () {
        ClearForm();
    })
    $("#btnSearch").click(function () {
        FilterRecords(1);
    });
    $("#btnArchive").click(function () {
        FilterRecords(1, document.getElementById('btnArchive').checked);
    });
    $("#btnDuplicate").click(function () {
        FilterRecords(1, false, document.getElementById('btnDuplicate').checked);
    });
    $("#btnDeleted").click(function () {
        FilterRecords(1, false, false, document.getElementById('btnDeleted').checked);
    });
    $("#btnSaveChanges").click(function () {
        UpdateEvent(this);
    });

    $("#btnDeleteEvent").click(function () {
        DeleteEvent($("[data-element=TourDateID]").val());
    });

    $(".Add").click(function () {
        $("#myModal").modal("show");
        $("[data-TourDateID=TourDateID]").val(0);
        $("[data-element=TourDateID]").val(0);
        $("[data-element=artistid]").val(0);
        $("[data-element=venueid]").val(0);

        BindLookUp();
    });


    $(document).delegate('#input-image', 'change', prepareUpload);


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


function FilterRecords(page, archive, duplicate, deleted) {
    pageNumber = page;
    var c = new Common();
    var data = c.getValues("#form-search-Events", "");
    data["Pageindex"] = page;
    data["Pagesize"] = 30;

    data["sortColumn"] = $("#hfSortColumn").val();
    data["sortOrder"] = $("#hfSortOrder").val();

    if (archive || document.getElementById('btnArchive').checked) {
        data["archive"] = true;
    }

    if (duplicate || document.getElementById('btnDuplicate').checked) {
        data["duplicate"] = true;
    }

    if (deleted || document.getElementById('btnDeleted').checked) {
        data["deleted"] = true;
    }

    $("#tbodyArtists").html("");
    c.AjaxCall("AdminAPI/GetEvents", $.param(data), "GET", true, function (d) {
        CreateTable(d);
        Paginate(d);
    });
}


function CreateTable(data) {
    var html = "";

    for (var i = 0; i < data.Items.length; i++) {
        html += "<tr>"
        + "<td>" + data.Items[i].EventName + "</td>"
        + "<td>" + dateFormat(data.Items[i].Datetime_Local, "mm/dd/yyyy, h:MM:ss TT") + "</td>"

        + "<td>" + data.Items[i].ArtistName + "</td>"
         + "<td>" + data.Items[i].Main_Genre + "</td>"
        + "<td>" + data.Items[i].VenueName + "</td>"

        + "<td>" + data.Items[i].VenueCountry + "</td>"

        + "<td>" + data.Items[i].VenueCity + "</td>"

        + "<td>" + data.Items[i].AttendingCount + "</td>"

        + "<td><input type='button' class='btn btn-info btn-sm view' value='View/Edit' data-tourdateid='" + data.Items[i].TourDateID + "' />";

        if (data.Items[i].HotTour == 'Active') {
            html += "<td class='text-center'><div class='checkbox' style='padding-left:0px !important; margin-left:-10px !important'><input type='checkbox' id='opt" + data.Items[i].TourDateID + "' value='1' data-tourdateid='" + data.Items[i].TourDateID + "' class='checkUse' checked></div>"
        } else {
            html += "<td class='text-center'><div class='checkbox' style='padding-left:0px !important; margin-left:-10px !important'><input type='checkbox' id='opt" + data.Items[i].TourDateID + "' value='1' data-tourdateid='" + data.Items[i].TourDateID + "' class='checkUse'></div>"
        }
        + "</td>"
        + "</tr>";
    }
    $("#tbodyEvents").html(html);
    BindEvents();
    initUseHotTour();
}

function initUseHotTour() {
    $(".checkUse").click(function () {
        var id = $(this).data("tourdateid");
        var data = {
            TourDateID: id
        };
        var c = new Common();


        c.AjaxCall("AdminAPI/UpdateHotTour", JSON.stringify(data), "POST", true, function (d) {

        });
    });
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
    $(".view").click(function () {

        cmn.AjaxCall("AdminAPI/GetEventsByID?ID=" + $(this).data("tourdateid"), {}, "GET", true, function (d) {
            console.log(d);
            $("#myModal").modal("show");


            $("[data-element=HashTag]").val(d.HashTag);
            $("[data-element=TicketURL]").val(d.TicketURL);

            $("[data-element=EventName]").val(d.EventName);
            $("[data-element=SeatGeek_TourID]").val(d.SeatGeek_TourID);
            $("[data-element=ArtistName]").val(d.ArtistName);
            $("[data-element=artistid]").val(d.ArtistID);

            $("[data-element=Datetime_Local]").val(dateFormat(d.Datetime_Local, "mm/dd/yyyy HH:MM"));
            //$("[data-element=Datetime_Local]").val(d.Datetime_Local);
            $("[data-element=VenueName]").val(d.VenueName);
            $("[data-element=venueid]").val(d.VenueID);

            $("[data-element=SeatGeek_VenuID]").val(d.SeatGeek_VenuID);
            $("[data-element=Extended_Address]").val(d.Extended_Address);
            $("[data-element=Address]").val(d.Address);
            $("[data-element=VenueCountry]").val(d.VenueCountry);
            $("[data-element=VenueCity]").val(d.VenueCity);
            $("[data-element=VenueState]").val(d.VenueState);
            $("[data-element=Postal_Code]").val(d.Postal_Code);
            $("[data-element=VenueLat]").val(d.VenueLat);
            $("[data-element=VenueLong]").val(d.VenueLong);
            $("[data-element=Timezone]").val(d.Timezone);
            $('[data-element=TotalSeats]').val(d.TotalSeats);
            if (d.IsDeleted) {
                $("#btnDeleteEvent").attr('style', 'background-color:blue; border-color:blue;');
                $("#btnDeleteEvent").attr('value', 'Undo Delete');
            }
            else {
                $("#btnDeleteEvent").attr('style', 'background-color:Red; border-color:Red;');
                $("#btnDeleteEvent").attr('value', 'Delete Event');
            }


            $("[data-tourdateid=TourDateID]").val(d.TourDateID);
            $("[data-element=TourDateID]").val(d.TourDateID);

            html = "";
            html += '<div class="sliderimg" style="position:relative; float:left; margin-right:15px; margin-top:0px;">'
                             + '<img src=' + d.ImageURL + ' style="width:124px; height:124px;"/>'
                              + '<img src="http://23.111.138.246/Areas/Admin/Content/img/cross.png" data-venueid="' + d.VenueID + '" class="deletephoto" style="position:absolute; top:-5px; right:-5px; width:24px; display:none; cursor:pointer;" />'
                            + '</div>';


            $("#imgProfile").html(html);


            BindLookUp();




            $(".sliderimg").hover(function () {
                $(this).children().eq(1).show();

            }, function () {
                $(this).children().eq(1).hide();
            });



            InitDeletePhoto();
        }, this);

    });


    $(".Events").click(function () {
        var url = "http://23.111.138.246/Admin/Events/index/" + $(this).data("userid") + "|user";
        location.href = url;
    });


}

function BindLookUp() {
    //Get Artist Lookup

    $("[data-element=ArtistName]").autocomplete({
        source: function (request, response) {
            $.ajax({
                url: "http://23.111.138.246/AdminAPI/GetSearchByArtistName/",
                data: { query: request.term },
                dataType: 'json',
                type: 'GET',
                success: function (data) {
                    if (data.length == 0) {
                        $("[data-element=artistid]").val(0);
                    }
                    response($.map(data, function (item) {
                        return {
                            label: item.ArtistName,
                            value: item.ArtistID
                        }
                    }));
                }
            })
        },
        select: function (event, ui) {
            $("[data-element=ArtistName]").val(ui.item.label);
            $("[data-element=artistid]").val(ui.item.value);
            return false;
        },
        minLength: 1,
        delay: 300,
        focus: function (event, ui) { },
        change: function (event, ui) {
            if (ui.item == null) {
                // alert("manual");
                $("[data-element=artistid]").val(0);
                $("[data-element=ArtistName]").val('');
            }
            //else
            //alert("selected");
        }
    });

    //Get Venue lookup
    $("[data-element=VenueName]").autocomplete({
        source: function (request, response) {
            $.ajax({
                url: "http://23.111.138.246/AdminAPI/GetSearchByVenueName/",
                data: { query: request.term },
                dataType: 'json',
                type: 'GET',
                success: function (data) {
                    if (data.length == 0) {
                        $("[data-element=venueid]").val(0);
                    }
                    response($.map(data, function (item) {
                        return {
                            label: item.VenueName,
                            value: item.VenueID + '|' + item.SeatGeek_VenuID
                                                + '|' + item.Extended_Address
                                                + '|' + item.Extended_Address
                                                + '|' + item.VenueCountry
                                                + '|' + item.VenueCity
                                                + '|' + item.VenueState
                                                + '|' + item.Postal_Code
                                                + '|' + item.VenueLat
                                                + '|' + item.VenueLong
                                                + '|' + item.Timezone
                                                + '|' + item.ImageURL
                        }
                    }));
                }
            })
        },
        select: function (event, ui) {
            $("[data-element=VenueName]").val(ui.item.label);
            var str = ui.item.value.split('|');

            $("[data-element=venueid]").val(str[0]);
            $("[data-element=SeatGeek_VenuID]").val(str[1]);
            $("[data-element=Extended_Address]").val(str[2]);
            $("[data-element=Address]").val(str[3]);
            $("[data-element=VenueCountry]").val(str[4]);
            $("[data-element=VenueCity]").val(str[5]);
            $("[data-element=VenueState]").val(str[6]);
            $("[data-element=Postal_Code]").val(str[7]);
            $("[data-element=VenueLat]").val(str[8]);
            $("[data-element=VenueLong]").val(str[9]);
            $("[data-element=Timezone]").val(str[10]);

            html = "";
            html += '<div class="sliderimg" style="position:relative; float:left; margin-right:15px; margin-top:0px;">'
                             + '<img src=' + str[11] + ' style="width:124px; height:124px;"/>'
                              + '<img src="http://23.111.138.246/Areas/Admin/Content/img/cross.png" data-VenueID="' + str[0] + '" class="deletephoto" style="position:absolute; top:-5px; right:-5px; width:24px; display:none; cursor:pointer;" />'
                            + '</div>';


            $("#imgProfile").html(html);

            return false;
        },

        minLength: 1,
        change: function (event, ui) {

            if (ui.item == null) {
                // alert("manual");
                $("[data-element=venueid]").val(0);
                //$("[data-element=VenueName]").val('');
                $("[data-element=SeatGeek_VenuID]").val('');
                $("[data-element=Extended_Address]").val('');
                $("[data-element=Address]").val('');
                $("[data-element=VenueCountry]").val('');
                $("[data-element=VenueCity]").val('');
                $("[data-element=VenueState]").val('');
                $("[data-element=Postal_Code]").val('');
                $("[data-element=VenueLat]").val('');
                $("[data-element=VenueLong]").val('');
                $("[data-element=Timezone]").val('');
                $("#imgProfile").html('');
            }
        }
    });


    //Get Timezone Lookup

    $("[data-element=Timezone]").autocomplete({
        source: function (request, response) {
            $.ajax({
                url: "http://23.111.138.246/AdminAPI/GetSearchByTimezone/",
                data: { query: request.term },
                dataType: 'json',
                type: 'GET',
                success: function (data) {
                    if (data.length == 0) {
                        //$("[data-element=artistid]").val(0);
                    }
                    response($.map(data, function (item) {
                        return {
                            label: item.TZ,
                            value: item.TimeZoneID
                        }
                    }));
                }
            })
        },
        select: function (event, ui) {
            $("[data-element=Timezone]").val(ui.item.label);
            return false;
        },
        minLength: 1,
        delay: 300,
        focus: function (event, ui) { },
        change: function (event, ui) {
            if (ui.item == null) {
                // alert("manual");
                $("[data-element=Timezone]").val('');
            }
            //else
            //alert("selected");
        }
    });

}

function InitDeletePhoto() {
    $(".deletephoto").click(function () {
        new Common().AjaxCall("AdminAPI/deleteVenuephoto/" + $(this).data("venueid"), {}, "DELETE", true, function (d) { });
        var c = new Common();
        c.ShowMessage("Photo Removed successfully", "success");
        $("#imgProfile").html("");
    });
}


function ClearForm() {
    //New
    $("[data-TourDateID=TourDateID]").val(0);
    $("[data-element=TourDateID]").val(0);
    $("[data-element=artistid]").val(0);
    $("[data-element=venueid]").val(0);
    $("#hfDeleted").val('');
    $("[data-element=HashTag]").val('');
    $("[data-element=EventName]").val('');
    $("[data-element=SeatGeek_TourID]").val('');
    $("[data-element=ArtistName]").val('');
    $("[data-element=Datetime_Local]").val('');
    $("[data-element=VenueName]").val('');
    $("[data-element=SeatGeek_VenuID]").val('');
    $("[data-element=Extended_Address]").val('');
    $("[data-element=Address]").val('');
    $("[data-element=VenueCountry]").val('');
    $("[data-element=VenueCity]").val('');
    $("[data-element=VenueState]").val('');
    $("[data-element=Postal_Code]").val('');
    $("[data-element=VenueLat]").val('');
    $("[data-element=VenueLong]").val('');
    $("[data-element=Timezone]").val('');
    $("#imgProfile").html('');
    $("#input-image").val("");

}

function DeleteEvent(eventId) {


    new Common().AjaxDelete("AdminAPI/DeleteEvent/" + eventId + "/", {}, "DELETE", true, function (d) {
        if (d) {
            $("#btnDeleteEvent").attr('style', 'background-color:blue; border-color:blue;');
            $("#btnDeleteEvent").attr('value', 'Undo Delete');
            c.ShowMessage("Event Deleted", "success");
        }
        else {
            $("#btnDeleteEvent").attr('style', 'background-color:Red; border-color:Red;');
            $("#btnDeleteEvent").attr('value', 'Delete Event');
            c.ShowMessage("Event Active", "success");
        }
    });
    var c = new Common();

}

function UpdateEvent(btn) {
    var c = new Common();
    if (c.validate("form-Event")) {
        if ($("[data-element=InAppTicketing]").is(':checked')) {
            if ($("[data-element=TotalSeats]").val() == null || $("[data-element=TotalSeats]").val() == 0 || $("[data-element=TotalSeats]").val() == "" || $("[data-element=TotalSeats]").val() == undefined) {
                c.ShowMessage("Please enter total seat(s).", "warning");
                return false;
            }
        }
        var data = new FormData();
        data = c.getFormObj("form-Event");
        //debugger;
        data.append("userfile", $('#input-image').get(0).files[0]);

        c.AjaxCallFormData("AdminAPI/updateEvent", data, true, function (d) {
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