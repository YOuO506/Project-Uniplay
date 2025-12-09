using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.Script.Serialization;

namespace UniPlayWebSite.Api
{
    public partial class InventoryApi : System.Web.UI.Page
    {
        // DTO: Unity에 내려줄 아이템
        private class ItemDto
        {
            public int GameID { get; set; }       // 게임 ID
            public int ItemID { get; set; }       // 아이템 ID
            public string ItemName { get; set; }  // 아이템명
            public int ItemTypeID { get; set; }   // 아이템 타입
            public bool IsOwned { get; set; }     // 소유 여부
            public bool IsEquipped { get; set; }  // 현재 사용중 여부
            public string ImagePath { get; set; } // 아이템 이미지 경로 추가
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            // 응답 기본 세팅
            Response.Clear();
            Response.ContentType = "application/json";
            Response.Charset = "utf-8";

            try
            {
                // 0) 공통 파라미터 파싱 -----------------------------
                string mode = (Request["mode"] ?? "").Trim();      // 어떤 동작인지: list / equip
                string userId = (Request["userId"] ?? "").Trim();  // 사용자 ID (둘 다에서 사용)
                string gameIdRaw = (Request["gameId"] ?? "").Trim();// 게임 ID(문자열, 나중에 int로 파싱)
                // 0) DB 연결문자열
                string connStr = ConfigurationManager.ConnectionStrings["GameDB"].ConnectionString;

                // 1) 장착 변경 모드(equip) 처리 ----------------------
                //    - 버튼 "Use"를 눌렀을 때 호출됨
                //    - 같은 유저/같은 게임에서 모두 0으로 내린 뒤, 선택한 아이템만 1로 세팅
                if (string.Equals(mode, "equip", System.StringComparison.OrdinalIgnoreCase))
                {
                    // 필수 파라미터 검증: userId, gameId, itemId가 모두 있어야 함
                    if (string.IsNullOrEmpty(userId))
                    {
                        WriteError("missing_userId", "userId는 필수입니다."); // 빠지면 에러
                        return;                                              // 처리 종료
                    }
                    if (!int.TryParse(gameIdRaw, out int gameId))
                    {
                        WriteError("invalid_gameId", "gameId는 정수여야 합니다."); // 숫자 아니면 에러
                        return;
                    }
                    // equip에서는 itemId도 필요함
                    if (!int.TryParse((Request["itemId"] ?? "0").Trim(), out int itemId) || itemId <= 0)
                    {
                        WriteError("invalid_itemId", "itemId는 정수여야 합니다."); // 숫자 아니면 에러
                        return;
                    }

                    // DB 접속해서 IsEquipped 갱신
                    using (var con = new System.Data.SqlClient.SqlConnection(connStr))  // SQL 연결 객체
                    using (var cmd = new System.Data.SqlClient.SqlCommand(@"
                            -- 1) 선택한 아이템의 타입(ItemTypeID) 가져오기
                            DECLARE @TypeID INT;  
                            SELECT @TypeID = ItemTypeID
                              FROM Items
                             WHERE GameID = @G AND ItemID = @I;
                        
                            -- 2) 같은 게임 + 같은 타입에 해당하는 기존 장착 해제
                            UPDATE UI
                                SET IsEquipped = 0
                              FROM UserItems UI
                              INNER JOIN Items IT
                                 ON UI.GameID = IT.GameID AND UI.ItemID = IT.ItemID
                             WHERE UI.UserID = @U
                               AND UI.GameID = @G
                               AND IT.ItemTypeID = @TypeID;

                            -- 3) 선택한 아이템을 장착 상태로 설정
                            UPDATE UserItems
                               SET IsEquipped = 1
                             WHERE UserID = @U AND GameID = @G AND ItemID = @I;
                        ", con))  // 위의 SQL 전체를 SqlCommand로 실행
                    {
                        cmd.Parameters.AddWithValue("@U", userId); // @U = 현재 로그인한 유저 ID
                        cmd.Parameters.AddWithValue("@G", gameId); // @G = 현재 게임 ID
                        cmd.Parameters.AddWithValue("@I", itemId); // @I = 선택한 아이템 ID
                        con.Open();                                // DB 연결 열기
                        cmd.ExecuteNonQuery();                     // SQL 실행 (DECLARE + UPDATE 2개 모두 실행됨)
                    }

                    // 성공 JSON 즉시 반환 (list처럼 큰 응답 불필요)
                    Response.Write(@"{""success"":true}");        // 간단 성공 응답
                    return;                                       // equip 처리 완료, 종료
                }

                // 2) 목록 모드(list) 처리 ----------------------------
                //    - 인벤토리 패널을 띄울 때 호출됨
                if (!string.Equals(mode, "list", System.StringComparison.OrdinalIgnoreCase))
                {
                    WriteError("invalid_mode", "mode=list|equip 만 지원합니다."); // 그 외 모드는 거부
                    return;
                }
                // list에서도 userId, gameId는 필수
                if (string.IsNullOrEmpty(userId))
                {
                    WriteError("missing_userId", "userId는 필수입니다."); // 빠지면 에러
                    return;
                }
                if (!int.TryParse(gameIdRaw, out int gameIdList))
                {
                    WriteError("invalid_gameId", "gameId는 정수여야 합니다."); // 숫자 아니면 에러
                    return;
                }

                // 결과 담을 리스트 준비
                var items = new System.Collections.Generic.List<ItemDto>(); // 응답용 DTO 리스트

                // Items 기준으로 전체를 가져오되,
                // 해당 유저의 UserItems를 LEFT JOIN해서 보유 여부(IsOwned)와 장착 여부(IsEquipped)를 함께 내려줌
                string sql = @"
                    SELECT
                        I.GameID,
                        I.ItemID,
                        I.ItemName,
                        I.ItemTypeID,
                        COALESCE(NULLIF(I.ImagePath,''), 'Images/Items/item_placeholder.png') AS ImagePath, 
                        CAST(CASE WHEN UI.UserID IS NULL THEN 0 ELSE 1 END AS bit) AS IsOwned, 
                        ISNULL(UI.IsEquipped, CAST(0 AS bit)) AS IsEquipped
                    FROM dbo.Items AS I
                    LEFT JOIN dbo.UserItems AS UI
                           ON UI.GameID = I.GameID
                          AND UI.ItemID = I.ItemID
                          AND UI.UserID = @UserID
                    WHERE I.GameID = @GameID
                    ORDER BY I.ItemID ASC;";

                // DB 실행
                using (var con = new System.Data.SqlClient.SqlConnection(connStr)) // 연결
                using (var cmd = new System.Data.SqlClient.SqlCommand(sql, con))   // 커맨드
                {
                    cmd.CommandType = System.Data.CommandType.Text;        // 일반 텍스트 쿼리
                    cmd.Parameters.AddWithValue("@UserID", userId);        // 파라미터 바인딩
                    cmd.Parameters.AddWithValue("@GameID", gameIdList);    // "
                    con.Open();                                            // 연결 열기
                    using (var rd = cmd.ExecuteReader())                   // 실행해서 리더 획득
                    {
                        while (rd.Read())                                  // 한 행씩
                        {
                            // DTO로 매핑(각 컬럼 꺼내서 채움)
                            items.Add(new ItemDto
                            {
                                GameID = rd.GetInt32(rd.GetOrdinal("GameID")),
                                ItemID = rd.GetInt32(rd.GetOrdinal("ItemID")),
                                ItemName = rd.GetString(rd.GetOrdinal("ItemName")),
                                ItemTypeID = rd.GetInt32(rd.GetOrdinal("ItemTypeID")),
                                IsOwned = rd.GetBoolean(rd.GetOrdinal("IsOwned")),
                                IsEquipped = rd.GetBoolean(rd.GetOrdinal("IsEquipped")),
                                ImagePath = rd.GetString(rd.GetOrdinal("ImagePath")),
                            });
                        }
                    }
                }

                // 직렬화해서 응답
                var serializer = new System.Web.Script.Serialization.JavaScriptSerializer(); // JSON 직렬화기
                var res = new { success = true, items = items };                             // 통일된 응답 스키마
                Response.Write(serializer.Serialize(res));                                   // JSON 출력
            }
            catch (System.Exception ex)
            {
                // 예외 상황: 개발 단계에서는 메시지 내려줌(운영에선 일반 메시지 추천)
                WriteError("server_error", ex.Message);             // 에러 JSON
            }
            finally
            {
                Response.End();                                     // 응답 종료 (버퍼 플러시)
            }
        }

        // 공통 에러 응답 (항상 success=false로 내려줌)
        private void WriteError(string code, string message)
        {
            var serializer = new JavaScriptSerializer();
            var err = new { success = false, error = code, message = message };
            Response.StatusCode = 400;
            Response.Write(serializer.Serialize(err));
        }
    }
}