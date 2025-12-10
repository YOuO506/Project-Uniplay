# UniPlay - Web 프로젝트

이 폴더는 UniPlay 플랫폼의 **ASP.NET WebForms 기반 웹 사이트 프로젝트 소스**를 포함합니다.  
Unity 기반 미니게임과 하나의 계정 시스템으로 연동되는 웹 서비스입니다.

---

## 🌐 웹 주요 기능

- 회원 가입 및 로그인
- 게임별 랭킹 시스템
- 웹 기반 아이템 상점
- 공지사항
- 커뮤니티 게시판 (글, 댓글, 좋아요)

웹에서의 활동 내역은 Unity 게임과 실시간으로 데이터가 연동됩니다.

---

## 🛒 상점 및 아이템 시스템

- 코인 기반 아이템 구매
- 코스튬 아이템 1회 구매
- 기능성 아이템 사용형
- 보유 아이템 인벤토리 관리
- 아이템 장착 여부 관리

---

## 📊 랭킹 시스템

- 게임별 최고 점수 기준 랭킹 제공
- 사용자별 기록 저장
- 실시간 랭킹 갱신

---

## 📁 프로젝트 구조

Web/  
├─ App_Code            : 공통 클래스 및 서버 로직  
├─ MasterPage.master  : 전체 레이아웃  
├─ Login.aspx         : 로그인  
├─ Store.aspx         : 아이템 상점  
├─ Ranking.aspx       : 랭킹  
├─ Notice.aspx        : 공지사항  
└─ Community.aspx     : 커뮤니티  

---

## 🛠 개발 환경

- OS : Windows 10  
- Framework : ASP.NET WebForms  
- IDE : Visual Studio 2022  
- Language : C#  
- Database : MSSQL Server  
- Frontend : HTML / CSS  

---

## 🔗 Unity 연동 개요

- 웹 로그인 계정과 Unity 게임 계정 통합
- 웹에서 구매한 아이템 정보 Unity 클라이언트로 전달
- 게임 플레이 결과(점수, 코인) 웹 서버 및 DB 저장
