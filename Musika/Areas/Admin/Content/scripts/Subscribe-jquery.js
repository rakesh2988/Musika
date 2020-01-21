var pageNumber = 1;
$(document).ready(function () {
    $('#myModal').on('hidden.bs.modal', function () {
        ClearForm();
    })
    $("#btnSearch").click(function () {
        FilterRecords(1);
    });
    $("#btnSaveChanges").click(function () {
        UpdateUser(this);
    });

    $("#btnExport").click(function () {
        generate_excel('tblSubscribeList');
    });



    $("#btnShowPassword").click(function () {
        $("#pnlPassword").show();
        $("[data-element=Password]").addClass("validate[required]");
        $("#confirmPassword").addClass("validate[required, equals[password]]");
    });
    FilterRecords(pageNumber);

});

function FilterRecords(page) {
    pageNumber = page;
    var c = new Common();
    var data = c.getValues("#form-search-users", "");
    data["Pageindex"] = page;
    data["Pagesize"] = 30;

    
    $("#tbodyUsers").html("");
    c.AjaxCall("api/Admin/GetSubscribeUsers", $.param(data), "GET", true, function (d) {
        CreateTable(d);
        Paginate(d);
    });
}


function CreateTable(data) {
    var html = "";
   
    for (var i = 0; i < data.Items.length; i++) {
        html += "<tr>"
        + "<td>" +dateFormat(data.Items[i].CreatedDate, "mm/dd/yyyy, h:MM:ss TT", "Standard")  + "</td>"
        + "<td>" + data.Items[i].Email + "</td>"
        + "<td><span class='" + (data.Items[i].RecordStatus == "Active" ? "text-success" : "text-info") + "'>" + data.Items[i].RecordStatus + "</span></td>"
        + "</tr>";
    }
    $("#tbodyUsers").html(html);
    BindEvents();
}

function Paginate(data) {
    var pagination = "";
    var counter = 0;
    if (data.PageNumber>1) {
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

function roundTo2Decimals(numberToRound) {
    return Math.round(numberToRound * 100) / 100
}


function BindEvents() {
    var cmn = new Common();
    $(".view").click(function () {
        var Rating = $(this).data("rating");

        cmn.AjaxCall("api/Admin/GetUserByID?ID=" + $(this).data("userid"), {}, "GET", true, function (d) {
            console.log(d);
            $("#myModal").modal("show");
            $("#lblRecordStatus").html(d.users.RecordStatus);
            $("#btnActive").val(d.users.RecordStatus == 'Active' ? 'InActive' : 'Active');
            $("[data-element=Email]").val(d.users.Email);
            $("[data-element=FullName]").val(d.users.FullName);
            $("[data-rating=Rating]").val(Rating);
            $("#starrating").html('<i class="' + "star star-" + Math.round(Rating) + '"></i>');
           
            $("[data-element=Gender]").val(d.users.Gender);
            $("[data-element=Education]").val(d.users.Education);
            $("[data-element=SchoolName]").val(d.users.SchoolName);
            $("[data-element=InactivePeriod]").val(d.users.InactivePeriod == null ? -1 : d.users.InactivePeriod);

            var html = "";
            for (var i = 0; i < d.Subject.length; i++) {
                html += "<tr>"
                + "<td>" + d.Subject[i].SubjectName + "</td>"
                + "</tr>";
            }
            $("#tbodySubjects").html(html);

            html = "";
            html += '<div class="sliderimg" style="position:relative; float:left; margin-right:15px; margin-top:15px;">'
                             + '<img src=' + d.users.ThumbnailURL + ' />'
                              + '<img src="23.111.138.246/Areas/Admin/Content/img/cross.png" data-userid="' + d.users.UserID + '" class="deletephoto" style="position:absolute; top:-5px; right:-5px; width:24px; display:none; cursor:pointer;" />'
                            + '</div>';

            //html += '<span><img src=' + d.users.ThumbnailURL + ' /></span>' +
            //       '<span><img src="23.111.138.246/Areas/Admin/Content/img/cross.png" data-userid="' + d.users.UserID + '" class="deletephoto" style="position:absolute; top:-5px; right:-5px; width:24px;  cursor:pointer;" /></span>'

            $("#imgProfile").html(html);
            $("[data-userid=UserId]").val(d.users.UserID);
            $("[data-element=UserId]").val(d.users.UserID);

            if (d.users.UserID != 1) {
                $("#btnActive").show();
            } else {
                $("#btnActive").hide();
            }

            //$("#lblfrat").val(fratname);
            //$("#idrole").val(d.Role);
            //$("[data-element=FirstName]").val(d.FirstName);
            //$("[data-element=LastName]").val(d.LastName);
            //$("[data-element=UserState]").val(d.State);
            //$("[data-element=UserSchool]").val(d.SchoolId);
            //$("[data-element=UserStatus]").val(status);
            //$("[data-element=UserId]").val(d.UserId);
           // $("#imgProfile").html("<img src='" + d.ThumbnailURL + "' />");
            var html = "";
            //for (var i = 0; i < d.PhotoGallery.length; i++) {
            //    html += '<div class="sliderimg" style="position:relative; float:left; margin-right:15px; margin-top:15px;">'
            //                 + '<img src=' + d.PhotoGallery[i].ThumbnailURL + ' />'
            //                  + '<img src="http://localhost:60737/Areas/Admin/Content/img/cross.png" data-photoid="' + d.PhotoGallery[i].PhotoId + '" class="deletephoto" style="position:absolute; top:-5px; right:-5px; width:24px; display:none; cursor:pointer;" />'
            //                + '</div>';
            //}
            //$("#slider").html(html);

            $(".sliderimg").hover(function () {
                $(this).children().eq(1).show()
                //var index = $(".sliderimg").index();
                //$(this).children().eq(1).show().click(function () {
                //    d.PhotoGallery.splice(index, 1);
                //    $(this).parent().remove();
                //});
            }, function () {
                $(this).children().eq(1).hide();
            });

            //$(".sliderimg").hover(function () {
            //    var index = $(".sliderimg").index();
            //    $(this).children().eq(1).show().click(function () {
            //        d.PhotoGallery.splice(index, 1);
            //        $(this).parent().remove();
            //    });
            //}, function () {
            //    $(this).children().eq(1).hide();
            //});

            InitDeletePhoto();
            InitChangeStatus();
        }, this);

    });
    //$(".delete").click(function () {
    //    cmn.Delete("api/users/delete/" + $(this).data("userid"), {}, "DELETE", true, function (d) {
    //        if (d) {
    //            FilterRecords(pageNumber);
    //        }
    //    }, this);
    //});

    $(".Asked").click(function () {
        var url = "Admin/DashBoard/Question/" + $(this).data("userid");
        location.href = url;
    });

    $(".Answered").click(function () {
        var url = "Admin/DashBoard/Answer/" + $(this).data("userid") + "|user";
        location.href = url;
    });

    
}

function InitChangeStatus() {
    $("#btnActive").click(function () {
        new Common().AjaxCall("api/Admin/ChangeUserStatus/?ID=" + $("[data-userid=UserId]").val() + "&InactivePeriod=" + $("[data-element=InactivePeriod]").val(), {}, "POST", true, function (d) {
            if (d) {
                var c = new Common();
                $("#btnActive").hide();
                c.ShowMessage("Status Changed successfully.", "success");
                $("#lblRecordStatus").html() == "Active" ? $("#lblRecordStatus").html("InActive") : $("#lblRecordStatus").html("Active");
            }
        }, this);
      
    });
}



function InitDeletePhoto() {
    $(".deletephoto").click(function () {
        new Common().AjaxCall("api/Admin/deletephoto/" + $(this).data("userid"), {}, "DELETE", true, function (d) { });
        var c = new Common();
        c.ShowMessage("Photo Removed successfully", "success");
        $("#imgProfile").html("");
    });
}

function ClearForm() {
    $("#lblRecordStatus").html("");
    $("#lblRecordStatus2").html("");
    $("[data-element=Email]").val("");
    $("[data-element=FullName]").val("");
    $("[data-rating=Rating]").val(0);
    $("#starrating").html('<i class="' + "star star-" + 0 + '"></i>');
    $("[data-element=Gender]").val("");
    $("[data-element=InactivePeriod]").val("");
    $("[data-element=Education]").val("");
    $("[data-element=SchoolName]").val("");

    $("[data-element=Password]").val("").removeClass("validate[required]");
    $("#confirmPassword").val("").removeClass("validate[required, equals[password]]");
    $("#imgProfile").html("");
    $("#pnlPassword").hide();
    $("#btnActive").hide();
    $("#btnActive").unbind();

}

function UpdateUser(btn) {
    var c = new Common();
    if (c.validate("form-user")) {
        var data = c.getValues("#form-user");
        if ($("#pnlPassword").is(":visible")) {
            data["Password"] = $("[data-element=Password]").val();
        }
        c.AjaxCall("api/Admin/updateuser", JSON.stringify(data), "POST", true, function (d) {
            if (d) {
                $('#myModal').modal("hide");
                c.ShowMessage("User updated successfully.", "success");
                FilterRecords(pageNumber);
            }
        }, btn);
    }
}


function generate_excel(tableid) {
    var table= document.getElementById(tableid);
    var html = table.outerHTML;
    window.open('data:application/vnd.ms-excel;base64,' + base64_encode(html));
}

function base64_encode (data) {
    // http://kevin.vanzonneveld.net
    // +   original by: Tyler Akins (http://rumkin.com)
    // +   improved by: Bayron Guevara
    // +   improved by: Thunder.m
    // +   improved by: Kevin van Zonneveld (http://kevin.vanzonneveld.net)
    // +   bugfixed by: Pellentesque Malesuada
    // +   improved by: Kevin van Zonneveld (http://kevin.vanzonneveld.net)
    // +   improved by: Rafal Kukawski (http://kukawski.pl)
    // *     example 1: base64_encode('Kevin van Zonneveld');
    // *     returns 1: 'S2V2aW4gdmFuIFpvbm5ldmVsZA=='
    // mozilla has this native
    // - but breaks in 2.0.0.12!
    //if (typeof this.window['btoa'] == 'function') {
    //    return btoa(data);
    //}
    var b64 = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/=";
    var o1, o2, o3, h1, h2, h3, h4, bits, i = 0,
      ac = 0,
      enc = "",
      tmp_arr = [];

    if (!data) {
        return data;
    }

    do { // pack three octets into four hexets
        o1 = data.charCodeAt(i++);
        o2 = data.charCodeAt(i++);
        o3 = data.charCodeAt(i++);

        bits = o1 << 16 | o2 << 8 | o3;

        h1 = bits >> 18 & 0x3f;
        h2 = bits >> 12 & 0x3f;
        h3 = bits >> 6 & 0x3f;
        h4 = bits & 0x3f;

        // use hexets to index into b64, and append result to encoded string
        tmp_arr[ac++] = b64.charAt(h1) + b64.charAt(h2) + b64.charAt(h3) + b64.charAt(h4);
    } while (i < data.length);

    enc = tmp_arr.join('');

    var r = data.length % 3;

    return (r ? enc.slice(0, r - 3) : enc) + '==='.slice(r || 3);

}




var dateFormat = function () {
    var token = /d{1,4}|m{1,4}|yy(?:yy)?|([HhMsTt])\1?|[LloSZ]|"[^"]*"|'[^']*'/g,
        timezone = /\b(?:[PMCEA][SDP]T|(?:Pacific|Mountain|Central|Eastern|Atlantic) (?:Standard|Daylight|Prevailing) Time|(?:GMT|UTC)(?:[-+]\d{4})?)\b/g,
        timezoneClip = /[^-+\dA-Z]/g,
        pad = function (val, len) {
            val = String(val);
            len = len || 2;
            while (val.length < len) val = "0" + val;
            return val;
        };

    // Regexes and supporting functions are cached through closure
    return function (date, mask, utc) {
        var dF = dateFormat;

        // You can't provide utc if you skip other args (use the "UTC:" mask prefix)
        if (arguments.length == 1 && Object.prototype.toString.call(date) == "[object String]" && !/\d/.test(date)) {
            mask = date;
            date = undefined;
        }

        // Passing date through Date applies Date.parse, if necessary
        date = date ? new Date(date) : new Date;
        if (isNaN(date)) throw SyntaxError("invalid date");

        mask = String(dF.masks[mask] || mask || dF.masks["default"]);

        // Allow setting the utc argument via the mask
        if (mask.slice(0, 4) == "UTC:") {
            mask = mask.slice(4);
            utc = true;
        }

        var _ = utc ? "getUTC" : "get",
            d = date[_ + "Date"](),
            D = date[_ + "Day"](),
            m = date[_ + "Month"](),
            y = date[_ + "FullYear"](),
            H = date[_ + "Hours"](),
            M = date[_ + "Minutes"](),
            s = date[_ + "Seconds"](),
            L = date[_ + "Milliseconds"](),
            o = utc ? 0 : date.getTimezoneOffset(),
            flags = {
                d: d,
                dd: pad(d),
                ddd: dF.i18n.dayNames[D],
                dddd: dF.i18n.dayNames[D + 7],
                m: m + 1,
                mm: pad(m + 1),
                mmm: dF.i18n.monthNames[m],
                mmmm: dF.i18n.monthNames[m + 12],
                yy: String(y).slice(2),
                yyyy: y,
                h: H % 12 || 12,
                hh: pad(H % 12 || 12),
                H: H,
                HH: pad(H),
                M: M,
                MM: pad(M),
                s: s,
                ss: pad(s),
                l: pad(L, 3),
                L: pad(L > 99 ? Math.round(L / 10) : L),
                t: H < 12 ? "a" : "p",
                tt: H < 12 ? "am" : "pm",
                T: H < 12 ? "A" : "P",
                TT: H < 12 ? "AM" : "PM",
                Z: utc ? "UTC" : (String(date).match(timezone) || [""]).pop().replace(timezoneClip, ""),
                o: (o > 0 ? "-" : "+") + pad(Math.floor(Math.abs(o) / 60) * 100 + Math.abs(o) % 60, 4),
                S: ["th", "st", "nd", "rd"][d % 10 > 3 ? 0 : (d % 100 - d % 10 != 10) * d % 10]
            };

        return mask.replace(token, function ($0) {
            return $0 in flags ? flags[$0] : $0.slice(1, $0.length - 1);
        });
    };
}();

// Some common format strings
dateFormat.masks = {
    "default": "ddd mmm dd yyyy HH:MM:ss",
    shortDate: "m/d/yy",
    mediumDate: "mmm d, yyyy",
    longDate: "mmmm d, yyyy",
    fullDate: "dddd, mmmm d, yyyy",
    shortTime: "h:MM TT",
    mediumTime: "h:MM:ss TT",
    longTime: "h:MM:ss TT Z",
    isoDate: "yyyy-mm-dd",
    isoTime: "HH:MM:ss",
    isoDateTime: "yyyy-mm-dd'T'HH:MM:ss",
    isoUtcDateTime: "UTC:yyyy-mm-dd'T'HH:MM:ss'Z'"
};

// Internationalization strings
dateFormat.i18n = {
    dayNames: [
        "Sun", "Mon", "Tue", "Wed", "Thu", "Fri", "Sat",
        "Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday"
    ],
    monthNames: [
        "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec",
        "January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December"
    ]
};

// For convenience...
Date.prototype.format = function (mask, utc) {
    return dateFormat(this, mask, utc);
};