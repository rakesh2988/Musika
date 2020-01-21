var pageNumber = 1;


$(document).ready(function () {
    $('#myModal').on('hidden.bs.modal', function () {
        ClearForm();
    })
    $("#btnSearch").click(function () {
        FilterRecords(1);
    });
    $("#btnSaveChanges").click(function () {
        UpdateGenre(this);
    });

    $(".Add").click(function () {
        $("#myModal").modal("show");
        $("[data-element=AID]").val(0);
        $("[data-element=Name]").val('');

       // BindAutoCompleteLookup();
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
    var data = c.getValues("#form-search-NotLatin", "");
    data["Pageindex"] = page;
    data["Pagesize"] = 30;

    data["sortColumn"] = $("#hfSortColumn").val();
    data["sortOrder"] = $("#hfSortOrder").val();

    $("#tbodyArtists").html("");
    c.AjaxCall("AdminAPI/GetNotLatin", $.param(data), "GET", true, function (d) {
        CreateTable(d);
        Paginate(d);
    });
}


function CreateTable(data) {
    var html = "";

    for (var i = 0; i < data.Items.length; i++) {
        html += "<tr>"
        + "<td>" + data.Items[i].Name + "</td>"

        + "<td><input type='button' class='btn btn-info btn-sm view' value='View/Edit' data-aid='" + data.Items[i].AID + "' />"
        + "<input type='button' class='btn btn-info btn-sm Delete' value='Delete' data-aid='" + data.Items[i].AID + "' style='margin-left:5px;' />"
        + "</td>"
        + "</tr>";
    }
    $("#tbodyGenre").html(html);
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

        cmn.AjaxCall("AdminAPI/GetNotLatinByID?ID=" + $(this).data("aid"), {}, "GET", true, function (d) {
            console.log(d);
            $("#myModal").modal("show");


            $("[data-element=Name]").val(d.Name);
            $("[data-element=AID]").val(d.AID);

           // BindAutoCompleteLookup();
          
            
        }, this);

    });


    $(".Delete").click(function () {
        DeleteGenre(this, $(this).data("aid"));
    });

}

function DeleteGenre(btn, ID) {
    var c = new Common();
    c.AjaxCall("AdminAPI/DeleteNotLatin/" + ID, {}, "DELETE", true, function (d) {
        if (d.Status) {
            c.ShowMessage(d.RetMessage, "success");
            FilterRecords(pageNumber);
        } else {
            c.ShowMessage(d.RetMessage, "error");
        }
    }, btn);

}

function ClearForm() {
    //New
    $("[data-element=AID]").val(0);
    $("[data-element=Name]").val('');
}


function UpdateGenre(btn) {
    var c = new Common();
    if (c.validate("form-NotLatin")) {
        var data = c.getValues("#form-NotLatin");
        c.AjaxCall("AdminAPI/updateNotLatin", JSON.stringify(data), "POST", true, function (d) {
            if (d.Status) {
                $('#myModal').modal("hide");
                c.ShowMessage(d.RetMessage, "success");
                FilterRecords(pageNumber);
            } else {
                c.ShowMessage(d.RetMessage, "error");
            }
        }, btn);
    }
}