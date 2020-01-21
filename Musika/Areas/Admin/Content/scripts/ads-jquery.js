$(document).ready(function () {
    LoadAdsRecords();
    $("#btnSave").click(function () {
        AddNewAds(this);
    });
    BindLookUp();
});

function LoadAdsRecords() {
    var c = new Common();
    var data = c.getValues("#form-add-ads12", "");

    $("#tbodyAds").html("");
    c.AjaxCall("AdminAPI/GetAds", $.param(data), "GET", true, function (d) {
        CreateTable(d);
    });
}
function BindLookUp() {
    //Get Cities Lookup    
    $("[data-element=CityName]").autocomplete({
        source: function (request, response) {
            $.ajax({
                url: "23.111.138.246/AdminAPI/GetCities/",
                data: { query: request.term },
                dataType: 'json',
                type: 'GET',
                success: function (data) {
                    if (data.length == 0) {
                        $("[data-element=artistid]").val(0);
                    }
                    response($.map(data, function (item) {
                        return {
                            label: item.City,
                            value: item.CityID
                        }
                    }));
                }
            })
        },
        select: function (event, ui) {
            $("[data-element=CityName]").val(ui.item.label);
            $("[data-element=CityID]").val(ui.item.value);
            return false;
        },
        minLength: 2,
        delay: 300,
        focus: function (event, ui) { },
        change: function (event, ui) {
            if (ui.item == null) {
                // alert("manual");              
                $("[data-element=CityName]").val('');
                $("[data-element=CityID]").val(0);
            }
            //else
            //alert("selected");
        }
    });
}

function LoadddlCities() {
    //var c = new Common();
    //var data = c.getValues("#form-add-ads12", "");

    //c.AjaxCall("AdminAPI/GetCities", $.param(data), "GET", true, function (d) {
    //   var data = d;
    //  var options = "<option value=''>Select City</option>";
    //for (var i = 0; i < data.Items.length; i++) {
    //    options += "<option value='" + data.Items[i].City + "'>" + data.Items[i].City + "</option>";
    //}
    //$("#ddlCities").html(options);
    //});
}

function CreateTable(data) {
    var html = "";

    for (var i = 0; i < data.Items.length; i++) {
        html += "<tr id=tr" + data.Items[i].AdId + ">"
        + "<td>" + "<img class='openbannerimge' src='" + data.Items[i].ImageURL + "' alt='' style='height:50px; width:50px;cursor:pointer;' />" + "</td>"
        + "<td><a href='" + data.Items[i].LinkURL + "' target='_blank'>" + data.Items[i].LinkURL + "</a></td>"
        + "<td>" + data.Items[i].City + "</td>"
        + "<td>" + data.Items[i].Recordstatus + "</td>"
        + "<td>"
        + "<input type='button' class='btn btn-info btn-sm Delete' value='Delete'   data-adid='" + data.Items[i].AdId + "' style='margin-top:5px;' />"
        + "<input type='button' class='btn btn-info btn-sm edit' value='Edit'   data-city='" + data.Items[i].City + "' data-adid='" + data.Items[i].AdId + "' data-linkurl='" + data.Items[i].LinkURL + "' data-recordstatus='" + data.Items[i].Recordstatus + "' style='margin-top:5px;margin-left:5px' />"
        + "</td>"
        + "</tr>";
    }
    $("#tbodyAds").html(html);
    BindEvents();
}
function BindEvents() {
    var cmn = new Common();
    $(".Delete").click(function () {
        var _Adid = $(this).data("adid");
        cmn.ConfirmAjaxCall("AdminAPI/DeleteAds/" + $(this).data("adid"), {}, "DELETE", true, function (d) {
            if (d) {
                $('#tr' + _Adid).remove();
                //cmn.ShowMessage("Deleted successfully.., ", "success");
            }
        }, this);
    });

    $(".edit").click(function () {
        
        $("[data-element=AdId]").val($(this).data("adid"));
        $("[data-element=link]").val($(this).data("linkurl"));
        //$("[data-element=CityName]").val($(this).data("city"));
        $("[data-element=Recordstatus]").val($(this).data("recordstatus"));


    });

    $(".openbannerimge").click(function () {
        $("#imgBannerImage").attr("src", $(this).attr("src"));
        $("#modalShowBannerImage").modal("show");
    });

    BindLookUp();

}

function AddNewAds(btn) {
    var objCommon = new Common();

    var LinkUrl = $("#txtLinkURL").val();
    var ImageSelected = $("#fileImage")[0].files[0];
    var ddlCity = $("[data-element=CityID]").val();

    var adid = $("[data-element=AdId]").val();

    if (adid == 0)
    {
        if (!ImageSelected) {
        objCommon.ShowMessage("Please select an image.", "error");
        return;
    }
    }

    if (ddlCity == "0") {
        objCommon.ShowMessage("Please select the Country.", "error");
        return
    }

    if (LinkUrl == "") {
        objCommon.ShowMessage("Please add Banner URL.", "error");
        return;
    }

    var fd = new FormData();
    fd.append("File", $("#fileImage")[0].files[0]);
    fd.append("LinkURL", LinkUrl);
    fd.append("City", ddlCity);

    if (adid == 0) {
        objCommon.AjaxCallFormData("Admin/Ads/AddNewAdsWithImage", fd, true, function (response) {
            console.log(response);
            if (response == "success") {
                LoadAdsRecords();
                $("#txtLinkURL").val('');
                $("#fileImage").val('');
                $("[data-element=CityName]").val('');
                $("[data-element=CityID]").val(0);
                $("[data-element=AdId]").val(0);
            }
            else {
                objCommon.ShowMessage(response, "error");
            }
        }, btn);
    } else {
        var data = objCommon.getValues("#form-add-ads");

        objCommon.AjaxCall("AdminAPI/updateAds", JSON.stringify(data), "POST", true, function (response) {
            console.log(response);
            if (response.Status == true) {
                LoadAdsRecords();
                $("#txtLinkURL").val('');
                $("#fileImage").val('');
                $("[data-element=CityName]").val('');
                $("[data-element=CityID]").val(0);
                $("[data-element=AdId]").val(0);

                $("[data-element=link]").val('');
                $("[data-element=CityName]").val('');
                $("[data-element=Recordstatus]").val('Active');

            }
            else {
                objCommon.ShowMessage(response.RetMessage, "error");
            }
        }, btn);
    }
}