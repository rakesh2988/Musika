/// <reference path="common.js" />
var pageNumber = 1;
$(document).ready(function () {
    $('#myModal').on('hidden.bs.modal', function () {
        ClearForm();
    })
    $("#btnSearch").click(function () {
        FilterRecords(1);
    });
    $("#btnSaveChanges").click(function () {
        UpdateFrat(this);
    });
    FilterRecords(pageNumber);
});

function keypressHandler(e) {
    if (e.which == 13) {
        //$(this).blur();
        $('#btnSearch').click();//give your submit an ID
        $(e).focus();
    }
}
function FilterRecords(page) {
    pageNumber = page;
    var c = new Common();
    var data = c.getValues("#form-search-frats", "");
    data["page"] = page;
    data["pageSize"] = 30;
    c.AjaxCall("api/frats/getall", $.param(data), "GET", true, function (d) {
        CreateTable(d);
        Paginate(d);
    });
}
function CreateTable(data) {
    var html = "";
    for (var i = 0; i < data.Items.length; i++) {
        html += "<tr>";
        html += "<td>" + data.Items[i].FirstName + " " + data.Items[i].LastName + "</td>";
        html += "<td>" + data.Items[i].Email + "</td>";
        html += "<td>" + data.Items[i].FratName + "</td>";
        html += "<td>" + data.Items[i].School + "</td>";
        html += "<td class='" + (data.Items[i].Status == "Active" ? "text-success" : (data.Items[i].Status == "InActive" || data.Items[i].Status == "Denied") ? "text-danger" : "text-info") + "'>" + data.Items[i].Status + "</td>";
        html += "<td><input type='button' class='btn btn-info btn-sm view' value='View/Edit' data-username='" + data.Items[i].FirstName + " " + data.Items[i].LastName + "' data-email='" + data.Items[i].Email + "' data-frat-id='" + data.Items[i].FratId + "' data-status='" + data.Items[i].Status + "' />";
        html += "<input type='button' class='btn btn-danger btn-sm delete' value='Delete' data-frat-id='" + data.Items[i].FratId + "' style='margin-left:5px;' /></td>"
        html += "<td></td>";
        html += "</tr>";
    }
    $("#tbodyFrats").html(html);
    BindEvents();
}
function Paginate(data) {
    var pagination = "";
    var counter = 0;
    if (data.HasPreviousPage) {
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
        for (var i = data.PageNumber ; i <= data.PageCount; i++) {
            pagination += '<li class=' + (data.PageNumber == i ? "active" : "") + '><a href="javascript:FilterRecords(' + i + ')">' + i + '</a></li>';
        }
    }

    if (data.HasNextPage) {
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
        var username = $(this).data("username");
        var email = $(this).data("email");
        var status = $(this).data("status");
        var html = "";
        if (status == "Active" || status == "InActive") {
            html += '<option value="">Select Status</option>'
                 + '<option>Active</option>'
                 + '<option>InActive</option>';
            $("[data-element=FratStatus]").html(html);
        }
        else {
            html += '<option value="">Select Status</option>'
                 + '<option>Active</option>'
                 + '<option>InActive</option>'
                 + '<option>CodeSent</option>'
                 + '<option>Denied</option>';
            $("[data-element=FratStatus]").html(html);
        }
        cmn.AjaxCall("api/frats/getfratbyid?id=" + $(this).data("frat-id"), {}, "GET", true, function (d) {
            $("#myModal").modal("show");
            $("#txtUsername").val(username);
            $("#txtEmail").val(email);
            $("[data-element=FratName]").val(d.FratName);
            $("[data-element=FratStatus]").val(d.Status);
            $("[data-element=FratState]").val(d.State);
            $("[data-element=FratSchool]").val(d.SchoolId);
            $("[data-element=FratId]").val(d.FratId);
            $("#imgProfile").html("<img src='" + d.ProfileThumbnailURL + "' />");
            var html = "";
            for (var i = 0; i < d.PhotoGallery.length; i++) {
                html += '<div class="sliderimg" style="position:relative; float:left; margin-right:15px; margin-top:15px;">'
                             + '<img src=' + d.PhotoGallery[i].ThumbnailURL + ' />'
                              + '<img src="http://localhost:60737/Areas/Admin/Content/img/cross.png" data-photoid="' + d.PhotoGallery[i].PhotoId + '" class="deletephoto" style="position:absolute; top:-5px; right:-5px; width:24px; display:none; cursor:pointer;" />'
                            + '</div>';
            }
            $("#slider").html(html);

            $(".sliderimg").hover(function () {
                var index = $(".sliderimg").index();
                $(this).children().eq(1).show().click(function () {
                    d.PhotoGallery.splice(index, 1);
                    $(this).parent().remove();
                });
            }, function () {
                $(this).children().eq(1).hide();
            });
            InitDeletePhoto();
        }, this);
    });
    $(".delete").click(function () {
        cmn.Delete("api/frats/delete/" + $(this).data("frat-id"), {}, "DELETE", true, function (d) {
            if (d) {
                FilterRecords(pageNumber);
            }
        }, this);
    });
}
function InitDeletePhoto() {
    $(".deletephoto").click(function () {
        new Common().AjaxCall("api/frats/deletephoto/" + $(this).data("photoid"), {}, "DELETE", true, function (d) { });
    });
}
function ClearForm() {
    $("#txtEmail").val("");
    $("#txtUsername").val("");
    $("[data-element=FratName]").val("");
    $("[data-element=FratStatus]").val("");
    $("[data-element=FratState]").val("");
    $("[data-element=FratSchool]").val("");
    $("[data-element=FratId]").val("0");
    $("#imgProfile").html("");
}
function UpdateFrat(btn) {
    var c = new Common();
    if (c.validate("form-frat")) {
        var data = c.getValues("#form-frat");
        data["Email"] = $("#txtEmail").val();
        c.AjaxCall("api/frats/updatefrat", JSON.stringify(data), "POST", true, function (d) {
            if (d) {
                $('#myModal').modal("hide");
                c.ShowMessage("Frat updated successfully.", "success");
                FilterRecords(pageNumber);
            }
        }, btn);
    }
}
