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
    });
    $("#btnSearch").click(function () {
        FilterRecords(1);
    });
    $("#btnSaveChanges").click(function () {
        UpdateEvent(this);
    });

    $(".Add").click(function () {
        $("#myModal").modal("show");
        $("[data-TourDateID=TourDateID]").val(0);
        $("[data-element=TourDateID]").val(0);
        $("[data-element=artistid]").val(0);
        $("[data-element=venueid]").val(0);

        BindLookup();
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


function FilterRecords(page) {
    pageNumber = page;
    var c = new Common();
    var data = c.getValues("#form-search-Venue", "");
    data["Pageindex"] = page;
    data["Pagesize"] = 30;

    data["sortColumn"] = $("#hfSortColumn").val();
    data["sortOrder"] = $("#hfSortOrder").val();

    $("#tbodyArtists").html("");
    c.AjaxCall("AdminAPI/GetVenues", $.param(data), "GET", true, function (d) {
        CreateTable(d);
        Paginate(d);
    });
}


function CreateTable(data) {
    var html = "";

    for (var i = 0; i < data.Items.length; i++) {
        html += "<tr>"
        + "<td>" + data.Items[i].VenueName + "</td>"
        + "<td>" + data.Items[i].VenueCountry + "</td>"

        + "<td>" + data.Items[i].VenueCity + "</td>"
        + "<td>" + data.Items[i].Postal_Code + "</td>"

        + "<td>" + data.Items[i].EventCount + "</td>"

        + "<td><input type='button' class='btn btn-info btn-sm view' value='View/Edit' data-venueid='" + data.Items[i].VenueID + "' />"
        + "<input type='button' class='btn btn-info btn-sm Events' value='Events' data-venuename='" + data.Items[i].VenueName + "' data-venueid='" + data.Items[i].VenueID + "' style='margin-left:5px;' />"
        + "</td>"
        + "</tr>";
    }
    $("#tbodyEvents").html(html);
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
    $(".view").click(function () {

        cmn.AjaxCall("AdminAPI/GetVenueByID?ID=" + $(this).data("venueid"), {}, "GET", true, function (d) {
            console.log(d);
            $("#myModal").modal("show");


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


            $("[data-venueid=VenueID]").val(d.VenueID);
            $("[data-element=VenueID]").val(d.VenueID);

            html = "";
            html += '<div class="sliderimg" style="position:relative; float:left; margin-right:15px; margin-top:0px;">'
                             + '<img src=' + d.ImageURL + ' style="width:124px; height:124px;"/>'
                              + '<img src="23.111.138.246/Areas/Admin/Content/img/cross.png" data-venueid="' + d.VenueID + '" class="deletephoto" style="position:absolute; top:-5px; right:-5px; width:24px; display:none; cursor:pointer;" />'
                            + '</div>';


            $("#imgProfile").html(html);


            BindLookup();
          


            $(".sliderimg").hover(function () {
                $(this).children().eq(1).show()

            }, function () {
                $(this).children().eq(1).hide();
            });



            InitDeletePhoto();
        }, this);

    });


    $(".Events").click(function () {
        var url = "23.111.138.246/Admin/Events/index/" + $(this).data("venueid") + "/" + $(this).data("venuename") + "/";
        location.href = url;
    });

}

function BindLookup()
{
    //Get Timezone Lookup

    $("[data-element=Timezone]").autocomplete({
        source: function (request, response) {
            $.ajax({
                url: "23.111.138.246/AdminAPI/GetSearchByTimezone/",
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
    $("[data-element=venueid]").val(0);

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


function UpdateEvent(btn) {
    var c = new Common();
    if (c.validate("form-Venue")) {
        var data = new FormData();
        data = c.getFormObj("form-Venue");
        data.append("userfile", $('#input-image').get(0).files[0]);

        c.AjaxCallFormData("AdminAPI/updateVenue", data, true, function (d) {
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