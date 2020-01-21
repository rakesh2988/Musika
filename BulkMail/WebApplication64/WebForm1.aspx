<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="WebForm1.aspx.cs" Inherits="WebApplication64.WebForm1" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <asp:ScriptManager ID="ScriptManager1" runat="server" />
    <form id="form1" runat="server">
    <div>
        <asp:Button ID="king" runat="server" Text="Show all the Members" OnClick="king_Click" />     
        <asp:Label ID="kinglbl" runat="server" Text="Total Members: "></asp:Label>
        <asp:Label ID="Label2" runat="server" ForeColor="Red" Font-Size="Larger"></asp:Label>
        <br />
        <br />
        <asp:Button ID="Sendbtn" runat="server" Text="Send" OnClick="Sendbtn_Click"  />
        <asp:Label ID="cnfrm" runat="server"></asp:Label>
        <asp:GridView ID="GridView1" runat="server" CellPadding="4" ForeColor="#333333" GridLines="Both">
            <AlternatingRowStyle BackColor="White" />
            <EditRowStyle BackColor="#7C6F57" />
            <FooterStyle BackColor="#1C5E55" Font-Bold="True" ForeColor="White" />
            <HeaderStyle BackColor="#1C5E55" Font-Bold="True" ForeColor="White" />
            <PagerStyle BackColor="#666666" ForeColor="White" HorizontalAlign="Center" />
            <RowStyle BackColor="#E3EAEB" />
            <SelectedRowStyle BackColor="#C5BBAF" Font-Bold="True" ForeColor="#333333" />
            <SortedAscendingCellStyle BackColor="#F8FAFA" />
            <SortedAscendingHeaderStyle BackColor="#246B61" />
            <SortedDescendingCellStyle BackColor="#D4DFE1" />
            <SortedDescendingHeaderStyle BackColor="#15524A" />
        </asp:GridView>
    </div>
    </form>
    <asp:Timer ID="Timer1" OnTick="Timer1_Tick" runat="server" Interval="3600000"></asp:Timer>
</body>
</html>
