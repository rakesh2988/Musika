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
    $('#myModalGenre').on('hidden.bs.modal', function () {
        ClearFormGenre();
    })

    $("#btnSearch").click(function () {
        FilterRecords(1);
    });
    $("#btnSaveChanges").click(function () {
        UpdateArtist(this);
    });

    $("#btnSaveChangesGenre").click(function () {
        UpdateArtistGenre(this);
    });
    
    $(".AddGenre").click(function () {
        $("#myModalGenre").modal("show");

        $("[data-ArtistGenreID=ArtistGenreID]").val(0);
        $("[data-element=ArtistGenreID]").val(0);
        $("[data-element=GenreID]").val(0);

        $("[data-element=ArtistID]").val($("[data-element=ArtistID]").val());

        BindLookup();

    });

    $(".Add").click(function () {
        $("#myModal").modal("show");
        var c = new Common();
        c.ClearFormValues("#form-Artist");

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
    c.AjaxCall("AdminAPI/GetArtists", $.param(data), "GET", true, function (d) {
        CreateTable(d);
        Paginate(d);
    });
}

function CreateTable(data) {
    var html = "";

    for (var i = 0; i < data.Items.length; i++) {

        var image = data.Items[i].ThumbnailURL;
        if (!image) { image = data.Items[i].ImageURL }

        html += "<tr id=tr" + data.Items[i].ArtistID + ">"
        + "<td>" + '<img src=' + image + ' style="width:64px; height:64px"/>' + "</td>"
        + "<td>" + data.Items[i].ArtistName + "</td>"
        + "<td>" + data.Items[i].Main_Genre + "</td>"
        + "<td>" + data.Items[i].OnTour + "</td>"
        + "<td>" + data.Items[i].TrackCount + "</td>"

        + "<td>" + data.Items[i].EventCount + "</td>"

        if (data.Items[i].Isdefault == true) {
            html += "<td class='text-center'><div class='checkbox' style='padding-left:0px !important; margin-left:-10px !important'><input type='checkbox' id='opt" + data.Items[i].ArtistID + "' value='1' data-artistid='" + data.Items[i].ArtistID + "' class='checkUse' checked></div>"
        } else {
            html += "<td class='text-center'><div class='checkbox' style='padding-left:0px !important; margin-left:-10px !important'><input type='checkbox' id='opt" + data.Items[i].ArtistID + "' value='1' data-artistid='" + data.Items[i].ArtistID + "' class='checkUse'></div>"
        }

        html += "<td><input type='button' class='btn btn-info btn-sm view' value='View/Edit' data-artistid='" + data.Items[i].ArtistID + "' />"
        + "<input type='button' class='btn btn-info btn-sm GenreView' value='Genre' data-artistname='" + data.Items[i].ArtistName + "' data-artistid='" + data.Items[i].ArtistID + "' style='margin-left:5px;'/>"
        + "<input type='button' class='btn btn-info btn-sm Events' value='Events' data-artistname='" + data.Items[i].ArtistName + "' data-artistid='" + data.Items[i].ArtistID + "' style='margin-left:5px;'/>"
        + "<input type='button' class='btn btn-info btn-sm DeleteArtist' value='Delete' data-artistname='" + data.Items[i].ArtistName + "' data-artistid='" + data.Items[i].ArtistID + "' style='margin-top: 5px;'/>"
           
      
        + "</td>"
        + "</tr>";
    }
    $("#tbodyArtists").html(html);
    BindEvents();
    initUseDefaultArtist();
}

function initUseDefaultArtist() {
    $(".checkUse").click(function () {
        var id = $(this).data("artistid");
        var data = {
            ArtistID: id
        };
        var c = new Common();


        c.AjaxCall("AdminAPI/UpdateDefaultArtsit", JSON.stringify(data), "POST", true, function (d) {

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

        cmn.AjaxCall("AdminAPI/GetArtistByID?ID=" + $(this).data("artistid"), {}, "GET", true, function (d) {
            console.log(d);
            $("#myModal").modal("show");
            $("[data-element=ArtistName]").val(d.ArtistName);

            var image = d.ThumbnailURL;
            if (!image) { image = d.ImageURL; }


            html = "";
            html += '<div class="sliderimg" style="position:relative; float:left; margin-right:15px; margin-top:0px;">'
                             + '<img src="' + image + '" style="width:124px; height:124px;"/>'
                              + '<img src="23.111.138.246/Areas/Admin/Content/img/cross.png" data-ArtistID="' + d.ArtistID + '" class="deletephoto" style="position:absolute; top:-5px; right:-5px; width:24px; display:none; cursor:pointer;" />'
                            + '</div>';


            $("#imgProfile").html(html);
            $("[data-element=Musicgraph_ID]").val(d.Musicgraph_ID);
            $("[data-element=OnTour]").prop('checked', d.OnTour);

            if (d.OnTour) {
                $($("[data-element=OnTour]").parent()).addClass('checked');
            }

            $("[data-element=About]").val(d.About);
            $("[data-element=AboutES]").val(d.AboutES);

            $("[data-element=Spotify_ID]").val(d.Spotify_ID);
            $("[data-element=Instagram_ID]").val(d.Instagram_ID);
            $("[data-element=Instagram_Tag]").val(d.Instagram_Tag);


            //$("[data-element=OnTour]").is(":checked")

            $("[data-ArtistID=ArtistID]").val(d.ArtistID);
            $("[data-element=ArtistID]").val(d.ArtistID);

            

            $(".sliderimg").hover(function () {
                $(this).children().eq(1).show()
              
            }, function () {
                $(this).children().eq(1).hide();
            });

          

            InitDeletePhoto();
        }, this);

    });

    $(".DeleteArtist").click(function () {
        var _artistid = $(this).data("artistid");
        cmn.ConfirmAjaxCall("AdminAPI/DeleteArtistByID?ID=" + $(this).data("artistid"), {}, "GET", true, function (d) {
            if (d.Status == true) {
                cmn.ShowMessage(d.RetMessage, "success");
                //$(this).closest('tr').remove();
                $('#tr' + _artistid).remove();
                //FilterRecords(pageNumber);
            } 
            //else {
            //    cmn.ShowMessage(d.RetMessage, "error");
            //}
        }, this);

    });
  

    $(".GenreView").click(function () {

        $("#myModalArtistGenre").modal("show");
        $("[data-ArtistID=ArtistID]").val($(this).data("artistid"));
        $("[data-element=ArtistID]").val($(this).data("artistid"));

        FilterRecordsGenre(1);

    });


    $(".Genre").click(function () {
        var url = "23.111.138.246/Admin/ArtistGenre/index/" + $(this).data("artistid") + "/" + $(this).data("artistname") + "/";
        location.href = url;
    });

    $(".Events").click(function () {
        var url = "23.111.138.246/Admin/Events/index/" + $(this).data("artistid") + "/-/" + $(this).data("artistname") + "/";
        location.href = url;
    });

}

function InitDeletePhoto() {
    $(".deletephoto").click(function () {
        new Common().AjaxCall("AdminAPI/deleteArtistphoto/" + $(this).data("artistid"), {}, "DELETE", true, function (d) { });
        var c = new Common();
        c.ShowMessage("Photo Removed successfully", "success");
        $("#imgProfile").html("");
    });
}

function ClearForm() {
    $($("[data-element=OnTour]").parent()).removeClass('checked');
    $("[data-element=ArtistName]").val("");
    $("#imgProfile").html("");
    $("[data-element=Musicgraph_ID]").val("");
    $("[data-element=About]").val("");
    $("[data-element=Spotify_ID]").val("");
    $("[data-element=Instagram_ID]").val("");
    $("[data-element=Instagram_Tag]").val("");


}

function UpdateArtist(btn) {
    var c = new Common();
    if (c.validate("form-Artist")) {
        var data = c.getValues("#form-Artist");
        data["OnTour"] = $("[data-element=OnTour]").is(":checked");

          c.AjaxCall("AdminAPI/updateArtist", JSON.stringify(data), "POST", true, function (d) {
              if (d.Status) {

                  var data1 = new FormData();
                  data1 = c.getFormObj("form-Artist");
                  data1.append("artistfile", $('#input-image').get(0).files[0]);
                  data1.append("artistid", d.ID);

                  c.AjaxCallFormData("AdminAPI/updateArtistPic", data1, true, function (d) {
                      if (d.Status) {
                          $('#myModal').modal("hide");
                         
                      } else {
                          $('#myModal').modal("hide");
                          c.ShowMessage(d.RetMessage, "success");
                          FilterRecords(pageNumber);
                      }
                  }, btn);

                  c.ShowMessage(d.RetMessage, "success");
                  FilterRecords(pageNumber);
              
            } else {
                c.ShowMessage(d.RetMessage, "error");
            }
          }, btn);


    }
}



//Artist Genre 

function BindLookup() {

    //Genre Lookup
    $("[data-element=Name]").autocomplete({
        source: function (request, response) {
            $.ajax({
                url: "23.111.138.246/AdminAPI/GetSearchByGenreName/",
                data: { query: request.term },
                dataType: 'json',
                type: 'GET',
                success: function (data) {
                    if (data.length == 0) {
                        $("[data-element=GenreID]").val(0);
                    }
                    response($.map(data, function (item) {
                        return {
                            label: item.Name,
                            value: item.GenreID
                        };
                    }));
                }
            });
        },
        select: function (event, ui) {
            $("[data-element=Name]").val(ui.item.label);
            $("[data-element=GenreID]").val(ui.item.value);
            return false;
        },

        minLength: 1,
        change: function (event, ui) {

            if (ui.item == null) {
                // alert("manual");
                $("[data-element=GenreID]").val(0);
                $("[data-element=Name]").val('');
            }
        }
    });


}

function FilterRecordsGenre(page) {
    pageNumber = page;
    var c = new Common();
    var data = c.getValues("#form-search-ArtistsGenre", "");
    data["Pageindex2"] = page;
    data["Pagesize2"] = 30;

    data["sortColumn2"] = $("#hfSortColumn2").val();
    data["sortOrder2"] = $("#hfSortOrder2").val();
    data["ArtistID"] = $("[data-element=ArtistID]").val();

    $("#tbodyArtistsGenre").html("");
    c.AjaxCall("AdminAPI/GetArtistGenreByID", $.param(data), "GET", true, function (d) {
        CreateTableGenre(d);
        PaginateGenre(d);
    });
}

function PaginateGenre(data) {
    var pagination = "";
    var counter = 0;
    if (data.PageNumber > 1) {
        pagination += '<li><a href="javascript:FilterRecordsGenre(' + (data.PageNumber - 1) + ')" aria-label="Previous"><span aria-hidden="true">&laquo;</span></a></li>';
    }
    else {
        pagination += '<li class="disabled"><a href="javascript:void(0);" aria-label="Previous"><span aria-hidden="true">&laquo;</span></a></li>';
    }

    var start = 0;
    if (data.PageCount > 5) {
        if (data.PageNumber > 2) {
            pagination += '<li><a href="javascript:FilterRecordsGenre(' + (data.PageNumber - 2) + ')">' + (data.PageNumber - 2) + '</a></li>';
            pagination += '<li><a href="javascript:FilterRecordsGenre(' + (data.PageNumber - 1) + ')">' + (data.PageNumber - 1) + '</a></li>';
        }
        else if (data.PageNumber == 2) {
            pagination += '<li><a href="javascript:FilterRecordsGenre(' + (data.PageNumber - 1) + ')">' + (data.PageNumber - 1) + '</a></li>';
        }
        for (var i = data.PageNumber; i < data.PageCount; i++) {
            counter++;
            pagination += '<li class=' + (data.PageNumber == i ? "active" : "") + '><a href="javascript:FilterRecordsGenre(' + i + ')">' + i + '</a></li>';
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
            pagination += '<li class=' + (data.PageNumber == i ? "active" : "") + '><a href="javascript:FilterRecordsGenre(' + i + ')">' + i + '</a></li>';
        }
    }

    if (data.PageNumber < data.PageCount) {
        pagination += '<li><a href="javascript:FilterRecordsGenre(' + (data.PageNumber + 1) + ')" aria-label="Next"><span aria-hidden="true">&raquo;</span></a></li>';
    }
    else {
        pagination += '<li class="disabled"><a href="javascript:void(0);" aria-label="Next"><span aria-hidden="true">&raquo;</span></a></li>';
    }

    $("#paginationGenre").html(pagination);
    $("#pageGenre").text("Page " + data.PageNumber + " of " + data.PageCount);
}

function CreateTableGenre(data) {
    var html = "";

    for (var i = 0; i < data.Items.length; i++) {
        html += "<tr>"

        + "<td>" + data.Items[i].Genre_Name + "</td>"
        + "<td>" + data.Items[i].Primary + "</td>"

        + "<td><input type='button' class='btn btn-info btn-sm EditGenre' value='View/Edit' data-artistgenreid='" + data.Items[i].ArtistGenreID + "' />"
        + "<input type='button' class='btn btn-info btn-sm DeleteGenre' value='Delete' data-artistgenreid='" + data.Items[i].ArtistGenreID + "' style='margin-left:5px;'/>"
        + "</td>"
        + "</tr>";
    }
    $("#tbodyArtistsGenre").html(html);
    BindEventsGenre();
}

function SortDataGenre(obj) {
    var span = $(obj).find("span");
    $("#hfSortColumn2").val(span.attr('data-col'));
    if (span.hasClass('sortup')) {
        $("#hfSortOrder2").val("DESC");
        $(".sortup").attr("class", "sort");
        $(".sortdown").attr("class", "sort");
        span.attr("class", "sortdown");
    }
    else {
        $("#hfSortOrder2").val("ASC");
        $(".sortup").attr("class", "sort");
        $(".sortdown").attr("class", "sort");
        span.attr("class", "sortup");
    }
    FilterRecordsGenre(1);
}

function BindEventsGenre() {
    var cmn = new Common();
    $(".EditGenre").click(function () {

        cmn.AjaxCall("AdminAPI/GetArtistGenreByID?ID=" + $(this).data("artistgenreid"), {}, "GET", true, function (d) {
            console.log(d);
            $("#myModalGenre").modal("show");

            $("[data-element=Name]").val(d.Name);
            $("[data-element=Primary]").val(d.Primary == true ? '1' : '0');

            $("[data-ArtistGenreID=ArtistGenreID]").val(d.ArtistGenreID);
            $("[data-element=ArtistGenreID]").val(d.ArtistGenreID);

            $("[data-element=GenreID]").val(d.GenreID);
            $("[data-element=ArtistID]").val($("[data-element=ArtistID]").val())

            BindLookup();


        }, this);

    });

    $(".DeleteGenre").click(function () {
        DeleteArtistGenre(this,$(this).data("artistgenreid"));
    });

}

function UpdateArtistGenre(btn) {
    var c = new Common();
    if (c.validate("form-Genre")) {
        var data = c.getValues("#form-Genre");

        c.AjaxCall("AdminAPI/updateArtistGenre", JSON.stringify(data), "POST", true, function (d) {
            if (d.Status) {
                $('#myModalGenre').modal("hide");
                c.ShowMessage(d.RetMessage, "success");
                FilterRecordsGenre(pageNumber);
            } else {
                c.ShowMessage(d.RetMessage, "error");
            }
        }, btn);
    }
}

function ClearFormGenre() {

    $("[data-ArtistGenreID=ArtistGenreID]").val(0);
    $("[data-element=ArtistGenreID]").val(0);
    $("[data-element=GenreID]").val(0);
    $("[data-element=Name]").val('');
}

function DeleteArtistGenre(btn,ID) {
    var c = new Common();
    c.AjaxCall("AdminAPI/DeleteArtistGenre/" + ID, {}, "DELETE", true, function (d) {
        if (d.Status) {
            c.ShowMessage(d.RetMessage, "success");
            FilterRecordsGenre(pageNumber);
        } else {
            c.ShowMessage(d.RetMessage, "error");
        }
    }, btn);

}