<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="PaypalResponse.aspx.cs" Inherits="Musika.PaypalResponse" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
     <title></title>
    <link href="Content/Ticketing.css" rel="stylesheet" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/4.7.0/css/font-awesome.min.css" />

    <link type="text/css" rel="stylesheet" href="http://code.jquery.com/ui/1.10.3/themes/smoothness/jquery-ui.css" />
    <script type="text/javascript" src="http://code.jquery.com/jquery-1.8.2.js"></script>
    <%--<script type="text/javascript" src="http://code.jquery.com/ui/1.10.3/jquery-ui.js"></script>--%>
    <script src="https://code.jquery.com/ui/1.12.1/jquery-ui.min.js"></script>
    <link href="https://code.jquery.com/ui/1.12.1/themes/base/jquery-ui.css" rel="stylesheet" />
    <%--<script type="text/javascript" src='https://ajax.aspnetcdn.com/ajax/jQuery/jquery-1.8.3.min.js'></script>--%>
    <script type="text/javascript" src='https://cdnjs.cloudflare.com/ajax/libs/twitter-bootstrap/3.0.3/js/bootstrap.min.js'></script>

    <link rel="stylesheet" href='https://cdnjs.cloudflare.com/ajax/libs/twitter-bootstrap/3.0.3/css/bootstrap.min.css'
        media="screen" />
      <script type="text/javascript">
        function showAndroidToast(toast) {
            Android.showToast(toast);
        }
</script>
    <style>
        .row.cover_ticketing {
            float: left;
            width: 100%;
            padding-left: 15px;
            background: rgb(161,238,255);
            background: -moz-linear-gradient(left, rgba(161,238,255,1) 0%, rgba(216,249,255,1) 51%, rgba(161,238,255,1) 100%);
            background: -webkit-linear-gradient(left, rgba(161,238,255,1) 0%,rgba(216,249,255,1) 51%,rgba(161,238,255,1) 100%);
            background: linear-gradient(to right, rgba(161,238,255,1) 0%,rgba(216,249,255,1) 51%,rgba(161,238,255,1) 100%);
            filter: progid:DXImageTransform.Microsoft.gradient( startColorstr='#a1eeff', endColorstr='#a1eeff',GradientType=1 );
            margin: 0;
        }

        .form_cover_main h2 {
            font-weight: 700;
        }

        .row.cover_ticketing h2 {
            width: 100%;
            border-bottom: 0px solid #333;
        }

        .cover_ticketing p {
            float: left;
            width: 150px;
        }

        p:empty {
            display: none;
        }

        .form_cover_main {
            padding: 0px;
        }

        .form_cover_main {
            background: #C1F5FF;
        }

            .form_cover_main span input {
                background-color: #fff !important;
                border-radius: 10px !important;
                color: #000 !important;
            }

        .btn-primary {
            color: #caf4ff !important;
            background-color: #000 !important;
            border-color: #000 !important;
            padding: 9px 16px !important;
            font-size: 16px !important;
        }

        div#divPay .biling-cover:after {
            content: none;
        }

        .modal-content {
            background-color: #caf4ff;
            border: 1px solid #caf4ff;
        }

        .btn-danger {
            color: #fff;
            background-color: #000;
            border-color: #000;
        }

        .modal-footer {
            bottom: 5% !important;
        }

        .ui-dialog {
            top: 170px !important;
            left: 0 !important;
            right: 0 !important;
            margin: 0 auto !important;
            position: fixed !important;
        }
    </style>
</head>
<body>
    <form id="form1" runat="server">
        
           <!-- Popup Window -->
            <!-- Bootstrap -->
             <div id="msgDialogAlert" style="display: none; text-align: center; vertical-align: central">
            <p id="operationMsgAlert"></p>
        </div>
        <script type="text/javascript">  
            function AlertBox(msgtitle, message, controlToFocus) {
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
                            window.location.href = "musika://com.app.musika";
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
                AlertBox("Musika", "Ticket is Generated Successfully." + "         Find your QR codes for entry to the event in ‘My Plans’", null);
                return false;
            }

            function ShowMessagePaid() {
                debugger;
                AlertBox("Musika", 'After Successful Payment....' + ' You can Find your QR codes for entry to the event in ‘My Plans’', null);
                return false;
            }
            function ShowUnPaidMessage() {
                AlertBox("Musika", 'Payment Not Successful...', null);
                return false;
            }

        </script>
    </form>
</body>
</html>
