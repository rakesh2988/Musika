<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="TicketEventCheckout.aspx.cs" MaintainScrollPositionOnPostback="true" EnableEventValidation="false" Inherits="Musika.TicketEventCheckout1" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
    <link href="Content/Ticketing.css" rel="stylesheet" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/4.7.0/css/font-awesome.min.css" />

    <link type="text/css" rel="stylesheet" href="http://code.jquery.com/ui/1.10.3/themes/smoothness/jquery-ui.css" />
    <script type="text/javascript" src="http://code.jquery.com/jquery-1.8.2.js"></script>

    <script src="https://code.jquery.com/ui/1.12.1/jquery-ui.min.js"></script>
    <link href="https://code.jquery.com/ui/1.12.1/themes/base/jquery-ui.css" rel="stylesheet" />

    <script type="text/javascript" src='https://cdnjs.cloudflare.com/ajax/libs/twitter-bootstrap/3.0.3/js/bootstrap.min.js'></script>

    <link rel="stylesheet" href='https://cdnjs.cloudflare.com/ajax/libs/twitter-bootstrap/3.0.3/css/bootstrap.min.css'
        media="screen" />
    <link rel="stylesheet" href="https://unpkg.com/bootstrap-material-design@4.1.1/dist/css/bootstrap-material-design.min.css"
        integrity="sha384-wXznGJNEXNG1NFsbm0ugrLFMQPWswR3lds2VeinahP8N0zJw9VWSopbjv2x7WCvX" crossorigin="anonymous" />


    <script src="https://unpkg.com/popper.js@1.12.6/dist/umd/popper.js" integrity="sha384-fA23ZRQ3G/J53mElWqVJEGJzU0sTs+SvzG8fXVWP+kJQ1lwFAOkcUOysnlKJC33U" crossorigin="anonymous"></script>
    <script src="https://unpkg.com/bootstrap-material-design@4.1.1/dist/js/bootstrap-material-design.js" integrity="sha384-CauSuKpEqAFajSpkdjv3z9t8E7RlpJ1UP0lKM/+NdtSarroVKu069AlsRPKkFBz9" crossorigin="anonymous"></script>

    <%--<script src="http://api.geonames.org/export/geonamesData.js?username=musika" type="text/javascript"></script>--%>
    <script>$(document).ready(function () { $('body').bootstrapMaterialDesign(); });</script>
    <script>

        $(document).ready(function () {
            BindPopup();
            //  setDefaultCountry();
            if ($('#<%=hdnTab.ClientID %>').val() != "PayPal") {
           <%-- $('#<%=hdncountry.ClientID %>').val($('#ddlCountry option:selected').text());--%>
                if ($('#<%=hdncountry.ClientID %>').val() == "") {
                    var data = GetChildren($('#ddlCountry option:selected').val(), "States", $(".txtCustomerState"));
                    var val = document.getElementById('RequiredFieldValidator7');
                    //ValidatorEnable(val, false);
                }
                else {
                    GetChildren($('#ddlCountry option:selected').val(), "States", $(".txtCustomerState"));
                    var val = document.getElementById('RequiredFieldValidator7');
                    //ValidatorEnable(val, false);
                }
            }
            $('#<%=ddlCountry.ClientID %>').change(function () {
                $('#<%=hdncountry.ClientID %>').val($('#ddlCountry option:selected').text());
                GetChildren($('#ddlCountry option:selected').val(), "States", $(".txtCustomerState"));
                $('#<%=hdnSelectedCountry.ClientID %>').val($('#ddlCountry option:selected').text());
            });
            $('#<%=ddlState.ClientID %>').change(function () {
                $('#<%=hdnState.ClientID %>').val($('#ddlState option:selected').text());
                $('#<%=hdnSelectedState.ClientID %>').val($('#ddlState option:selected').text());
                if ($('#ddlState option:selected').val() != "-1") { ValidatorEnable(val, false); }
                else { ValidatorEnable(val, true); }
            });
        });


        function GetCountries() {
            $.ajax({
                type: "GET",
                url: "http://api.geonames.org/countryInfoJSON?formatted=true&lang=en&style=full&username=musika",
                //url:"http://api.geonames.org/countryInfo?username=musika",
                contentType: "application/json; charset=utf-8",
                dataType: "jsonp",
                success: function (data) {
                    // $(".ddlCustomerCountry").append($('<option />', { value: -1, text: 'Select Country' }));
                    // $(data.geonames).each(function (index, item) {
                    //   $(".ddlCustomerCountry").append($('<option />', { value: item.geonameId, text: item.countryName }));
                    // });
                },
                error: function (data) {
                    alert("Failed to get countries.");
                }
            });
        }

        function GetStates() {
            $(".ddlCustomerCountry").change(function () {
                GetChildren($(this).val(), "States", $(".txtCustomerState"));
            });
        }

        function GetChildren(geoNameId, childType, ddlSelector) {
            $.ajax({
                type: "GET",
                url: "http://api.geonames.org/childrenJSON?geonameId=" + geoNameId + "&username=musika",
                contentType: "application/json; charset=utf-8",
                dataType: "jsonp",
                success: function (data) {
                    $(ddlSelector).empty();
                    $(ddlSelector).append($('<option />', { value: -1, text: 'Select ' + childType }));
                    $(data.geonames).each(function (index, item) {
                        if (item.adminCodes1) {
                            $(ddlSelector).append($('<option />', { value: Object.values(item.adminCodes1)[0], text: item.name }));
                        }
                        else {
                            $(ddlSelector).append($('<option />', { value: item.geonameId, text: item.name }));
                        }
                    });

                    if ($('#<%=hdnSelectedState.ClientID %>').val() != "") {
                        var text1 = $('#<%=hdnSelectedState.ClientID %>').val();
                        var filter = $("#ddlState option").filter(function () {
                            return this.value.toLowerCase() == text1.toLowerCase() || this.text.toLowerCase() == text1.toLowerCase();
                        }).attr('selected', true);
                        var val = document.getElementById('RequiredFieldValidator7');
                        if (filter.length == 0) {
                            if ($('#ddlState option:selected').val() != "-1") {
                                ValidatorEnable(val, false);

                            }
                            else {
                                //ValidatorEnable(val, true);
                            }
                        }
                        else {
                            ValidatorEnable(val, false);
                        }
                    }

                },
                error: function (data) {
                    alert("failed to get data");
                }
            });
        }

    </script>

    <script type="text/javascript">
        function showAndroidToast(toast) {
            Android.showToast(toast);
        }

    </script>

    <style>
        .row.cover_ticketing {
            float: left;
            width: 100%;
            padding: 12px 15px;
            margin: 0;
            box-shadow: 0px 0px 4px rgba(0, 0, 0, 0.2);
            position: relative;
        }

            .row.cover_ticketing:before {
                content: "";
                height: 100px;
                /*width: 1px;*/
                width: 0px;
                background-color: #ddd;
                left: 48%;
                position: absolute;
                top: 85px;
            }

        .cover_ticketing p span#Label10, .cover_ticketing p span#Label4, .cover_ticketing p span#Label5, .cover_ticketing p span#Label6, .cover_ticketing p span#Label7, .cover_ticketing p span#Label8, .cover_ticketing p span#Label9 {
            font-weight: 700
        }

        .form_cover_main h2 {
            font-weight: 700
        }

        .row.cover_ticketing h2 {
            width: 100%;
            color: #428bca;
            border-bottom: 1px solid #ccc;
            float: left;
            margin-bottom: 18px;
            padding-bottom: 10px;
            font-weight: bold;
        }

        .cover_ticketing p {
            float: left;
            width: 100%;
            /* width: 50%; */
            padding: 0 5px;
            padding-bottom: 10px;
            font-size: 14px;
            border-bottom: 1px solid #eee;
            color: #555;
        }
        p.ticketSummary {
            border-bottom: 0;
            padding-bottom: 0;
        }
        p:empty {
            display: none
        }

        .form_cover_main {
            padding: 0
        }

            .form_cover_main span input {
                background-color: #fff !important;
                border-radius: 10px !important;
                color: #000 !important
            }

        .btn-primary {
            color: #caf4ff !important;
            background-color: #000 !important;
            border-color: #000 !important;
            padding: 9px 16px !important;
            font-size: 16px !important
        }

        div#divPay .biling-cover:after {
            content: none
        }

        .modal-content {
            background-color: #caf4ff;
            border: 1px solid #caf4ff
        }

        .btn-danger {
            color: #fff !important;
            background-color: #000 !important;
            border-color: #000 !important;
            width: auto !important;
            padding: 7px 13px !important;
            margin-right: 20px !important;
            border-radius: 6px
        }

        .modal-footer {
            bottom: 5%;
        }

        div#navigation {
            width: 100%;
            float: left;
            text-align: left;
            margin: 0 0 30px 0;
        }

            div#navigation span {
                width: auto;
                display: inline-block;
                float: none;
                background-color: #428bca;
                padding: 0 0px 0 0px;
                border-radius: 4px;
                margin: 0 0 4px
            }
            .row.cover_ticketing h2 i {
                margin-right: 7px;
                font-size: 22px;
                color: #428bca;
            }
            div#navigation span.bmd-form-group {
                margin-top: 18px !important;
            }
            div#navigation span.bmd-form-group select#drpPaymentMethod {
                padding-left: 36px;
            }
            div#navigation span.bmd-form-group:after {
                content: "\f09d";
                font-family: fontawesome;
                position: absolute;
                left: 9px;
                font-size: 17px;
                top: 6px;
                color: #fff;
            }
                div#navigation span i {
                    color: #fff;
                    padding-right: 6px
                }

            div#navigation input {
                width: auto;
                float: none;
                margin-right: 15px;
                background-color: #428bca !important;
                color: #fff !important;
                display: inline-block
            }

        .ui-dialog {
            top: 170px !important;
            left: 0 !important;
            right: 0 !important;
            margin: 0 auto !important;
            position: fixed !important
        }
    </style>
</head>
<body>
    <section class="billing--section">
        <div class="billing_wrapper">
            <div class="row cover_ticketing">
                <p class="ticketSummary">
                    <h2>
                        <i class="fa fa-ticket" aria-hidden="true"></i><asp:Label ID="Label3" Text="<%$Resources:Resource, TICKETSUMMARY %>" runat="server" /></h2>
                </p>
                <p>
                    <asp:Label ID="Label4" Text="<%$Resources:Resource, TicketPackage %>" runat="server" />
                    :
                <asp:Label ID="lblPackage" runat="server" Text=""></asp:Label>
                </p>
                <p>
                    <asp:Label ID="Label5" Text="<%$Resources:Resource, NoofTickets %>" runat="server" />
                    :
                <asp:Label ID="lblNumbers" runat="server" Text=""></asp:Label>
                </p>
                <p>
                    <asp:Label ID="Label6" Text="<%$Resources:Resource, TotalPrice %>" runat="server" />
                    :
                <asp:Label ID="lblTotalPrice" runat="server" Text=""></asp:Label>
                </p>
                <p>
                    <asp:Label ID="Label7" Text="<%$Resources:Resource, ArtistName %>" runat="server" />
                    :
                <asp:Label ID="lblArtistName" runat="server" Text=""></asp:Label>
                </p>
                <p>
                    <asp:Label ID="Label8" Text="<%$Resources:Resource, EventName %>" runat="server" />
                    :
                <asp:Label ID="lblEventName" runat="server" Text=""></asp:Label>
                </p>
                <p>
                    <asp:Label ID="Label9" Text="<%$Resources:Resource, EventStartDate %>" runat="server" />
                    :
                <asp:Label ID="lblStartDate" runat="server" Text=""></asp:Label>
                </p>
                <p>
                    <asp:Label ID="Label10" Text="<%$Resources:Resource, EventStartTime %>" runat="server" />:
                <asp:Label ID="lblTime" runat="server" Text=""></asp:Label>
                </p>
            </div>
            <form id="form1" class="form_cover_main" runat="server" autocomplete="off">
                <asp:HiddenField ID="hdnTab" runat="server" />
                <asp:HiddenField ID="hdnCardNumber" runat="server" />
                <asp:ScriptManager ID="ScriptManager1" runat="server"></asp:ScriptManager>
               
                <div id="navigation" runat="server">
                    <asp:Label ID="lblpayment" runat="server" Text="<%$Resources:Resource, SelectPaymentMethod %>" CssClass="slct_pymnt"></asp:Label>
                    <%-- <label class="slct_pymnt">select payment method</label>--%>
                    <asp:DropDownList class="form-control" ID="drpPaymentMethod" AutoPostBack="true" runat="server"
                        OnSelectedIndexChanged="drpPaymentMethod_SelectedIndexChanged">
                        <asp:ListItem Value="1" Text="<%$Resources:Resource, CreditCard %>"></asp:ListItem>
                        <asp:ListItem Value="0" Text="PayPal"></asp:ListItem>
                    </asp:DropDownList>

                </div>
                <asp:HiddenField ID="hdncountry" runat="server" />
                <asp:HiddenField ID="hdnSelectedState" runat="server" />
                <asp:HiddenField ID="hdnSelectedCountry" runat="server" />

                <asp:HiddenField ID="hdnState" runat="server" />
                <%-- <asp:UpdatePanel ID="updpanel" runat="server">
                    <ContentTemplate>--%>
                <div class="billing_card-info">
                    <div id="country" runat="server" class="form-group bmd-label-floating hide1">
                        <p class="country_label">
                            <asp:Label ID="Label15" Text="<%$Resources:Resource, Country %>" runat="server" />
                        </p>

                        <div class="select__wrapper">
                            <asp:DropDownList class="form-control ddlCustomerCountry" CssClass="ddlCustomerCountry" ID="ddlCountry" OnChange="ValidateIf();" runat="server">
                                <%-- <asp:ListItem Value="-1">Select Country</asp:ListItem>--%>
                                <asp:ListItem title="DO" Value="3508796">Dominican Republic</asp:ListItem>
                                <asp:ListItem title="US" Value="6252001">United States</asp:ListItem>
                                <asp:ListItem title="GB" Value="2635167">United Kingdom</asp:ListItem>
                                <asp:ListItem title="MX" Value="3996063">Mexico</asp:ListItem>
                                <asp:ListItem title="ES" Value="2510769">Spain</asp:ListItem>

                                <asp:ListItem title="AF" Value="1149361">Afghanistan</asp:ListItem>
                                <asp:ListItem title="AL" Value="783754">Albania</asp:ListItem>
                                <asp:ListItem title="DZ" Value="2589581">Algeria</asp:ListItem>
                                <%--<asp:ListItem title="US" Value="5880801">American Samoa</asp:ListItem>--%>
                                <asp:ListItem title="AD" Value="3041565">Andorra</asp:ListItem>
                                <asp:ListItem title="AO" Value="3351879">Angola</asp:ListItem>
                                <asp:ListItem title="AI" Value="3573511">Anguilla</asp:ListItem>
                                <%-- <asp:ListItem title="US" Value="6697173">Antarctica</asp:ListItem>--%>
                                <asp:ListItem title="AG" Value="3576396">Antigua and Barbuda</asp:ListItem>

                                <asp:ListItem title="AR" Value="3865483">Argentina</asp:ListItem>
                                <asp:ListItem title="AM" Value="174982">Armenia</asp:ListItem>
                                <%-- <asp:ListItem title="US" Value="3577279">Aruba</asp:ListItem>--%>
                                <asp:ListItem title="AU" Value="2077456">Australia</asp:ListItem>
                                <asp:ListItem title="AT" Value="2782113">Austria</asp:ListItem>
                                <asp:ListItem title="AZ" Value="587116">Azerbaijan</asp:ListItem>
                                <asp:ListItem title="BS" Value="3572887">Bahamas</asp:ListItem>
                                <asp:ListItem title="BH" Value="290291">Bahrain</asp:ListItem>
                                <asp:ListItem title="BD" Value="1210997">Bangladesh</asp:ListItem>
                                <asp:ListItem title="BB" Value="3374084">Barbados</asp:ListItem>
                                <asp:ListItem title="BY" Value="630336">Belarus</asp:ListItem>
                                <asp:ListItem title="BE" Value="2802361">Belgium</asp:ListItem>
                                <asp:ListItem title="BZ" Value="3582678">Belize</asp:ListItem>
                                <asp:ListItem title="BJ" Value="2395170">Benin</asp:ListItem>
                                <asp:ListItem title="BM" Value="3573345">Bermuda</asp:ListItem>
                                <asp:ListItem title="BT" Value="1252634">Bhutan</asp:ListItem>
                                <asp:ListItem title="BO" Value="3923057">Bolivia</asp:ListItem>
                                <asp:ListItem title="BA" Value="3277605">Bosnia and Herzegovina</asp:ListItem>
                                <asp:ListItem title="BW" Value="933860">Botswana</asp:ListItem>
                                <%--<asp:ListItem title="US" Value="3371123">Bouvet Island</asp:ListItem>--%>
                                <asp:ListItem title="BR" Value="3469034">Brazil</asp:ListItem>
                                <%-- <asp:ListItem title="US" Value="1282588">British Indian Ocean Territory</asp:ListItem>--%>
                                <asp:ListItem title="BN" Value="1820814">Brunei</asp:ListItem>
                                <asp:ListItem title="BG" Value="732800">Bulgaria</asp:ListItem>
                                <asp:ListItem title="BF" Value="2361809">Burkina Faso</asp:ListItem>
                                <asp:ListItem title="BI" Value="433561">Burundi</asp:ListItem>
                                <asp:ListItem title="CA" Value="6251999">Canada</asp:ListItem>
                                <asp:ListItem title="KH" Value="1831722">Cambodia</asp:ListItem>
                                <asp:ListItem title="CM" Value="2233387">Cameroon</asp:ListItem>

                                <asp:ListItem title="" Value="3374766">Cape Verde</asp:ListItem>
                                <asp:ListItem title="KY" Value="3580718">Cayman Islands</asp:ListItem>
                                <asp:ListItem title="CF" Value="239880">Central African Republic</asp:ListItem>
                                <asp:ListItem title="TD" Value="2434508">Chad</asp:ListItem>
                                <asp:ListItem title="CL" Value="3895114">Chile</asp:ListItem>
                                <asp:ListItem title="CN" Value="1814991">China</asp:ListItem>
                                <%--<asp:ListItem title="US" Value="2078138">Christmas Island</asp:ListItem>--%>
                                <%-- <asp:ListItem title="US" Value="1547376">Cocos (Keeling) Islands</asp:ListItem>--%>
                                <asp:ListItem title="CO" Value="3686110">Colombia</asp:ListItem>
                                <asp:ListItem title="KM" Value="921929">Comoros</asp:ListItem>
                                <asp:ListItem title="CG" Value="2260494">Republic Of The Congo</asp:ListItem>
                                <asp:ListItem title="CD" Value="203312">Democratic Republic Of The Congo</asp:ListItem>
                                <asp:ListItem title="CK" Value="1899402">Cook Islands</asp:ListItem>
                                <asp:ListItem title="CR" Value="3624060">Costa Rica</asp:ListItem>
                                <asp:ListItem title="CI" Value="2287781">Cote D'Ivoire (Ivory Coast)</asp:ListItem>
                                <asp:ListItem title="HR" Value="3202326">Croatia</asp:ListItem>
                                <asp:ListItem title="CU" Value="3562981">Cuba</asp:ListItem>
                                <asp:ListItem title="CY" Value="146669">Cyprus</asp:ListItem>


                                <%-- <asp:ListItem title="US" Value="56" Czech Republic">Czech Republic</asp:ListItem>--%>
                                <asp:ListItem title="CZ" Value="3077311">Czechia</asp:ListItem>

                                <asp:ListItem title="DK" Value="2623032">Denmark</asp:ListItem>
                                <asp:ListItem title="DJ" Value="223816">Djibouti</asp:ListItem>
                                <asp:ListItem title="DM" Value="3575830">Dominica</asp:ListItem>

                                <%--  <asp:ListItem title="US" Value="61" East Timor">East Timor</asp:ListItem>--%>

                                <asp:ListItem title="EC" Value="3658394">Ecuador</asp:ListItem>
                                <asp:ListItem title="EG" Value="357994">Egypt</asp:ListItem>
                                <asp:ListItem title="SV" Value="3585968">El Salvador</asp:ListItem>
                                <asp:ListItem title="GQ" Value="2309096">Equatorial Guinea</asp:ListItem>
                                <asp:ListItem title="ER" Value="338010">Eritrea</asp:ListItem>
                                <asp:ListItem title="EE" Value="453733">Estonia</asp:ListItem>
                                <asp:ListItem title="ET" Value="337996">Ethiopia</asp:ListItem>
                                <%-- <asp:ListItem title="US" Value="69" External Territories of Australia">External Territories of Australia</asp:ListItem>--%>
                                <%--   <asp:ListItem title="US" Value="3474414">Falkland Islands</asp:ListItem>--%>
                                <asp:ListItem title="FO" Value="2622320">Faroe Islands</asp:ListItem>
                                <asp:ListItem title="FJ" Value="2205218">Fiji Islands</asp:ListItem>
                                <asp:ListItem title="FI" Value="660013">Finland</asp:ListItem>
                                <asp:ListItem title="FR" Value="3017382">France</asp:ListItem>
                                <asp:ListItem title="GF" Value="3381670">French Guiana</asp:ListItem>
                                <asp:ListItem title="PF" Value="4030656">French Polynesia</asp:ListItem>
                                <asp:ListItem title="TF" Value="1546748">French Southern Territories</asp:ListItem>
                                <asp:ListItem title="GA" Value="2400553">Gabon</asp:ListItem>
                                <asp:ListItem title="GM" Value="2413451">Gambia The</asp:ListItem>
                                <asp:ListItem title="GE" Value="614540">Georgia</asp:ListItem>
                                <asp:ListItem title="DE" Value="2921044">Germany</asp:ListItem>
                                <asp:ListItem title="GH" Value="2300660">Ghana</asp:ListItem>
                                <%-- <asp:ListItem title="US" Value="2411586">Gibraltar</asp:ListItem>--%>
                                <asp:ListItem title="GR" Value="390903">Greece</asp:ListItem>
                                <asp:ListItem title="GL" Value="3425505">Greenland</asp:ListItem>
                                <asp:ListItem title="GD" Value="3580239">Grenada</asp:ListItem>
                                <asp:ListItem title="GP" Value="3579143">Guadeloupe</asp:ListItem>
                                <asp:ListItem title="GU" Value="4043988">Guam</asp:ListItem>
                                <asp:ListItem title="GT" Value="3595528">Guatemala</asp:ListItem>
                                <asp:ListItem title="GG" Value="3042362">Guernsey and Alderney</asp:ListItem>
                                <asp:ListItem title="GN" Value="2420477">Guinea</asp:ListItem>
                                <asp:ListItem title="GW" Value="2372248">Guinea-Bissau</asp:ListItem>
                                <asp:ListItem title="GY" Value="3378535">Guyana</asp:ListItem>
                                <asp:ListItem title="HT" Value="3723988">Haiti</asp:ListItem>
                                <%--  <asp:ListItem title="US" Value="1547314">Heard and McDonald Islands</asp:ListItem>--%>
                                <asp:ListItem title="HN" Value="3608932">Honduras</asp:ListItem>
                                <asp:ListItem title="HK" Value="1819730">Hong Kong S.A.R.</asp:ListItem>
                                <asp:ListItem title="HU" Value="719819">Hungary</asp:ListItem>
                                <asp:ListItem title="IS" Value="2629691">Iceland</asp:ListItem>
                                <asp:ListItem title="IN" Value="1269750">India</asp:ListItem>
                                <asp:ListItem title="ID" Value="1643084">Indonesia</asp:ListItem>

                                <asp:ListItem title="IR" Value="130758">Iran</asp:ListItem>
                                <asp:ListItem title="IQ" Value="99237">Iraq</asp:ListItem>
                                <asp:ListItem title="IE" Value="2963597">Ireland</asp:ListItem>
                                <asp:ListItem title="IL" Value="294640">Israel</asp:ListItem>
                                <asp:ListItem title="IT" Value="3175395">Italy</asp:ListItem>

                                <asp:ListItem title="JM" Value="3489940">Jamaica</asp:ListItem>
                                <asp:ListItem title="JP" Value="1861060">Japan</asp:ListItem>
                                <asp:ListItem title="JE" Value="3042142">Jersey</asp:ListItem>
                                <asp:ListItem title="JO" Value="248816">Jordan</asp:ListItem>
                                <asp:ListItem title="KZ" Value="1522867">Kazakhstan</asp:ListItem>
                                <asp:ListItem title="KE" Value="192950">Kenya</asp:ListItem>
                                <asp:ListItem title="KI" Value="4030945">Kiribati</asp:ListItem>
                                <asp:ListItem title="KP" Value="1873107">Korea North</asp:ListItem>
                                <asp:ListItem title="KR" Value="1835841">Korea South</asp:ListItem>
                                <asp:ListItem title="KW" Value="285570">Kuwait</asp:ListItem>
                                <asp:ListItem title="KG" Value="1527747">Kyrgyzstan</asp:ListItem>
                                <asp:ListItem title="LA" Value="1655842">Laos</asp:ListItem>

                                <asp:ListItem title="LV" Value="458258">Latvia</asp:ListItem>
                                <asp:ListItem title="LB" Value="272103">Lebanon</asp:ListItem>
                                <asp:ListItem title="LS" Value="932692">Lesotho</asp:ListItem>
                                <asp:ListItem title="LR" Value="2275384">Liberia</asp:ListItem>
                                <asp:ListItem title="LY" Value="2215636">Libya</asp:ListItem>
                                <asp:ListItem title="LI" Value="3042058">Liechtenstein</asp:ListItem>
                                <asp:ListItem title="LT" Value="597427">Lithuania</asp:ListItem>
                                <asp:ListItem title="LU" Value="2960313">Luxembourg</asp:ListItem>
                                <asp:ListItem title="MO" Value="1821275">Macao</asp:ListItem>

                                <asp:ListItem title="MG" Value="1062947">Madagascar</asp:ListItem>
                                <asp:ListItem title="MW" Value="927384">Malawi</asp:ListItem>
                                <asp:ListItem title="MY" Value="1733045">Malaysia</asp:ListItem>
                                <asp:ListItem title="MV" Value="1282028">Maldives</asp:ListItem>
                                <asp:ListItem title="ML" Value="2453866">Mali</asp:ListItem>
                                <asp:ListItem title="MT" Value="2562770">Malta</asp:ListItem>
                                <asp:ListItem title="IM" Value="3042225">Man (Isle of)</asp:ListItem>
                                <asp:ListItem title="MH" Value="2080185">Marshall Islands</asp:ListItem>
                                <asp:ListItem title="MQ" Value="3570311">Martinique</asp:ListItem>
                                <asp:ListItem title="MR" Value="2378080">Mauritania</asp:ListItem>
                                <asp:ListItem title="MU" Value="934292">Mauritius</asp:ListItem>
                                <asp:ListItem title="YT" Value="1024031">Mayotte</asp:ListItem>

                                <asp:ListItem title="FM" Value="2081918">Micronesia</asp:ListItem>
                                <asp:ListItem title="MD" Value="617790">Moldova</asp:ListItem>
                                <asp:ListItem title="MC" Value="2993457">Monaco</asp:ListItem>
                                <asp:ListItem title="MN" Value="2029969">Mongolia</asp:ListItem>
                                <asp:ListItem title="MS" Value="3578097">Montserrat</asp:ListItem>
                                <asp:ListItem title="MA" Value="2542007">Morocco</asp:ListItem>
                                <asp:ListItem title="MZ" Value="1036973">Mozambique</asp:ListItem>
                                <asp:ListItem title="MM" Value="1327865">Myanmar</asp:ListItem>
                                <asp:ListItem title="NA" Value="3355338">Namibia</asp:ListItem>
                                <asp:ListItem title="NR" Value="2110425">Nauru</asp:ListItem>
                                <asp:ListItem title="NP" Value="1282988">Nepal</asp:ListItem>

                                <asp:ListItem title="NL" Value="2750405">Netherlands</asp:ListItem>
                                <asp:ListItem title="NC" Value="2139685">New Caledonia</asp:ListItem>
                                <asp:ListItem title="NZ" Value="2186224">New Zealand</asp:ListItem>
                                <asp:ListItem title="NI" Value="3617476">Nicaragua</asp:ListItem>
                                <asp:ListItem title="NE" Value="2440476">Niger</asp:ListItem>
                                <asp:ListItem title="NG" Value="2328926">Nigeria</asp:ListItem>
                                <%--    <asp:ListItem title="US" Value="4036232">Niue</asp:ListItem>--%>
                                <%--   <asp:ListItem title="US" Value="2155115">Norfolk Island</asp:ListItem>--%>
                                <asp:ListItem title="MK" Value="718075">North Macedonia</asp:ListItem>
                                <asp:ListItem title="MP" Value="4041468">Northern Mariana Islands</asp:ListItem>

                                <asp:ListItem title="NO" Value="3144096">Norway</asp:ListItem>
                                <asp:ListItem title="OM" Value="286963">Oman</asp:ListItem>
                                <asp:ListItem title="PK" Value="1168579">Pakistan</asp:ListItem>
                                <asp:ListItem title="PW" Value="1559582">Palau</asp:ListItem>
                                <%--  <asp:ListItem title="US" Value="167" Palestinian Territory Occupied">Palestinian Territory Occupied</asp:ListItem>--%>
                                <asp:ListItem title="PA" Value="3703430">Panama</asp:ListItem>
                                <asp:ListItem title="PG" Value="2088628">Papua new Guinea</asp:ListItem>
                                <asp:ListItem title="PY" Value="3437598">Paraguay</asp:ListItem>

                                <asp:ListItem title="PE" Value="3932488">Peru</asp:ListItem>
                                <asp:ListItem title="PH" Value="1694008">Philippines</asp:ListItem>
                                <%--  <asp:ListItem title="US" Value="4030699">Pitcairn Island</asp:ListItem>--%>
                                <%-- <asp:ListItem title="US" Value="798544">Poland	</asp:ListItem>--%>
                                <asp:ListItem title="PT" Value="2264397">Portugal</asp:ListItem>
                                <asp:ListItem title="PR" Value="4566966">Puerto Rico</asp:ListItem>
                                <asp:ListItem title="QA" Value="289688">Qatar</asp:ListItem>
                                <asp:ListItem title="RE" Value="935317">Reunion</asp:ListItem>

                                <asp:ListItem title="RO" Value="798549">Romania</asp:ListItem>
                                <asp:ListItem title="RU" Value="2017370">Russia</asp:ListItem>
                                <asp:ListItem title="RW" Value="49518">Rwanda</asp:ListItem>
                                <asp:ListItem title="SH" Value="3370751">Saint Helena</asp:ListItem>
                                <asp:ListItem title="KN" Value="3575174">Saint Kitts And Nevis</asp:ListItem>
                                <asp:ListItem title="LC" Value="3576468">Saint Lucia</asp:ListItem>
                                <asp:ListItem title="PM" Value="3424932">Saint Pierre and Miquelon</asp:ListItem>
                                <asp:ListItem title="VC" Value="3577815">Saint Vincent And The Grenadines</asp:ListItem>
                                <asp:ListItem title="WS" Value="4034894">Samoa</asp:ListItem>
                                <asp:ListItem title="SM" Value="3168068">San Marino</asp:ListItem>
                                <asp:ListItem title="ST" Value="2410758">Sao Tome and Principe</asp:ListItem>
                                <asp:ListItem title="SA" Value="102358">Saudi Arabia</asp:ListItem>
                                <asp:ListItem title="SN" Value="2245662">Senegal</asp:ListItem>
                                <asp:ListItem title="RS" Value="6290252">Serbia</asp:ListItem>
                                <asp:ListItem title="SC" Value="241170">Seychelles</asp:ListItem>
                                <asp:ListItem title="SL" Value="2403846">Sierra Leone</asp:ListItem>
                                <%--  <asp:ListItem title="US" Value="1880251">Singapore</asp:ListItem>--%>
                                <asp:ListItem title="SK" Value="3057568">Slovakia</asp:ListItem>
                                <asp:ListItem title="SI" Value="3190538">Slovenia</asp:ListItem>
                                <%-- <asp:ListItem title="US" Value="198" Smaller Territories of the UK">Smaller Territories of the UK</asp:ListItem>--%>
                                <asp:ListItem title="SB" Value="2103350">Solomon Islands</asp:ListItem>
                                <asp:ListItem title="SO" Value="51537">Somalia</asp:ListItem>
                                <asp:ListItem title="ZA" Value="953987">South Africa</asp:ListItem>
                                <%--<asp:ListItem title="US" Value="3474415">South Georgia</asp:ListItem>--%>
                                <asp:ListItem title="SS" Value="7909807">South Sudan</asp:ListItem>
                                <%-- <asp:ListItem title="US" Value="3578476">Saint Barthélemy</asp:ListItem>--%>
                                <%--<asp:ListItem title="US" Value="3578421">Saint Martin</asp:ListItem>--%>

                                <asp:ListItem title="LK" Value="1227603">Sri Lanka</asp:ListItem>
                                <asp:ListItem title="SD" Value="366755">Sudan</asp:ListItem>
                                <asp:ListItem title="SR" Value="3382998">Suriname</asp:ListItem>
                                <%-- <asp:ListItem title="US" Value="607072">Svalbard And Jan Mayen Islands</asp:ListItem>--%>
                                <%-- <asp:ListItem title="US" Value="209" Swaziland">Swaziland</asp:ListItem>--%>
                                <asp:ListItem title="SE" Value="2661886">Sweden</asp:ListItem>

                                <asp:ListItem title="CH" Value="2658434">Switzerland</asp:ListItem>
                                <asp:ListItem title="SY" Value="163843">Syria</asp:ListItem>
                                <asp:ListItem title="TW" Value="1668284">Taiwan</asp:ListItem>
                                <asp:ListItem title="TJ" Value="1220409">Tajikistan</asp:ListItem>
                                <asp:ListItem title="TZ" Value="149590">Tanzania</asp:ListItem>
                                <asp:ListItem title="TH" Value="1605651">Thailand</asp:ListItem>
                                <asp:ListItem title="TG" Value="2363686">Togo</asp:ListItem>
                                <asp:ListItem title="TK" Value="4031074">Tokelau</asp:ListItem>
                                <asp:ListItem title="TO" Value="4032283">Tonga</asp:ListItem>
                                <asp:ListItem title="TT" Value="3573591">Trinidad And Tobago</asp:ListItem>
                                <asp:ListItem title="TN" Value="2464461">Tunisia</asp:ListItem>
                                <asp:ListItem title="TR" Value="298795">Turkey</asp:ListItem>
                                <asp:ListItem title="TM" Value="1218197">Turkmenistan</asp:ListItem>
                                <%--<asp:ListItem title="US" Value="3576916">Turks And Caicos Islands</asp:ListItem>--%>
                                <asp:ListItem title="TV" Value="2110297">Tuvalu</asp:ListItem>
                                <asp:ListItem title="UG" Value="226074">Uganda</asp:ListItem>
                                <asp:ListItem title="UA" Value="690791">Ukraine</asp:ListItem>
                                <asp:ListItem title="AE" Value="290557">United Arab Emirates</asp:ListItem>

                                <%-- <asp:ListItem title="US" Value="2017370"> Russia</asp:ListItem>--%>



                                <asp:ListItem title="UM" Value="5854968">United States Minor Outlying Islands</asp:ListItem>
                                <asp:ListItem title="UY" Value="3439705">Uruguay</asp:ListItem>
                                <asp:ListItem title="UZ" Value="1512440">Uzbekistan</asp:ListItem>
                                <asp:ListItem title="VU" Value="2134431">Vanuatu</asp:ListItem>
                                <asp:ListItem title="VA" Value="3164670">Vatican City State (Holy See)</asp:ListItem>
                                <asp:ListItem title="VE" Value="3625428">Venezuela</asp:ListItem>
                                <asp:ListItem title="VN" Value="1562822">Vietnam</asp:ListItem>
                                <%--  <asp:ListItem title="US" Value="3577718">Virgin Islands (British)</asp:ListItem>--%>
                                <asp:ListItem title="VI" Value="4796775">Virgin Islands (US)</asp:ListItem>
                                <asp:ListItem title="WF" Value="4034749">Wallis And Futuna Islands</asp:ListItem>
                                <%-- <asp:ListItem title="US" Value="2461445">Western Sahara</asp:ListItem>--%>
                                <asp:ListItem title="YE" Value="69543">Yemen</asp:ListItem>
                                <%--  <asp:ListItem title="US" Value="243" Yugoslavia">Yugoslavia</asp:ListItem>--%>
                                <asp:ListItem title="ZM" Value="895949">Zambia</asp:ListItem>
                                <asp:ListItem title="ZW" Value="878675">Zimbabwe</asp:ListItem>
                            </asp:DropDownList>
                            <asp:RequiredFieldValidator CssClass="validation" ID="RequiredFieldValidator6"
                                runat="server" ControlToValidate="ddlCountry" SetFocusOnError="true" ForeColor="Red"
                                ErrorMessage="<%$Resources:Resource, CountryRequired %>" InitialValue="-1">

                            </asp:RequiredFieldValidator>
                        </div>
                    </div>
                    <div class="" runat="server" id="creditDiv">

                        <div class="form-group">
                            <img src="Content/img/cards.png" class="img-responsive" />
                        </div>
                        <div class="form-group card_-number position-relative">
                            <asp:Label ID="Label20" class="bmd-label-floating hide1" Text="<%$Resources:Resource, CreditCardNumber %>" runat="server" />
                            <asp:TextBox ID="txtCardNumber" onfocus='setCard(this);' onInput="showCurrentValue(event)"
                                TextMode="Phone" inputmode="numeric" class="form-control" runat="server"
                                onblur="GetCardType(this)"></asp:TextBox>
                            <span class="form-clear d-none card-cancel"><span class="crossSign"><i class="fa fa-plus-circle"></i></span></span>
                            <img src="" width="25" id="myImage" class="cardimage" />
                            <asp:RequiredFieldValidator CssClass="validation" ID="RequiredFieldValidator8" runat="server"
                                ControlToValidate="txtCardNumber" SetFocusOnError="true" ForeColor="Red"
                                ErrorMessage="<%$Resources:Resource, CardNumberRequired %>"></asp:RequiredFieldValidator>
                        </div>

                        <div class="form-group-row">
                            <div class="form-group row__half position-relative">
                                <asp:Label ID="Label21" class="bmd-label-floating hide1" Text="<%$Resources:Resource, NameonCard %>" runat="server" />
                                <asp:TextBox ID="txtNameOnCard" class="form-control" runat="server"></asp:TextBox>
                                <span class="form-clear d-none"><span class="crossSign"><i class="fa fa-plus-circle"></i></span></span>
                                <asp:RequiredFieldValidator CssClass="validation" ID="RequiredFieldValidator9" runat="server" ControlToValidate="txtNameOnCard" SetFocusOnError="true" ForeColor="Red" ErrorMessage="<%$Resources:Resource, CardNameRquired %>"></asp:RequiredFieldValidator>

                            </div>
                            <div class="form-group row__half">
                                <asp:Label ID="Label25" class="bmd-label-floating" Text="<%$Resources:Resource, MonthofExpiration %>" runat="server" />
                                <asp:DropDownList ID="ddlMonth" runat="server">
                                    <asp:ListItem Value="01">01</asp:ListItem>
                                    <asp:ListItem Value="02">02</asp:ListItem>
                                    <asp:ListItem Value="03">03</asp:ListItem>
                                    <asp:ListItem Value="04">04</asp:ListItem>
                                    <asp:ListItem Value="05">05</asp:ListItem>
                                    <asp:ListItem Value="06">06</asp:ListItem>
                                    <asp:ListItem Value="07">07</asp:ListItem>
                                    <asp:ListItem Value="08">08</asp:ListItem>
                                    <asp:ListItem Value="09">09</asp:ListItem>
                                    <asp:ListItem Value="10">10</asp:ListItem>
                                    <asp:ListItem Value="11">11</asp:ListItem>
                                    <asp:ListItem Value="12">12</asp:ListItem>
                                </asp:DropDownList>

                            </div>
                            <div class="form-group row__half">

                                <asp:Label ID="Label26" class="bmd-label-floating" Text="<%$Resources:Resource, YearofExpiration %>" runat="server" />
                                <asp:DropDownList ID="ddlExpirationYear" runat="server">
                                </asp:DropDownList>
                            </div>
                            <div class="form-group row__half CSC_row position-relative">
                                <%-- <label for="exampleInputPassword1" class="bmd-label-floating">CSC</label>
                                <input type="number" class="form-control" />--%>

                                <label class="bmd-label-floating">CVV</label>
                                <asp:TextBox ID="txtCVV" class="form-control" runat="server" TextMode="Number"></asp:TextBox>
                                <span class="form-clear d-none"><span class="crossSign"><i class="fa fa-plus-circle"></i></span></span>
                                <asp:RequiredFieldValidator CssClass="validation" ID="RequiredFieldValidator10" runat="server" SetFocusOnError="true" ControlToValidate="txtCVV" ForeColor="Red" ErrorMessage="<%$Resources:Resource, CVVRequired %>"></asp:RequiredFieldValidator>

                            </div>
                        </div>
                    </div>
                    <div class="form-group-row">
                        <div class="form-group row__half position-relative">
                            <asp:Label ID="Label24" class="bmd-label-floating" Text="<%$Resources:Resource, FirstName %>" runat="server" />
                            <asp:TextBox ID="txtName" runat="server"></asp:TextBox>
                            <span class="form-clear d-none"><span class="crossSign"><i class="fa fa-plus-circle"></i></span></span>
                            <asp:RequiredFieldValidator CssClass="validation" ID="RequiredFieldValidator2" runat="server" SetFocusOnError="true" ControlToValidate="txtName" ForeColor="Red" ErrorMessage="<%$Resources:Resource, NameRequired %>"></asp:RequiredFieldValidator>
                        </div>
                        <div class="form-group row__half position-relative">
                            <asp:Label ID="Label27" class="bmd-label-floating" Text="<%$Resources:Resource, LastName %>" runat="server" />
                            <asp:TextBox ID="txtLastName" runat="server"></asp:TextBox>
                            <span class="form-clear d-none"><span class="crossSign"><i class="fa fa-plus-circle"></i></span></span>

                        </div>
                        <div class="form-group position-relative">
                            <asp:Label ID="Label28" class="bmd-label-floating" Text="<%$Resources:Resource, Email %>" runat="server" />
                            <asp:TextBox ID="txtEmail" TextMode="Email" runat="server"></asp:TextBox>
                            <span class="form-clear d-none"><span class="crossSign"><i class="fa fa-plus-circle"></i></span></span>
                            <asp:RequiredFieldValidator CssClass="validation" ID="RequiredFieldValidator4" runat="server" SetFocusOnError="true" ControlToValidate="txtEmail" ForeColor="Red" ErrorMessage="<%$Resources:Resource, EmailRequired %>"></asp:RequiredFieldValidator>
                        </div>
                        <%--<div class="form-group row__half position-relative">
                            <asp:Label ID="Label11" class="bmd-label-floating" Text="<%$Resources:Resource, PhoneNumber %>" runat="server" />
                            <asp:TextBox ID="txtPhone" TextMode="Phone" runat="server"></asp:TextBox>
							 <span class="form-clear d-none"><span class="crossSign"><i class="fa fa-plus-circle"></i></span></span>
                            <asp:RequiredFieldValidator CssClass="validation" ID="RequiredFieldValidator13" runat="server" SetFocusOnError="true" ControlToValidate="txtPhone" ForeColor="Red" ErrorMessage="<%$Resources:Resource, PhoneRequired %>"></asp:RequiredFieldValidator>
                        </div>--%>
                    </div>


                </div>
                <div id="billingAddress" runat="server" class="billing-address_info">
                    <h2>
                        <asp:Label ID="Label12" Text="<%$Resources:Resource, BILLING %>" runat="server" /></h2>
                    <div class="form-group position-relative">
                        <asp:Label ID="Label14" class="bmd-label-floating" Text="<%$Resources:Resource, Address %>" runat="server" />
                        <asp:TextBox ID="txtAddress" class="form-control" runat="server"></asp:TextBox>
                        <span class="form-clear d-none"><span class="crossSign"><i class="fa fa-plus-circle"></i></span></span>
                        <asp:RequiredFieldValidator CssClass="validation" ID="RequiredFieldValidator3" runat="server" SetFocusOnError="true" ControlToValidate="txtAddress" ForeColor="Red" ErrorMessage="<%$Resources:Resource, AddressRequired %>"></asp:RequiredFieldValidator>
                    </div>
                    <div class="form-group position-relative">
                        <asp:Label ID="Label30" class="bmd-label-floating" Text="<%$Resources:Resource, Apartment %>" runat="server" />
                        <asp:TextBox ID="txtApartment" class="form-control" runat="server"></asp:TextBox>
                        <span class="form-clear d-none"><span class="crossSign"><i class="fa fa-plus-circle"></i></span></span>
                        <%--<asp:RequiredFieldValidator CssClass="validation" ID="RequiredFieldValidator1" runat="server" SetFocusOnError="true" ControlToValidate="txtApartment" ForeColor="Red" ErrorMessage="<%$Resources:Resource, ApartmentRequired %>"></asp:RequiredFieldValidator>--%>
                    </div>
                    <div class="form-group position-relative">
                        <asp:Label ID="Label29" class="bmd-label-floating" Text="<%$Resources:Resource, City %>" runat="server" />
                        <asp:TextBox ID="txtCity" class="form-control" runat="server"></asp:TextBox>
                        <span class="form-clear d-none"><span class="crossSign"><i class="fa fa-plus-circle"></i></span></span>
                        <asp:RequiredFieldValidator CssClass="validation" ID="RequiredFieldValidator11" runat="server" SetFocusOnError="true" ControlToValidate="txtCity" ForeColor="Red" ErrorMessage="<%$Resources:Resource, CityRequired %>"></asp:RequiredFieldValidator>
                    </div>
                    <div class="form-group">
                        <%-- <div class="form-group row__half">--%>
                        <asp:Label ID="Label2" class="bmd-label-floating" Text="<%$Resources:Resource, State %>" runat="server" />

                        <asp:DropDownList class="form-control" runat="server" ID="ddlState" CssClass="txtCustomerState">
                            <asp:ListItem Value="-1" Text="Select State"></asp:ListItem>
                        </asp:DropDownList>
                        <asp:RequiredFieldValidator CssClass="validation" ID="RequiredFieldValidator7" runat="server" ControlToValidate="ddlState" SetFocusOnError="true" ForeColor="Red" ErrorMessage="<%$Resources:Resource, StateRequired %>" InitialValue="-1"></asp:RequiredFieldValidator>

                        <%-- </div>--%>
                        <%-- <div class="form-group row__half position-relative">
                                    <asp:Label ID="Label31" class="bmd-label-floating" Text="<%$Resources:Resource, PostalCode %>" runat="server" />
                                    <asp:TextBox ID="txtPostalCode" class="form-control" TextMode="Number" runat="server"></asp:TextBox>
                                    <span class="form-clear d-none"><span class="material-icons">X</span></span>
                                    <asp:RequiredFieldValidator CssClass="validation" ID="RequiredFieldValidator12" runat="server" ControlToValidate="txtPostalCode" SetFocusOnError="true" ForeColor="Red" ErrorMessage="<%$Resources:Resource, PostalRequired %>"></asp:RequiredFieldValidator>
                                </div>--%>
                    </div>
                </div>
                <%-- </ContentTemplate>
                </asp:UpdatePanel>--%>

                <div class="biling-cover">
                    <div class="biling-saprate paypalsection" runat="server" id="paypalDiv">

                        <span>
                            <asp:Button ID="btnSubmit" CssClass="btnpayment" Text="<%$Resources:Resource, Submit %>" runat="server" class="btn btn-primary" OnClick="btnSubmit_Click" />
                            <asp:Label ID="lblMessage2" runat="server"></asp:Label>
                            <div id="divPay" runat="server" class="make_payment_btn">
                                <asp:Button ID="btnPayment" runat="server" Text="<%$Resources:Resource, MakePayment %>" />
                                <asp:Label ID="Label1" runat="server"></asp:Label>
                            </div>
                        </span>
                    </div>

                    <div class="creditcardsubmit" runat="server" id="creditDiv1">
                        <span class="paymentbtn">
                            <asp:Button ID="btnCard" CssClass="btncss" runat="server" Text="<%$Resources:Resource, Submit %>" />
                        </span>
                    </div>
                </div>


                <div id="msgDialogAlert" style="font-size: 14px; display: none; text-align: center; vertical-align: central">
                    <p id="operationMsgAlert"></p>
                </div>

                <div id="MyPopup" class="modal" role="dialog">
                    <div class="modal-dialog-centered payment_popup">
                        <!-- Modal content-->
                        <div class="modal-content">
                            <div class="modal-header">
                                <button type="button" class="close" data-dismiss="modal">
                                    &times;</button>
                                <h4 class="modal-title"></h4>
                            </div>
                            <div class="modal-body justify-content-between">
                            </div>
                            <div class="modal-footer">
                                <asp:Button class="btn btn-danger" ID="btnAcceptCard" runat="server" OnClientClick="return preventMultipleSubmissions();" Text="Aceptar" OnClick="btnAcceptCard_Click"></asp:Button>
                                <asp:Button class="btn btn-danger" ID="btnAcceptPayment" runat="server" Text="Aceptar" OnClick="btnAcceptPayment_Click"></asp:Button>
                            </div>
                        </div>
                    </div>
                </div>

                <script type="text/javascript">
                    var isSubmitted = false;
                    function preventMultipleSubmissions() {
                        if (!isSubmitted) {
                            $('#<%=btnAcceptCard.ClientID %>').val('Loading...');
                            isSubmitted = true;
                            return true;
                        }
                        else {
                            return false;
                        }
                    }

                    function BindPopup() {

                        var body = "<p>";
                        body += "Antes de proceder a realizar el pago compruebe que la tarjeta de credito o debito que utilizara en la compra tenga relieve. Solo el titular de la tarjeta podra retirar los tickets correspondientes a su orden. Debera presentar la tarjeta de credito o debito con relieve fisicamente, documento de identidad y confirmacion impresa (Indispensable)";
                        body += "NO ACEPTAMOS TARJETAS SIN RELIEVE.";

                        body += "Top of Form";
                        body += "TERMINOS Y CONDICIONES MUSIKA TICKETS";
                        body += "Musika es una marca registrada de Musika SRL, proveedor lider de servicios de procesamiento de transacciones, boletería y pago de facturas. ";

                        body += "EL USUARIO ACEPTA VOLUNTARIAMENTE LOS SIGUIENTES TERMINOS Y CONDICIONES: ";

                        body += "El usuario reconoce que Musika no es el Organizador ni el Responsable del Evento y por lo tanto desiste y renuncia irrevocablemente, desde ahora y para siempre y sin reservas de ningun tipo, a demandas, reclamaciones, o instancias que pudiere tener en contra de Musika, sus accionistas, directores, representantes o empleados, empresas subsidiarias o afiliadas, que tengan su fundamento o que esten relacionadas directa o indirectamente con el evento, de cualquier tipo de naturaleza. ";

                        body += "Todas las ventas procesadas a traves de Musika son definitivas y registradas como venta final. No se aceptan devoluciones o cambios luego de procesado la venta. ";

                        body += "El usuario reconoce y acepta que cada boleto contiene una numeracion o codigo unico o individual que dara derecho a un solo acceso por boleto, por lo que el titular sera el unico responsable de salvaguardar la confidencialidad del codigo contenido en el mismo. El usuario acepta el riesgo de falsificacion, copia, manipulaciones o duplicidad en cuyo caso solo la primera persona en presentar el boleto o codigo tendra acceso al evento. Un boleto o codigo equivaldra siempre a una sola entrada, sujeto a las condiciones del evento. ";

                        body += "Para el retiro de boletas procesadas via www.musikaapp.com o enlinea, el cliente debe presentar la tarjeta de credito o debito utilizada en su compra, y una identifiacion oficial. unicamente el propietario de la tarjeta puede retirar los boletos. No se aceptan tarjetas de credito y debito sin relieve. ";

                        body += "Musika no asume responsabilidad alguna sobre la organizacion del evento, su postergacion o cancelacion, la cual es asumida directamente por el usuario, quien a su vez asume todo riesgo y peligro derivado del mismo. En caso de lluvia, fallas tecnicas, enfermedad o cualquier otra causa de fuerza mayor, el evento podra ser pospuesto y si sucediera dicha postergacion el ticket mantendra su validez hasta la fijacion de una nueva fecha, la cual sera aceptada por el usuario quien renuncia toda reclamacion por dichos cambios, reconociendo que la postergacion no dara derecho de reembolso. En el caso de que el organizador o responsable del evento anule o cancele el evento, sera responsabilidad exclusiva del mismo la devolucion total o parcial del pago recibido, segun proceda. ";

                        body += "En ningun caso seran reembolsados los eventuales gastos o cargos por servicios de boleteria de Musika, si algunos. ";

                        body += "El organizador se reserva el derecho de admision al evento, reservandose el derecho de permitir o no el ingreso de cualquier tipo de bebidas, comidas, camaras fotograficas, celulares, filmadoras y/o grabadoras. Esta prohibido ingresar todo tipo de armas de fuego y/o piezas punzantes o que representen peligro, drogas, farmacos, metales, maderas, rollos de papel, fosforos o encendedores, material pirotecnico, punteros laser, envases de vidrio o plastico. ";

                        body += "BAJO RESERVAS DE DERECHOS DE MUSIKA";

                        $("#MyPopup .modal-title").html("TERMINOS Y CONDICIONES MUSIKA TICKETS");

                        body += "</p>";
                        $("#MyPopup .modal-body").html(body);

                    }
                </script>
                <!--  input up restrict js -->
                <script>
                    var ispaypal = false;

                    $(document).ready(function (e) {

                        $("#btnPayment").click(function () {
                            if (Page_ClientValidate()) {
                                ispaypal = true;
                                $('#MyPopup').modal('show');
                                $("#btnAcceptCard").css('display', 'none');
                                $("#btnAcceptPayment").css('display', 'block');
                            }
                            return false;
                        });
                        $("#btnSubmit").click(function () {
                            if ($('#ddlState option:selected').val() == "-1") {
                                var val = document.getElementById('RequiredFieldValidator7');
                                ValidatorEnable(val, true);
                                return false;
                            }
                        });
                        $("#btnCard").click(function () {
                            debugger;
                            if ($('#ddlState option:selected').val() == "-1") {
                                var val = document.getElementById('RequiredFieldValidator7');
                                ValidatorEnable(val, true);
                                return false;
                            }
                            if (Page_ClientValidate()) {

                                ispaypal = false;
                                $('#MyPopup').modal('show');
                                $("#btnAcceptPayment").css('display', 'none');
                                $("#btnAcceptCard").css('display', 'block');
                            }


                            return false;
                        });

                        $(".card-cancel").click(function () {
                            $('#<%=hdnCardNumber.ClientID %>').val("");

                            var img = document.getElementById('myImage');
                            img.style.visibility = 'hidden';
                        });
                        $('input:text').focus(function () {
                            $('span.crossSign').css({ 'color': 'red!important' });
                        });

                        //$("#txtCardNumber").keyup(function () { $('#<%=hdnCardNumber.ClientID %>').val(number.value); });
                    });

                    function bootstrapClearButton() {
                        $('.position-relative :input').on('keydown focus', function () {
                            if ($(this).val().length > 0) {
                                $(this).nextAll('.form-clear').removeClass('d-none');
                            }
                        }).on('keydown keyup blur', function () {
                            if ($(this).val().length === 0) {
                                $(this).nextAll('.form-clear').addClass('d-none');
                            }
                        });
                        $('.form-clear').on('click', function () {
                            $(this).addClass('d-none').prevAll(':input').val('');
                        });
                    }

                    // Init the script on the pages you need
                    bootstrapClearButton();
                    function showCurrentValue(event) {
                        console.log(event.target.value);
                        const value = event.target.value;
                        $('#<%=hdnCardNumber.ClientID %>').val(value);
                    }

                    function setCard(number) {
                        if ($('#<%=hdnCardNumber.ClientID %>').val() != "") {
                            document.getElementById("txtCardNumber").value = $('#<%=hdnCardNumber.ClientID %>').val();
                        }
                    }
                    function GetCardType(number) {
                        var cNo = number.value;
                        var masking = cNo;
                        document.getElementById("txtCardNumber").value = masking.replace(/\d(?=\d{4})/g, "*");
                        //$("#txtCardNumber").attr("type", "text");
                        var img = document.getElementById('myImage');
                        var re = new RegExp("^4");
                        if (cNo.match(re) != null) {
                            document.getElementById('myImage').src = "Content/img/visa.png";
                            img.style.visibility = 'show';
                            return "Visa";
                        }

                        // Mastercard 
                        // Updated for Mastercard 2017 BINs expansion
                        if (/^(5[1-5][0-9]{14}|2(22[1-9][0-9]{12}|2[3-9][0-9]{13}|[3-6][0-9]{14}|7[0-1][0-9]{13}|720[0-9]{12}))$/.test(cNo)) {
                            document.getElementById('myImage').src = "Content/img/Mastercard.png";
                            //number = number.replace(/\d(?=\d{4})/g, "*");
                            img.style.visibility = 'show';
                            return "Mastercard";
                        }

                        // AMEX
                        re = new RegExp("^3[47]");
                        if (cNo.match(re) != null) {
                            document.getElementById('myImage').src = "Content/img/American-Express.png";
                            // number = number.replace(/\d(?=\d{4})/g, "*");
                            img.style.visibility = 'show';
                            return "AMEX";
                        }

                        // Discover
                        re = new RegExp("^(6011|622(12[6-9]|1[3-9][0-9]|[2-8][0-9]{2}|9[0-1][0-9]|92[0-5]|64[4-9])|65)");
                        if (cNo.match(re) != null) {
                            document.getElementById('myImage').src = "Content/img/Discover.png";
                            // number = number.replace(/\d(?=\d{4})/g, "*");
                            img.style.visibility = 'show';
                            return "Discover";
                        }

                        // Diners
                        re = new RegExp("^36");
                        if (cNo.match(re) != null) {
                            document.getElementById('myImage').src = "Content/img/dinners_club.png";
                            // number = number.replace(/\d(?=\d{4})/g, "*");
                            img.style.visibility = 'show';
                            return "Diners";
                        }

                        // Diners - Carte Blanche
                        re = new RegExp("^30[0-5]");
                        if (cNo.match(re) != null) {
                            document.getElementById('myImage').src = "Content/img/Diners-Club-Carte-Blanche.png";
                            // number = number.replace(/\d(?=\d{4})/g, "*");
                            img.style.visibility = 'show';
                            return "Diners - Carte Blanche";
                        }

                        // JCB
                        re = new RegExp("^35(2[89]|[3-8][0-9])");
                        if (cNo.match(re) != null) {
                            document.getElementById('myImage').src = "Content/img/jcb-card.jpg";
                            // number = number.replace(/\d(?=\d{4})/g, "*");
                            img.style.visibility = 'show';
                            return "JCB";
                        }
                        // Visa Electron
                        re = new RegExp("^(4026|417500|4508|4844|491(3|7))");
                        if (cNo.match(re) != null) {
                            document.getElementById('myImage').src = "Content/img/visa-electron.png";
                            // number = number.replace(/\d(?=\d{4})/g, "*");
                            img.style.visibility = 'show';
                            return "Visa Electron";
                        }

                        document.getElementById('myImage').src = "";
                        return "";
                    }
                    function ValidateIf() {

                        var list = document.getElementById('ddlCountry');
                        var val = document.getElementById('RequiredFieldValidator7');
                        if (list.options[list.selectedIndex].value == ("6697173" ||
                            "5880801" || "3577279" || "3578476" || "3371123" || "1547376"
                            || "7626836" || "2078138" || "3577718" || "3576916" || "607072"
                            || "1880251" || "4030699" || "4036232" || "2155115" || "3578421"
                            || "1282588" || "1547314" || "3474415" || "2411586" || "3474414"
                            || "2461445")) {
                            // ValidatorEnable(val, false);
                        }
                        else {
                            //ValidatorEnable(val, true);
                        }
                    }
                    $(function () {
                        $('.form_cover_main span input').focus(function (ev) {
                            ev.preventDefault();
                        });
                    });
                    $(document).ready(function () {
                        var ua = navigator.userAgent.toLowerCase();
                        var isAndroid = ua.indexOf("android") > -1; //&& ua.indexOf("mobile");
                        if (isAndroid) {
                            $(window).keydown(function (event) {
                                if (event.keyCode == 13) {
                                    if (Page_ClientValidate()) {
                                        var tab = $('#hdnTab').val();
                                        if (tab == "Card") {
                                            $('#MyPopup').modal('show');
                                            $("#btnAcceptPayment").css('display', 'none');
                                            $("#btnAcceptCard").css('display', 'block');
                                        }
                                        else if (tab == "PayPal") {
                                            $('#MyPopup').modal('show');
                                            $("#btnAcceptCard").css('display', 'none');
                                            $("#btnAcceptPayment").css('display', 'block');
                                        }
                                    }
                                    event.preventDefault();
                                    return false;
                                }
                            });
                        }
                    });
                </script>
                <!--  input up restrict js -->

                <script type="text/javascript"> 
                    function bindData() {
                        GetCountries();
                        GetStates();
                    }
                    function AlertBox(msgtitle, message, controlToFocus, redirectSuccess) {
                        $("#msgDialogAlert").dialog({
                            autoOpen: false,
                            modal: true,

                            title: msgtitle,
                            closeOnEscape: true,
                            buttons: [{
                                text: "OK",
                                width: "75",
                                click: function () {
                                    $(this).dialog("close");
                                    if (redirectSuccess) {
                                        window.location.href = "musika://com.app.musika";
                                    }
                                    if (controlToFocus != null)
                                        controlToFocus.focus();
                                }
                            }],
                            close: function () {
                                $("#operationMsgAlert").html("");
                                if (controlToFocus != null)
                                    controlToFocus.focus();
                            },
                            show: { effect: "clip", duration: 200 }
                        });
                        $("#operationMsgAlert").html(message);
                        $("#msgDialogAlert").dialog("open");
                    };

                    function ShowMessageFree() {
                        AlertBox("Musika", "Ticket is Generated Successfully." + "         Find your QR codes for entry to the event in ‘My Plans’", null, true);
                        return false;
                    }
                    function ShowMessageUnSuccessfulFree() {
                        AlertBox("Musika", "Ticket is not Generated Successfully.", null, false);
                        return false;
                    }

                    function ShowMessagePaid() {
                        AlertBox("Musika", 'Payment Successful....' + ' Find your QR codes for entry to the event in ‘My Plans’', null, true);
                        return false;
                    }
                    function ShowUnPaidMessage() {

                        AlertBox("Musika", 'Payment Not Successful...', null, false);
                        return false;
                    }

                    function ShowUnPaidMessageAZUL(msg, field) {

                        if (msg) {
                            if (field == "exp") {
                                $("#ddlExpirationYear").focus();
                            }
                            else {
                                $("#txtCardNumber").focus();
                            }
                            AlertBox("Musika", msg, null, false);
                        }
                        else {
                            AlertBox("Musika", 'Payment Not Successful...', null, false);
                        }
                        return false;
                    }

                </script>
            </form>
        </div>
    </section>
</body>
</html>
