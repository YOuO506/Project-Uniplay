<%@ Page Title="로그인" Language="C#" MasterPageFile="~/MasterPage.Master" AutoEventWireup="true" CodeBehind="Login.aspx.cs" Inherits="UniPlayWebSite.Login" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div style="text-align: center; margin-top: 100px;">

        <h2 class="pixel-Head" style="font-size: 50px;">Account</h2>

        <div class="pixel-box">

            <table style="margin: 0 auto;">

                <tr>
                    <td class="pixel-label" style="text-align: right;">ID </td>
                    <td class="pixel-label">:</td>
                    <td>
                        <asp:TextBox ID="txtID" runat="server" CssClass="pixel-input" /></td>
                </tr>
                <tr>
                    <td class="pixel-label" style="text-align: right;">PWD </td>
                    <td class="pixel-label">:</td>
                    <td>
                        <asp:TextBox ID="txtPWD" runat="server" CssClass="pixel-input" TextMode="Password"  /></td>
                </tr>
                <tr>
                    <td colspan="3" style="text-align: center; padding-top: 10px;">
                        <asp:Button ID="btnLogin" runat="server" CssClass="pixel-button" Text="LOGIN" OnClick="btnLogin_Click" />
                    </td>
                </tr>

            </table>

            <br />
            <asp:Label ID="lblResult" runat="server" ForeColor="Red" />

        </div>

    </div>
</asp:Content>
