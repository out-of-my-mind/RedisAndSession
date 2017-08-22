<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="WebForm1.aspx.cs" Inherits="Pic.WebForm1" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
    <div>
        ----------------------Redis---------------------------------- <br />

        <asp:Label ID="Label4" runat="server" Text=""></asp:Label>
        <br />
        <asp:Button ID="btn_redis_add" runat="server" Text="list写入redis" 
            onclick="Button6_Click" />

        <asp:Button ID="btn_redis_remove" runat="server" 
            onclick="btn_redis_remove_Click" Text="移出数据" />
        <br />
        <br />
        <br />
        <asp:Label ID="Label5" runat="server" Text="Label"></asp:Label>
        <br />
        <asp:Button ID="Button6" runat="server" onclick="Button6_Click1" Text="对象处理" />
        <asp:Button ID="Button7" runat="server" onclick="Button7_Click" Text="对象处理1" />
        <br />
        <br />
        <asp:Button ID="Button4" runat="server" Text="Redis" onclick="Button4_Click" />

        <br />
        <br />
        <asp:Label ID="Label6" runat="server" Text="Label"></asp:Label>
        &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
        <asp:Label ID="Label7" runat="server" Text="Label"></asp:Label>
        &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
        <asp:Label ID="Label8" runat="server" Text="Label"></asp:Label>
        <br />

        <asp:Button ID="Button8" runat="server" Text="登入" onclick="Button8_Click" />

        <asp:Button ID="Button9" runat="server" Text="取消会话" onclick="Button9_Click" />

        <asp:Button ID="Button10" runat="server" Text="获取Session个数" 
            onclick="Button10_Click" />

        <asp:Button ID="Button11" runat="server" Text="移除项" onclick="Button11_Click" />

        <asp:Button ID="Button12" runat="server" Text="获取当前SessionID" 
            onclick="Button12_Click" />

       <br /> ------------------------------------------------------------
    </div>
    </form>
</body>
</html>
