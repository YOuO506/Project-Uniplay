<%@ Page Title="랭킹 페이지" Language="C#" MasterPageFile="~/MasterPage.Master" AutoEventWireup="true" CodeBehind="Ranking.aspx.cs" Inherits="UniPlayWebSite.Ranking" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

    <style>
        .rank-toolbar {
            display: flex;
            align-items: center;
            gap: 10px;
            margin: 8px 0 12px
        }

        .ctrl {
            height: 34px;
            padding: 0 10px
        }

        .grid {
            width: 100%;
            border-collapse: collapse
        }

            .grid th, .grid td {
                border: 1px solid rgba(255,255,255,.2);
                padding: 6px 8px
            }

            .grid th {
                background: rgba(255,255,255,.08)
            }
    </style>


    <h2>랭킹</h2>

    <!-- 게임 선택 -->
    <div class="rank-toolbar">
        <span>Game</span>
        <asp:DropDownList ID="ddlGame" runat="server" AutoPostBack="true"
            OnSelectedIndexChanged="ddlGame_SelectedIndexChanged" CssClass="ctrl" />
    </div>

    <!-- 랭킹 표 -->
    <asp:GridView ID="gvRank" runat="server" AutoGenerateColumns="False" CssClass="grid"
        EmptyDataText="랭킹 데이터가 없습니다.">
        <Columns>
            <asp:BoundField DataField="Rank" HeaderText="순위" />
            <asp:BoundField DataField="NickName" HeaderText="닉네임" />
            <asp:BoundField DataField="MaxScore" HeaderText="점수" />
            <asp:BoundField DataField="RecordTime" HeaderText="기록 시간"
                DataFormatString="{0:yyyy-MM-dd HH:mm}" />
        </Columns>
    </asp:GridView>

</asp:Content>
