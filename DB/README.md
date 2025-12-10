# UniPlay – Database (PracticeDB)

이 폴더는 UniPlay 프로젝트에서 사용하는 **MSSQL 데이터베이스 PracticeDB**와 관련된 파일들을 모아둔 디렉터리입니다.  
Unity 미니게임 클라이언트와 ASP.NET WebForms 웹 사이트가 **공통으로 사용하는 데이터 저장소**입니다.

---

## 🗄 데이터베이스 개요

- DBMS: Microsoft SQL Server (MSSQL)
- Database Name: PracticeDB
- 사용 목적
  - 회원 계정 및 권한 관리
  - 게임 정보 및 점수 기록 저장
  - 아이템 / 인벤토리 관리
  - 랭킹 시스템
  - 공지사항 및 커뮤니티 데이터 관리

---

## 🧱 주요 테이블 요약

### 1. 회원 / 권한

- **Members**
  - UserID (PK, char(16))
  - PassWd
  - NickName
  - UGrade (FK → UserGrade.UGrade)
  - Coin
- **UserGrade**
  - UGrade (PK, int)
  - GradeName

> UGrade = 1 이 관리자, UGrade = 0 이 일반 사용자.

---

### 2. 게임 / 점수

- **Games**
  - GameID (PK)
  - GameName
  - GameTypeID (FK → GameTypes.GameTypeID)
  - GameDescription
  - ReleaseDate

- **GameTypes**
  - GameTypeID (PK)
  - GameTypeName

- **GameScores**
  - UserID (PK, FK → Members.UserID)
  - GameID (PK, FK → Games.GameID)
  - MaxScore
  - RecordTime

> GameScores에는 **해당 유저의 게임별 최고 점수만 저장**되며, 기록 갱신 시 MaxScore 기준으로 업데이트함.

---

### 3. 아이템 / 인벤토리

- **Items**
  - GameID (PK, FK → Games.GameID)
  - ItemID (PK)
  - ItemName
  - ItemDescription
  - ItemTypeID (FK → ItemTypes.ItemTypeID)
  - Price
  - UploadTime
  - ImagePath

- **ItemTypes**
  - ItemTypeID (PK)
  - ItemTypeName

- **UserItems**
  - UserID (PK, FK → Members.UserID)
  - GameID (PK, FK → Games.GameID)
  - ItemID (PK, FK → Items.ItemID)
  - ItemTypeID (FK → ItemTypes.ItemTypeID)
  - PurchaseDate
  - IsEquipped (장착 여부)

> UserItems는 **(UserID, GameID, ItemID)** 복합 PK 구조로,  
> 같은 유저가 같은 게임에서 같은 아이템을 중복 구매하지 않도록 설계됨.  
> 코스튬은 1회 구매, 기능성 아이템은 사용 시 소모 로직을 게임/웹 로직에서 처리.

---

### 4. 공지 / 커뮤니티

- **Notice**
  - No (PK, identity)
  - Title
  - Contents
  - Author (FK → Members.UserID)
  - UploadTime
  - Hits

- **Community**
  - PostID (PK)
  - Title
  - Contents
  - Author (FK → Members.UserID)
  - UploadTime / UpdateTime
  - Hits
  - LikeCount
  - CommentCount
  - Category
  - IsDeleted
  - ImagePath

- **CommunityComments**
  - CommentID (PK)
  - PostID (FK → Community.PostID)
  - Author (FK → Members.UserID)
  - Contents
  - UploadTime / UpdateTime
  - ParentCommentID
  - IsDeleted

- **CommunityLikes**
  - PostID (FK → Community.PostID)
  - UserID (FK → Members.UserID)
  - LikedTime

---

## 🔧 설계 특징 정리

- 게임 점수는 **최고 점수만 저장**해 불필요한 데이터 누적 방지
- UserItems의 복합 키로 **아이템 중복 구매 방지**
- UGrade로 관리자 / 일반 사용자 권한 분리
- 커뮤니티는 게시글 + 댓글 + 좋아요 테이블로 구성해  
  추후 기능 확장 (검색, 정렬, 통계 등)에 대응 가능하게 설계

---

## 🔗 Unity / Web 연동

- Unity
  - 로그인한 UserID 기준으로 점수·코인·인벤토리 조회/갱신
- Web
  - 회원 관리, 상점, 공지, 커뮤니티, 랭킹 페이지에서 PracticeDB 직접 사용

> Unity와 Web이 **같은 PracticeDB를 공유**하므로,  
> 어느 쪽에서 데이터를 수정해도 다른 쪽에서 바로 반영되는 구조입니다.
