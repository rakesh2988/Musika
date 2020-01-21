
//$(document).ajaxComplete(
//      function (event, xhr, settings) {
//          if (xhr.status == 401) {
//              window.location.href = "23.111.138.246/surveyadmin/account/login";
//          }
//      });


//<div class="modal fade" id="myGlobalModal" tabindex="-1" role="dialog" aria-labelledby="myModalLabel">
//        <div class="modal-dialog" role="document">
//            <div class="modal-content">
//                <div class="modal-body">
//                    <div class="modal-wrapper">
//                        <div class="modal-icon text-center" style="font-size:70px;">
//                            <i class="fa fa-question-circle"></i>
//                        </div>
//                        <div class="modal-text text-center">
//                            <h4>Are you sure?</h4>
//                            <p id="txtGlobal"></p>
//                        </div>
//                    </div>
//                </div>
//                <div class="modal-footer">
//                    <button type="button" class="btn btn-default" data-dismiss="modal">Close</button>
//                    <button type="button" class="btn btn-danger" id="btnGlobalConfirm">Confirm</button>
//                </div>
//            </div>
//        </div>
//    </div>


//$("[name=BannerURLEmail]").change(function () {
//    $("#IsBanner1Changed").val("1");

//    if (this.files && this.files[0]) {
//        var FR = new FileReader();
//        FR.onload = function (e) {
//            $("#imgBanner1").html("<img src='" + e.target.result + "' style='width:100%' />");
//        };
//        FR.readAsDataURL(this.files[0]);
//    }
//});

var Common = function () {
    _this = this;
    var baseUrl = "http://localhost:55172/";
   // var baseUrl = "http://appserver.musikaapp.com/";
    _this.AjaxDelete = function (url, data, methodType, isAsync, callback, btn) {
        $.ajax({
            dataType: 'json',
            crossDomain: true,
            contentType: 'application/json; charset=utf-8', // text for IE, xml for the rest ,
            url: baseUrl + url,
            type: "post",
            data: { _method: "DELETE" },
            success: function (data) { callback(data); },
            error: function (jqXhr, textStatus, errorThrown) {
                if (jqXhr.getResponseHeader('Content-Type').indexOf('application/json') > -1) {
                    // only parse the response if you know it is JSON
                    var error = $.parseJSON(jqXhr.responseText);
                    _this.ShowMessage(error.Message, "error");
                } else {
                    _this.ShowMessage("Oops! Something went wrong, please try again later.", "error");
                }
            }
        });
    };

    _this.AjaxCall = function (url, data, methodType, isAsync, callback, btn) {
        var value = $(btn).val();
        $(btn).val("Processing...").prop("disabled", true);
        $.ajax({
            type: methodType,
            dataType: 'json',
            crossDomain: true,
            contentType: 'application/json; charset=utf-8', // text for IE, xml for the rest ,
            url: baseUrl + url,
            data: data,
            async: isAsync,
            success: function (response) {
                $(btn).val(value).prop("disabled", false);
                callback(response);
            },
            error: function (jqXhr, textStatus, errorThrown) {
                if (jqXhr.getResponseHeader('Content-Type').indexOf('application/json') > -1) {
                    // only parse the response if you know it is JSON
                    var error = $.parseJSON(jqXhr.responseText);
                    _this.ShowMessage(error.Message, "error");
                } else {
                    _this.ShowMessage("Oops! Something went wrong, please try again later.", "error");
                }
                //$(".modal").modal("hide");
                $(btn).val(value).prop("disabled", false);
            }
        });
    };


    _this.ConfirmDeleteAjaxCall = function (url, data, methodType, isAsync, callback, btn, txt) {
        if (txt != "" && txt != null) {
            $("#txtGlobal").text(txt);
        }
        else {
            $("#txtGlobal").text("Are you sure that you want to delete?");
        }

        $("#myGlobalModal").modal("show");

        $("#btnGlobalConfirm").click(function () {
            _this.AjaxDelete(url, data, methodType, isAsync, callback, btn);
            $("#myGlobalModal").modal("hide");
        });
    };


    _this.ConfirmAjaxCall = function (url, data, methodType, isAsync, callback, btn, txt) {
        if (txt != "" && txt != null) {
            $("#txtGlobal").text(txt);
        }
        else {
            $("#txtGlobal").text("Are you sure that you want to delete?");
        }

        $("#myGlobalModal").modal("show");

        $("#btnGlobalConfirm").click(function () {
            _this.AjaxCall(url, data, methodType, isAsync, callback, btn);
            $("#myGlobalModal").modal("hide");
        });
    };

    _this.AjaxCallFormData = function (url, data, isAsync, callback, btn) {
        var value = $(btn).val();
        $(btn).val("Processing...").prop("disabled", true);

        $.ajax({
            url: baseUrl + url,
            data: data,
            //crossDomain: true,
            contentType: false,
            processData: false,
            async: isAsync,
            type: 'POST',
            //headers: {
            //    "Authorization": "Basic " + btoa("admin" + ":" + "admin")
            //},
            success: function (response) {
                $(btn).val(value).prop("disabled", false);
                callback(response);
            },
            error: function (jqXhr, textStatus, errorThrown) {
                $(btn).val(value).prop("disabled", false);
                if (jqXhr.getResponseHeader('Content-Type').indexOf('application/json') > -1) {
                    // only parse the response if you know it is JSON
                    var error = $.parseJSON(jqXhr.responseText);
                    _this.ShowMessage(error, "error");
                } else {
                    _this.ShowMessage("Oops! Something went wrong, please try again later.", "error");
                }
                //$(".modal").modal("hide");
            }
        });
    };

    _this.Confirm = function (btn, txt) {
        if (txt != "" && txt != null) {
            $("#txtGlobal").text(txt);
        }
        else {
            $("#txtGlobal").text("Are you sure that you want to delete?");
        }
        $("#myGlobalModal").modal("show");
        $("#btnGlobalConfirm").click(function () {
            $(btn).trigger("click");
            $("#myGlobalModal").modal("hide");
            return true;
        });
    };

    _this.Delete = function (url, data, methodType, isAsync, callback, btn, txt) {
        if (txt != "" && txt != null) {
            $("#lblSure").text(txt);
        }
        else {
            $("#lblSure").text("Are you sure to delete record?");
        }
        $("#myModalConfirm").modal("show");
        $("#confimdelete").click(function () {
            _this.AjaxCall(url, data, methodType, isAsync, callback, btn);
            $("#myModalConfirm").modal("hide");
        });
    };

    _this.ShowMessage = function (msg, type) {
        toastr.options = {
            "closeButton": true,
            "debug": false,
            "newestOnTop": true,
            "progressBar": true,
            "positionClass": "toast-bottom-full-width",
            "preventDuplicates": false,
            "onclick": null,
            "showDuration": "300",
            "hideDuration": "1000",
            "timeOut": "10000",
            "extendedTimeOut": "1000",
            "showEasing": "swing",
            "hideEasing": "linear",
            "showMethod": "fadeIn",
            "hideMethod": "fadeOut"
        };
        toastr[type](msg, type.charAt(0).toUpperCase() + type.slice(1))
    };

    _this.validate = function (id) {
        // Validation
        if ($("#" + id).length > 0) {
            if (!$("#" + id).validationEngine('validate',
                {
                    scroll: false,
                    promptPosition: "bottomLeft"
                })) {
                return false;
            }
            else {
                return true;
            }
        }
    };

    _this.getValues = function (id, cls) {
        var json = {};
        $(id, cls).find("input[type=text], select, textarea, input[type=hidden]").each(function () {
            if ($(this).is("[data-element]")) {
                json[$(this).attr("data-element")] = $(this).val();
            }
        });
        return json;
    };

    _this.GetFormValues = function (form) {
        var json = {};
        $(form).find("input, select, textarea").not("input[type=radio]").each(function () {
            if ($(this).is("[name]")) {
                if ($(this).is(':checkbox')) {
                    json[$(this).attr("name")] = $(this).is(":checked");
                }
                else {
                    json[$(this).attr("name")] = $(this).val();
                }
            }

        });
        return json;
    };

    _this.ClearFormValues = function (form) {
        var json = {};
        $(form).find("input[type=text], select, textarea, input[type=hidden]").each(function () {
            if ($(this).is("[data-element]")) {
                json[$(this).attr("data-element")] = $(this).val("");
            }
        });
        return json;
    };

    _this.GetQueryStringParams = function (sParam) {
        var sPageURL = window.location.search.substring(1);
        var sURLVariables = sPageURL.split('&');
        for (var i = 0; i < sURLVariables.length; i++) {
            var sParameterName = sURLVariables[i].split('=');
            if (sParameterName[0] == sParam) {
                return sParameterName[1];
            }
        }
    };

    _this.CommaNumber = function (num) {
        return Number(num).toLocaleString('en');
    };

    _this.getFormObj = function (formId) {
        var formObj = new FormData(); ``
        var inputs = $("#" + formId).serializeArray();
        //var inputs = $("#" + formId).closest('div').find("input,select").serialize();
        $.each(inputs, function (i, input) {
            formObj.append(input.name, input.value);
        });
        return formObj;
    };

    //D:\Salman_SDS\Projects\TFS\TellUsKnow\TellUsNow\Areas\Admin\Content\js\plugins\jquery-file-upload\jquery.fileupload.js
    _this.readImage = function (id) {
        if (this.files && this.files[0]) {
            var FR = new FileReader();
            FR.onload = function (e) {
                $('#' + id).attr("src", e.target.result);
                //$('#' + id).html("<img src='" + e.target.result + "' style='width:100%' />");
                //$("#base64").val(e.target.result);
            };
            FR.readAsDataURL(this.files[0]);
        }
    };
};