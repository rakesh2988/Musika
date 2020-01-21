<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="TicketEventCheckout_Old2.aspx.cs" Inherits="Musika.TicketEventCheckout" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
    <link href="Content/Ticketing.css" rel="stylesheet" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/4.7.0/css/font-awesome.min.css" />

    <link type="text/css" rel="stylesheet" href="http://code.jquery.com/ui/1.10.3/themes/smoothness/jquery-ui.css" />
    <script type="text/javascript" src="http://code.jquery.com/jquery-1.8.2.js"></script>
    <script type="text/javascript" src="http://code.jquery.com/ui/1.10.3/jquery-ui.js"></script>

    <script type="text/javascript">
        function showPopup() {
            alert("OK");
            var msg = "Antes de proceder a realizar el pago compruebe que la tarjeta de credito o debito que utilizara en la compra tenga relieve. Solo el titular de la tarjeta podra retirar los tickets correspondientes a su orden. Debera presentar la tarjeta de credito o debito con relieve fisicamente, documento de identidad y confirmacion impresa (Indispensable)
            NO ACEPTAMOS TARJETAS SIN RELIEVE.

            Top of Form
            TERMINOS Y CONDICIONES UEPA TICKETS
            Musika es una marca registrada de Musika SRL, proveedor lider de servicios de procesamiento de transacciones, boleteria y pago de facturas.

            EL USUARIO ACEPTA VOLUNTARIAMENTE LOS SIGUIENTES TERMINOS Y CONDICIONES:

            El usuario reconoce que Musika no es el Organizador ni el Responsable del Evento y por lo tanto desiste y renuncia irrevocablemente, desde ahora y para siempre y sin reservas de ningun tipo, a demandas, reclamaciones, o instancias que pudiere tener en contra de Musika, sus accionistas, directores, representantes o empleados, empresas subsidiarias o afiliadas, que tengan su fundamento o que estén relacionadas directa o indirectamente con el evento, de cualquier tipo de naturaleza.

            Todas las ventas procesadas a traves de Musika son definitivas y registradas como venta final.No se aceptan devoluciones o cambios luego de procesado la venta.

            El usuario reconoce y acepta que cada boleto contiene una numeracion o codigo unico o individual que dara derecho a un solo acceso por boleto, por lo que el titular será el único responsable de salvaguardar la confidencialidad del codigo contenido en el mismo.El usuario acepta el riesgo de falsificacion, copia, manipulaciones o duplicidad en cuyo caso solo la primera persona en presentar el boleto o codigo tendra acceso al evento.Un boleto o codigo equivaldra siempre a una sola entrada, sujeto a las condiciones del evento.

            Para el retiro de boletas procesadas via www.MusikaApp.com o enlinea, el cliente debe presentar la tarjeta de credito o debito utilizada en su compra, y una identifiacion oficial.unicamente el propietario de la tarjeta puede retirar los boletos.No se aceptan tarjetas de credito y debito sin relieve.

            Musika no asume responsabilidad alguna sobre la organizacion del evento, su postergacion o cancelacion, la cual es asumida directamente por el usuario, quien a su vez asume todo riesgo y peligro derivado del mismo.En caso de lluvia, fallas tecnicas, enfermedad o cualquier otra causa de fuerza mayor, el evento podria ser pospuesto y si sucediera dicha postergación el ticket mantendra su validez hasta la fijacion de una nueva fecha, la cual sera aceptada por el usuario quien renuncia toda reclamacion por dichos cambios, reconociendo que la postergacion no dara derecho de reembolso.En el caso de que el organizador o responsable del evento anule o cancele el evento, sera responsabilidad exclusiva del mismo la devolucion total o parcial del pago recibido, segun proceda.

            En ningun caso seran reembolsados los eventuales gastos o cargos por servicios de boletería de Musika, si algunos.

            El organizador se reserva el derecho de admision al evento, reservandose el derecho de permitir o no el ingreso de cualquier tipo de bebidas, comidas, camaras fotograficas, celulares, filmadoras y / o grabadoras.Esta prohibido ingresar todo tipo de armas de fuego y / o piezas punzantes o que representen peligro, drogas, farmacos, metales, maderas, rollos de papel, fosforos o encendedores, material pirotecnico, punteros laser, envases de vidrio o plastico.

            BAJO RESERVAS DE DERECHOS DE MUSIKA
            Aceptar";
            alert(msg);
        }
    </script>

    <script type="text/javascript">
        function showAndroidToast(toast) {
            Android.showToast(toast);
        }
</script>

    <script type="text/javascript">
        function ShowTermsPopup() {
            window.open("TermsNConditions.aspx", "_blank", "WIDTH=1080,HEIGHT=790,scrollbars=no, menubar=no,resizable=yes,directories=no,location=no"); 
        }
    </script>

    <script>
        $(document).ready(function () {
            $("#btnAcceptPayment").click(function () {
                alert("Button is Clicked");
                document.getElementById("btnAcceptPayment").click();
            });
        });
    </script>
</head>
<body>
    <form id="form1" class="form_cover_main" runat="server">
        <asp:ScriptManager ID="ScriptManager1" runat="server"></asp:ScriptManager>
        <div class="biling-cover">
            <div class="biling-saprate">
                <h2>Billing</h2>
                <span>
                    <label>Name</label>
                    <asp:TextBox ID="txtName" runat="server"></asp:TextBox>
                    <asp:RequiredFieldValidator ID="RequiredFieldValidator1" runat="server" ControlToValidate="txtName" ErrorMessage="Name is Required"></asp:RequiredFieldValidator>
                </span>
                <span>
                    <label>Email</label>
                    <asp:TextBox ID="txtEmail" runat="server"></asp:TextBox>
                    <asp:RequiredFieldValidator ID="RequiredFieldValidator2" runat="server" ControlToValidate="txtEmail" ErrorMessage="Email is Required"></asp:RequiredFieldValidator>
                </span>
                <span>
                    <label>Address</label>
                    <asp:TextBox ID="txtAddress" runat="server"></asp:TextBox>
                    <asp:RequiredFieldValidator ID="RequiredFieldValidator3" runat="server" ControlToValidate="txtAddress" ErrorMessage="Address is Required"></asp:RequiredFieldValidator>
                </span>
                <span>
                    <label>Country/State</label>
                    <asp:TextBox ID="txtCountryState" runat="server"></asp:TextBox>
                    <asp:RequiredFieldValidator ID="RequiredFieldValidator4" runat="server" ControlToValidate="txtCountryState" ErrorMessage="Country/State is Required"></asp:RequiredFieldValidator>
                </span>
                <span>
                    <label>City</label>
                    <asp:TextBox ID="txtCity" runat="server"></asp:TextBox>
                    <asp:RequiredFieldValidator ID="RequiredFieldValidator5" runat="server" ControlToValidate="txtCity" ErrorMessage="City is Required"></asp:RequiredFieldValidator>
                </span>
                <span>
                    <label>Postal Code</label>
                    <asp:TextBox ID="txtPostalCode" runat="server"></asp:TextBox>
                    <asp:RequiredFieldValidator ID="RequiredFieldValidator6" runat="server" ControlToValidate="txtPostalCode" ErrorMessage="Postal Code is Required"></asp:RequiredFieldValidator>
                </span>
                <span>
                    <label>Phone Number</label>
                    <asp:TextBox ID="txtPhone" runat="server" TextMode="Number"></asp:TextBox>
                    <asp:RequiredFieldValidator ID="RequiredFieldValidator7" runat="server" ControlToValidate="txtPhone" ErrorMessage="Phone is Required"></asp:RequiredFieldValidator>
                </span>

                <span>
                    <asp:Button ID="btnSubmit" CssClass="btnpayment" Text="Submit" runat="server" class="btn btn-primary" OnClick="btnSubmit_Click" />
                    <asp:Label ID="lblMessage2" runat="server"></asp:Label>
                </span>
            </div>


            <script type="text/javascript">
                function ShowPopup(title, body) {
                    $("#MyPopup .modal-title").html(title);
                    $("#MyPopup .modal-body").html(body);
                    $("#MyPopup").modal("show");
                }

                function ShowPopup2() {
                    //body += "Test Document";
                    var body = "";
                    body += "Antes de proceder a realizar el pago compruebe que la tarjeta de credito o debito que utilizara en la compra tenga relieve. Solo el titular de la tarjeta podra retirar los tickets correspondientes a su orden. Debera presentar la tarjeta de credito o debito con relieve fisicamente, documento de identidad y confirmacion impresa (Indispensable)";
                    body += "NO ACEPTAMOS TARJETAS SIN RELIEVE.";

                    body += "Top of Form";
                    body += "TERMINOS Y CONDICIONES UEPA TICKETS";
                    body += "Musika es una marca registrada de Musika SRL, proveedor lider de servicios de procesamiento de transacciones, boleteria y pago de facturas. ";

                    body += "EL USUARIO ACEPTA VOLUNTARIAMENTE LOS SIGUIENTES TERMINOS Y CONDICIONES: ";

                    body += "El usuario reconoce que Musika no es el Organizador ni el Responsable del Evento y por lo tanto desiste y renuncia irrevocablemente, desde ahora y para siempre y sin reservas de ningun tipo, a demandas, reclamaciones, o instancias que pudiere tener en contra de Musika, sus accionistas, directores, representantes o empleados, empresas subsidiarias o afiliadas, que tengan su fundamento o que estén relacionadas directa o indirectamente con el evento, de cualquier tipo de naturaleza. ";

                    body += "Todas las ventas procesadas a traves de Musika son definitivas y registradas como venta final. No se aceptan devoluciones o cambios luego de procesado la venta. ";

                    body += "El usuario reconoce y acepta que cada boleto contiene una numeracion o codigo unico o individual que dara derecho a un solo acceso por boleto, por lo que el titular será el único responsable de salvaguardar la confidencialidad del codigo contenido en el mismo. El usuario acepta el riesgo de falsificacion, copia, manipulaciones o duplicidad en cuyo caso solo la primera persona en presentar el boleto o codigo tendra acceso al evento. Un boleto o codigo equivaldra siempre a una sola entrada, sujeto a las condiciones del evento. ";

                    body += "Para el retiro de boletas procesadas via www.MusikaApp.com o enlinea, el cliente debe presentar la tarjeta de credito o debito utilizada en su compra, y una identifiacion oficial. unicamente el propietario de la tarjeta puede retirar los boletos. No se aceptan tarjetas de credito y debito sin relieve. ";

                    body += "Musika no asume responsabilidad alguna sobre la organizacion del evento, su postergacion o cancelacion, la cual es asumida directamente por el usuario, quien a su vez asume todo riesgo y peligro derivado del mismo. En caso de lluvia, fallas tecnicas, enfermedad o cualquier otra causa de fuerza mayor, el evento podria ser pospuesto y si sucediera dicha postergación el ticket mantendra su validez hasta la fijacion de una nueva fecha, la cual sera aceptada por el usuario quien renuncia toda reclamacion por dichos cambios, reconociendo que la postergacion no dara derecho de reembolso. En el caso de que el organizador o responsable del evento anule o cancele el evento, sera responsabilidad exclusiva del mismo la devolucion total o parcial del pago recibido, segun proceda. ";

                    body += "En ningun caso seran reembolsados los eventuales gastos o cargos por servicios de boletería de Musika, si algunos. ";

                    body += "El organizador se reserva el derecho de admision al evento, reservandose el derecho de permitir o no el ingreso de cualquier tipo de bebidas, comidas, camaras fotograficas, celulares, filmadoras y/o grabadoras. Esta prohibido ingresar todo tipo de armas de fuego y/o piezas punzantes o que representen peligro, drogas, farmacos, metales, maderas, rollos de papel, fosforos o encendedores, material pirotecnico, punteros laser, envases de vidrio o plastico. ";

                    body += "BAJO RESERVAS DE DERECHOS DE MUSIKA ";
                    //body += "Aceptar";
                    $("#MyPopup .modal-title").html("TERMINOS Y CONDICIONES UEPA TICKETS");
                    $("#MyPopup .modal-body").html(body);
                    $("#MyPopup").modal("show");
                }
            </script>


            <!-- Popup Window -->
            <!-- Bootstrap -->
            <script type="text/javascript" src='https://ajax.aspnetcdn.com/ajax/jQuery/jquery-1.8.3.min.js'></script>
            <script type="text/javascript" src='https://cdnjs.cloudflare.com/ajax/libs/twitter-bootstrap/3.0.3/js/bootstrap.min.js'></script>
            <link rel="stylesheet" href='https://cdnjs.cloudflare.com/ajax/libs/twitter-bootstrap/3.0.3/css/bootstrap.min.css'
                media="screen" />
            <!-- Bootstrap -->
            <!-- Modal Popup -->
            <div id="MyPopup" class="modal fade" role="dialog">
                <div class="modal-dialog">
                    <!-- Modal content-->
                    <div class="modal-content">
                        <div class="modal-header">
                            <button type="button" class="close" data-dismiss="modal">
                                &times;</button>
                            <h4 class="modal-title"></h4>
                        </div>
                        <div class="modal-body">
                        </div>
                        <div class="modal-footer">
                            <button type="button" class="btn btn-danger" data-dismiss="modal" id="btnAcceptPayment">
                                Aceptar</button>
                        </div>
                    </div>
                </div>
            </div>
            <!-- Modal Popup -->
            <!-- End Popup Window -->
        </div>

        <div id="divPay" runat="server">
            <div class="biling-cover">
                <div class="biling-saprate">
                    <h2>Card Info</h2>
                    <div class="card-cover">
                        <img src="Content/img/cards.png">
                    </div>
                    <span>
                        <label>Card Number</label>
                        <asp:TextBox ID="txtCardNumber" runat="server"></asp:TextBox>
                        <asp:RequiredFieldValidator ID="RequiredFieldValidator8" runat="server" ControlToValidate="txtCardNumber" ErrorMessage="Card Number is Required"></asp:RequiredFieldValidator>
                    </span>
                    <span>
                        <label>Name on Card</label>
                        <asp:TextBox ID="txtNameOnCard" runat="server"></asp:TextBox>
                        <asp:RequiredFieldValidator ID="RequiredFieldValidator9" runat="server" ControlToValidate="txtNameOnCard" ErrorMessage="Name is Required"></asp:RequiredFieldValidator>
                    </span>

                    <label class="epiry-dates">
                        <span>
                            <label>Expiry Month</label>
                            <asp:DropDownList ID="drpExpiryMonth" runat="server" AutoPostBack="true">
                                <asp:ListItem>Jan</asp:ListItem>
                                <asp:ListItem>Feb</asp:ListItem>
                                <asp:ListItem>March</asp:ListItem>
                                <asp:ListItem>April</asp:ListItem>
                                <asp:ListItem>May</asp:ListItem>
                                <asp:ListItem>June</asp:ListItem>
                                <asp:ListItem>July</asp:ListItem>
                                <asp:ListItem>Aug</asp:ListItem>
                                <asp:ListItem>Sep</asp:ListItem>
                                <asp:ListItem>Oct</asp:ListItem>
                                <asp:ListItem>Nov</asp:ListItem>
                                <asp:ListItem>Dec</asp:ListItem>
                            </asp:DropDownList>
                        </span>
                    </label>

                    <label class="epiry-dates">
                        <span>
                            <label>Expiry Date</label>
                            <asp:DropDownList ID="drpExpiryDate" runat="server" AutoPostBack="true">
                                <asp:ListItem>2018</asp:ListItem>
                                <asp:ListItem>2019</asp:ListItem>
                                <asp:ListItem>2020</asp:ListItem>
                                <asp:ListItem>2021</asp:ListItem>
                                <asp:ListItem>2022</asp:ListItem>
                                <asp:ListItem>2023</asp:ListItem>
                                <asp:ListItem>2024</asp:ListItem>
                                <asp:ListItem>2025</asp:ListItem>
                                <asp:ListItem>2026</asp:ListItem>
                                <asp:ListItem>2027</asp:ListItem>
                                <asp:ListItem>2028</asp:ListItem>
                                <asp:ListItem>2029</asp:ListItem>
                            </asp:DropDownList>
                        </span>
                    </label>

                    <span class="cvv-code">
                        <label>CVV</label>
                        <asp:TextBox ID="txtCVV" runat="server" TextMode="Number"></asp:TextBox>
                        <i class="fa fa-credit-card-alt" aria-hidden="true"></i>
                        <asp:RequiredFieldValidator ID="RequiredFieldValidator10" runat="server" ControlToValidate="txtCVV" ErrorMessage="CVV is Required"></asp:RequiredFieldValidator>
                    </span>
                    <span class="cvv-code">
                        <label>Cost</label>
                        <asp:TextBox ID="txtTicketCost" runat="server" TextMode="Number" ReadOnly="true"></asp:TextBox>
                    </span>
                    <span class="cvv-code" style="width: 400px;">
                        <asp:CheckBox ID="chkAccept" runat="server" AutoPostBack="true" OnCheckedChanged="chkAccept_CheckedChanged" />
                        <%--<a href="#" onclick="return showPopup();">Accept Terms and Condition</a>--%>
                        <asp:LinkButton ID="lnkButton" runat="server" Text="Accept Terms and Condition" OnClick="lnkButton_Click"></asp:LinkButton>
                    </span>
                </div>
            </div>
            <div class="make_payment_btn">
                <asp:Button ID="btnpayment" CssClass="btnpayment" OnClientClick="ShowPopup2();" Text="Make Payment" runat="server" OnClick="btnpayment_Click" Visible="false" />
                <asp:Button ID="Button1" CssClass="btnpayment2" OnClientClick="ShowTermsPopup();" Text="Make Payment" runat="server" />
                <asp:Label ID="lblmessage" runat="server"></asp:Label>
            </div>
        </div>
    </form>
</body>
</html>
