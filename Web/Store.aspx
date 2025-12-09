<%@ Page Title="상점" Language="C#" MasterPageFile="~/MasterPage.Master" AutoEventWireup="true" CodeBehind="Store.aspx.cs" Inherits="UniPlayWebSite.Store" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

    <style>
        /* ====== 레이아웃 ====== */
        .inv-wrap {
            display: flex;
            gap: 16px;
            width: 90%;
            margin: 20px auto;
        }

        .inv-sidebar {
            width: 140px;
            display: flex;
            flex-direction: column;
            gap: 10px;
        }

        /* 오른쪽 패널(유니티 패널 느낌) */
        .inv-panel {
            flex: 1;
            background: #7a788a; /* 내부 회색 */
            border: 2px solid #ffffff;
            border-radius: 8px;
            box-shadow: 0 0 10px #00000060;
            padding: 14px 14px 18px;
        }

        /* ====== 탭 ====== */
        /* 왼쪽 세로 탭(게임) */
        .tab-vert {
            display: block;
            padding: 10px 12px;
            text-align: left;
            background: #e8e6ef;
            color: #2c2f42;
            text-decoration: none;
            border: 2px solid #ffffff;
            border-radius: 10px;
            font-family: 'NeoDunggeunmoPro', sans-serif;
            font-size: 18px;
            box-shadow: 0 3px 0 #555;
        }

            .tab-vert:hover {
                filter: brightness(0.95);
            }

        /* 상단 가로 탭(아이템 타입) */
        .inv-top-tabs {
            display: flex;
            gap: 8px;
            margin-bottom: 12px;
        }

        .tab-horz {
            display: inline-block;
            padding: 8px 14px;
            background: #e8e6ef;
            color: #2c2f42;
            text-decoration: none;
            border: 2px solid #ffffff;
            border-radius: 10px;
            font-family: 'NeoDunggeunmoPro', sans-serif;
            font-size: 16px;
            box-shadow: 0 3px 0 #555;
        }

            .tab-horz:hover {
                filter: brightness(0.95);
            }

        /* ====== 아이템 그리드/카드 ====== */
        .inv-grid {
            display: grid;
            grid-template-columns: repeat(2, minmax(260px, 1fr));
            gap: 20px;
            padding: 6px 6px 2px;
        }

        .card {
            background: #6e6b7c;
            border: 2px solid #ffffff;
            border-radius: 8px;
            box-shadow: 0 3px 0 #444;
            padding: 14px;
            text-align: center;
        }

        .card-title {
            font-family: 'NeoDunggeunmoPro', sans-serif;
            font-size: 28px;
            color: #fff;
            margin-bottom: 8px;
        }

        .card-img {
            width: 160px;
            height: 160px;
            image-rendering: pixelated;
            margin: 0 auto 12px;
            display: block;
        }

        /* ====== 버튼 (Buy) ====== */
        .btn-buy {
            width: 90%;
            padding: 10px 0;
            border: none;
            font-family: 'DungGeunMo', sans-serif;
            font-size: 22px;
            color: #fff;
            background: #b49bb8; /* 보라빛 버튼 */
            border-radius: 10px;
            box-shadow: 0 4px 0 #4a3e50;
            cursor: pointer;
        }

            .btn-buy:hover {
                filter: brightness(1.03);
            }

            .btn-buy:active {
                transform: translateY(1px);
                box-shadow: 0 3px 0 #4a3e50;
            }
            /* 비활성(이미 보유) */
            .btn-buy[disabled], .btn-buy:disabled {
                background: #9a96a3;
                box-shadow: 0 4px 0 #56525d;
                opacity: 0.7;
                cursor: not-allowed;
            }

        .btn-add {
            width: 90%;
            padding: 10px 0;
            border: none;
            font-family: 'DungGeunMo', sans-serif;
            font-size: 22px;
            color: #fff;
            background: #ebbe71; /* 노란색 버튼 */
            border-radius: 10px;
            box-shadow: 0 4px 0 #82693f;
            cursor: pointer;
            display: block; /* 버튼을 블록요소로 */
            margin: 0 auto; /* 좌우 자동 여백 → 중앙 정렬 */
            margin-top: 10px;
        }

        /* 가격 텍스트 */
        .price {
            font-family: 'DungGeunMo', sans-serif;
            font-size: 18px;
            color: #fff;
            margin-top: 8px;
        }

        /* 공통: 링크 밑줄 제거 */
        a, .tab-vert, .tab-horz {
            text-decoration: none;
        }
    </style>

    <asp:Button ID="btnAdminItems" runat="server" CssClass="btn-add" Text="아이템 등록/삭제" OnClick="btnAdminItems_Click" />

    <!-- 전체 래핑: 유니티 패널 -->
    <div class="inv-wrap">

        <!-- 좌측: 게임 세로 탭 -->
        <aside class="inv-sidebar">

            <asp:Repeater ID="rptGames" runat="server" OnItemCommand="rptGames_ItemCommand">
                <ItemTemplate>
                    <asp:LinkButton ID="btnGame" runat="server" CssClass="tab-vert"
                        CommandName="FilterGame" CommandArgument='<%# Eval("GameID") %>'>
            <%# Eval("GameName") %>
                    </asp:LinkButton>
                </ItemTemplate>
            </asp:Repeater>

        </aside>

        <!-- 우측: 패널 -->
        <section class="inv-panel">

            <!-- 상단: 아이템 유형 가로 탭 -->
            <div class="inv-top-tabs">

                <asp:Repeater ID="rptTypes" runat="server" OnItemCommand="rptTypes_ItemCommand">
                    <ItemTemplate>
                        <asp:LinkButton ID="btnType" runat="server" CssClass="tab-horz"
                            CommandName="FilterType" CommandArgument='<%# Eval("ItemTypeID") %>'>
              <%# Eval("ItemTypeName") %>
                        </asp:LinkButton>
                    </ItemTemplate>
                </asp:Repeater>

            </div>

            <!-- 본문: 아이템 2열 그리드 -->
            <div class="inv-grid">

                <asp:Repeater ID="rptItems" runat="server" OnItemCommand="rptItems_ItemCommand" OnItemDataBound="rptItems_ItemDataBound">

                    <ItemTemplate>
                        <div class="card">
                            <div class="card-title"><%# Eval("ItemName") %></div>
                            <img class="card-img" src='<%# ResolveUrl("~/" + Eval("ImagePath")) %>' alt="" />

                            <div>
                                <asp:Button ID="btnBuy" runat="server"
                                    CssClass="btn-buy"
                                    CommandName="Buy"
                                    CommandArgument='<%# Eval("GameID") + "|" + Eval("ItemID") %>'
                                    Text="Buy"
                                    OnClientClick='<%# "return confirm(\"이 아이템을 구매하시겠습니까? 가격: " + Eval("Price") + " 코인\");" %>'
                                    Enabled='<%# !(bool)Eval("Owned") %>' />

                            </div>

                            <div class="price">
                                <img src="Images/coin.png" alt="Coin" style="width: 16px; height: 16px; vertical-align: middle; margin-right: 4px;" />
                                <%# string.Format("{0:N0}", Eval("Price")) %>
                            </div>

                        </div>
                    </ItemTemplate>

                </asp:Repeater>

            </div>

        </section>
    </div>

    <!-- 선택 상태 저장 -->
    <asp:HiddenField ID="hidGameID" runat="server" />
    <asp:HiddenField ID="hidTypeID" runat="server" />

</asp:Content>
