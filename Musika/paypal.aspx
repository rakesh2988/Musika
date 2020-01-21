<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="paypal.aspx.cs" Inherits="Musika.paypal" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    Redirecting.....
     <form id="payForm" method="post" action="<%=URL%>">
        <input type="hidden" name="cmd" value="<%=cmd%>" />
        <input type="hidden" name="business" value="<%=business%>" />
        <input type="hidden" name="item_name" value="<%=item_name%>" />
        <input type="hidden" name="amount" value="<%=amount%>" />
        <input type="hidden" name="no_shipping" value="<%=no_shipping%>" />
        <input type="hidden" name="return" value="<%=return_url%>" />
        <input type="hidden" name="rm" value="<%=rm%>" />
        <input type="hidden" name="notify_url" value="<%=notify_url%>" />
        <input type="hidden" name="cancel_return" value="<%=cancel_url%>" />
        <input type="hidden" name="currency_code" value="<%=currency_code%>" />
        <input type="hidden" name="item_number" value="<%=item_number%>" />
         <input type="hidden" name="address_city" value="<%=address_city%>" />
         <input type="hidden" name="address_state" value="<%=address_state%>" />
         <input type="hidden" name="address_zip" value="<%=address_zip%>" />
        <input type="hidden" name="custom" value="<%=request_id%>" />
        
    </form>
       <script language="javascript">
    document.forms["payForm"].submit();
    </script>
</body>
</html>
