# DefenceGame 진행 상황

## 📅 마지막 업데이트
- **날짜**: 2026-03-10
- **Git Commit**: `6f9bb63` - Setup Unity UI for DefenceGame

---

## ✅ 완료된 작업 (Completed)

### Phase 1: 코어 시스템 (완료)
| # | 작업 | 설명 | 상태 |
|---|------|------|------|
| 1 | **Framework DI/EventBus** | BallShotGame 프레임워크 통합 | ✅ 완료 |
| 2 | **Excel 데이터 시스템** | ExcelDataReader, 데이터 클래스, 자동 변환 | ✅ 완료 |
| 3 | **GridSystem** | Tilemap 연결, Wall/Barrier 감지 | ✅ 완료 |
| 4 | **A* Pathfinder** | 랜덤 경로, 웨이포인트 시스템, 방문한 곳 재방문 금지, 뒤로가기 방지 | ✅ 완료 |
| 5 | **PathAgent** | 경로 따라 이동, 스프라이트 방향 전환 | ✅ 완료 |
| 6 | **GameManager** | Castle HP, 생존 시간, 점수, 골드 시스템 | ✅ 완료 |
| 7 | **WaveManager** | Excel 기반 웨이브, 다양한 Enemy 스폰 | ✅ 완료 |
| 8 | **Enemy 시스템** | 랜덤 경로 이동, Castle 도달, 처치 보상 | ✅ 완료 |
| 9 | **GachaManager** | 50골드 가챠, Excel 확률 연동 | ✅ 완료 |
| 10 | **UnitPlacementManager** | SpaceBar → 자동 Wall 타일 배치 | ✅ 완료 |
| 11 | **Unit 기본 구조** | Unit 클래스, 데이터 연동 | ✅ 완료 |
| 12 | **Unit 공격 시스템** | 타겟 찾기, 근접/원거리 공격 | ✅ 완료 |
| 13 | **Bullet 시스템** | 투사체 발사, 적 추적(유도), 스프라이트 방향 보정 | ✅ 완료 |
| 14 | **UI 시스템** | GameUI 스크립트 구현, Unity UI 설정 (Canvas, Text, Panel), 게임 오버 연동 | ✅ 완료 |

---

## ✅ Phase A 완료 (UI 연동 및 마무리)
| # | 작업 | 설명 | 예상 시간 |
|---|------|------|-----------|
| 18 | **통합 테스트** | 전체 게임 플레이 테스트 | 20분 |
| 19 | **엑셀 밸런스** | 데미지/체력/골드 수치 조정 | 15분 |
| 20 | **버그 수정** | 발견된 문제 해결 | 20분 |

---

## 🎮 조작법

| 키 | 기능 |
|----|------|
| **SpaceBar** | 가챠 실행 (50골드 소모) → 유닛 Wall 타일에 자동 배치 |

---

## 📁 파일 구조

### 핵심 시스템 (Scripts/DefenceGame/Core/)
```
Core/
├── GridSystem.cs           # Grid, Wall/Barrier 감지
├── Pathfinder.cs           # A* 경로 탐색, 랜덤 웨이포인트
├── PathAgent.cs            # 경로 따라 이동
├── GameManager.cs          # Castle HP, Score, Gold, 게임 상태
├── WaveManager.cs          # 웨이브 관리, 적 스폰
├── Enemy.cs                # 적 이동, 체력, 데미지
├── GachaManager.cs         # 50골드 가챠 시스템
├── UnitPlacementManager.cs # 유닛 자동 배치
├── Unit.cs                 # 유닛 공격, 타겟팅
└── Bullet.cs               # 투사체 시스템
```

### UI (Scripts/DefenceGame/UI/)
```
UI/
└── GameUI.cs               # UI 업데이트, 게임 오버 화면
```

### 데이터 (Scripts/DefenceGame/Data/)
```
Data/
├── GameDataSO.cs           # ScriptableObject 정의
├── EnemyData.cs            # 적 데이터 구조
├── TowerData.cs            # 타워 데이터 구조
├── WaveData.cs             # 웨이브 데이터 구조
├── GachaData.cs            # 가챠 확률 데이터
├── GradeType.cs            # 등급 Enum
└── Editor/
    ├── ExcelConverter.cs       # Excel → ScriptableObject 변환
    ├── ExcelSampleGenerator.cs # 샘플 엑셀 생성
    ├── GameDataEditorMenu.cs   # Unity 메뉴
    └── GameDataSettingsWindow.cs # 설정 창
```

---

## 🎯 핵심 기능 요약

### 1. 적 시스템
- **스폰**: WaveManager가 Excel 데이터 기반으로 적 스폰
- **이동**: Pathfinder가 랜덤 웨이포인트를 통해 Castle로 이동
- **공격**: Castle 도달 시 HP 감소
- **보상**: 처치 시 골드 획득

### 2. 유닛(타워) 시스템
- **가챠**: SpaceBar 누르면 50골드 소모 후 랜덤 유닛 획득
- **배치**: Wall 타일 위에 자동 배치
- **공격**: 범위 내 적 자동 공격 (근접/원거리)
- **투사체**: 원거리 유닛은 Bullet 발사

### 3. 게임 상태
- **Castle HP**: 20 → 0 되면 게임 오버
- **골드**: 적 처치 시 획득, 가챠에 사용
- **점수**: 생존 시간 × 10 + 처치 수 × 100
- **웨이브**: 일정 시간마다 다음 웨이브 진행

---

## 🚀 다음 작업 순서

1. **Unity Editor에서 UI 배치** (수동 작업)
   - Canvas 생성
   - TextMeshPro 텍스트 6개 배치
   - Game Over Panel 생성
   - GameUI 스크립트 연결

2. **테스트**
   - 모든 UI 값이 실시간으로 업데이트되는지 확인
   - 게임 오버 시 패널 표시 확인
   - 재시작 버튼 작동 확인

3. **밸런스 조정**
   - Excel 데이터 튜닝
   - 골드 획득량, 가챠 비용, 유닛 공격력 조정

---

## 📝 비고
- **개발 환경**: Unity 2D URP
- **의존성**: ExcelDataReader, DocumentFormat.OpenXml
- **디자인 패턴**: DI (Service Locator), Event Bus (Pub/Sub)
- **경로 탐색**: A* 알고리즘, 랜덤 웨이포인트
- **가챠 방식**: SpaceBar 누름 → 50골드 소모 → Wall 타일에 유닛 배치
- **공격 방식**: 자동 타겟팅, 근접/원거리(Bullet) 지원
- **Bullet 개선** (2026-03-10): 유도 미사일 방식으로 개선, rotationOffset 지원으로 Arrow 등 다양한 스프라이트 방향 처리
- **Pathfinding 개선** (2026-03-10): 방문한 노드 추적(visitedNodes)으로 재방문 금지, 뒤로가기 방지(타겟과의 거리 증가 시 해당 방향 제외)
