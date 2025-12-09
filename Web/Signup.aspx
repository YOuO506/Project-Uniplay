<%@ Page Title="회원가입" Language="C#" MasterPageFile="~/MasterPage.Master" AutoEventWireup="true" CodeBehind="Signup.aspx.cs" Inherits="UniPlayWebSite.Signup" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

    <div style="text-align: center; margin-top: 100px;">

        <h2 class="pixel-Head" style="font-size: 50px;">Sign Up</h2>

        <div class="pixel-box">
            <table style="margin: 0 auto;">
                <!-- ID -->
                <tr>
                    <td class="pixel-label" style="text-align: right;">ID </td>
                    <td class="pixel-label">:</td>
                    <td>
                        <asp:TextBox ID="txtID" runat="server" CssClass="pixel-input" />
                    </td>
                </tr>
                <!-- Password -->
                <tr>
                    <td class="pixel-label" style="text-align: right;">PWD </td>
                    <td class="pixel-label">:</td>
                    <td>
                        <asp:TextBox ID="txtPWD" runat="server" CssClass="pixel-input" TextMode="Password" />
                    </td>
                </tr>
                <!-- NickName -->
                <tr>
                    <td class="pixel-label" style="text-align: right;">Nick </td>
                    <td class="pixel-label">:</td>
                    <td>
                        <asp:TextBox ID="txtNick" runat="server" CssClass="pixel-input" />
                    </td>
                </tr>
                <!-- 가입 버튼 -->
                <tr>
                    <td colspan="3" style="text-align: center; padding-top: 10px;">
                        <asp:Button ID="btnSignup" runat="server" CssClass="pixel-button" Text="SignUp" OnClick="btnSignup_Click" />
                    </td>
                </tr>
            </table>

            <br />
            <asp:Label ID="lblResult" runat="server" ForeColor="Red" />
        </div>
    </div>

</asp:Content>
