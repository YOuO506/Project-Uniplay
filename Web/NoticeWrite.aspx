<%@ Page Title="공지사항 작성" Language="C#" MasterPageFile="~/MasterPage.Master" AutoEventWireup="true" CodeBehind="NoticeWrite.aspx.cs" Inherits="UniPlayWebSite.NoticeWrite" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

    <h2 class="pixel-Head" style="padding-left: 40px;">공지사항 작성</h2>

    <div class="page-wrapper">
        <table style="width: 100%; color: white;">
            <tr>
                <td style="width: 100px;">제목</td>
                <td>
                    <asp:TextBox ID="txtTitle" runat="server" CssClass="pixel-input" Width="90%" /></td>
            </tr>
            <tr>
                <td style="vertical-align: top;">내용</td>
                <td>
                    <asp:TextBox ID="txtContents" runat="server" CssClass="pixel-input" TextMode="MultiLine" Rows="10" Width="90%" />
                </td>
            </tr>
        </table>

        <br />
        <asp:Button ID="btnSave" runat="server" Text="저장" CssClass="pixel-button" OnClick="btnSave_Click" />

        <asp:Button ID="btnCancel" runat="server" Text="취소" CssClass="pixel-button" OnClick="btnCancel_Click" />
        
        <br />
        <br />
        
        <asp:Label ID="lblMessage" runat="server" ForeColor="Red" />

    </div>

</asp:Content>
