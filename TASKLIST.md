# DefenceGame 진행 상황

## 📅 마지막 업데이트
- **날짜**: 2026-03-10
- **Git Commit**: `7c6511f` - Fix ExcelConverter: Preserve existing GameData.asset on conversion

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
| 15 | **Unit 드래그/합성 시스템** | 드래그 앤 드롭, 같은 등급/이름 유닛 합성, 시각적 피드백 | ✅ 완료 |

---

## ✅ Phase A 완료 (UI 연동 및 마무리)
| # | 작업 | 설명 | 예상 시간 |
|---|------|------|-----------|
| 18 | **통합 테스트** | 전체 게임 플레이 테스트 | 20분 |
| 19 | **엑셀 밸런스** | 데미지/체력/골드 수치 조정 | 15분 |
| 20 | **버그 수정** | 발견된 문제 해결 | 20분 |

---

## 🎮 조작법

| 키/동작 | 기능 |
|----|------|
| **SpaceBar** | 가챠 실행 (50골드 소모) → 유닛 Wall 타일에 자동 배치 |
| **마우스 드래그** | 유닛을 다른 위치로 이동 또는 같은 유닛과 합성 |

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
├── Bullet.cs               # 투사체 시스템
├── UnitDragSystem.cs       # 유닛 드래그 앤 드롭
└── UnitMergeManager.cs     # 유닛 합성 시스템
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
- **드래그**: 마우스로 유닛을 드래그하여 다른 Wall 타일로 이동
- **합성**: 같은 등급 + 같은 이름 유닛 2개를 겹쳐서 다음 등급으로 합성 (Common→Uncommon→Rare→Epic→Legendary)
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
- **Pathfinding 버그 수정** (2026-03-10): visitedNodes 누적 문제 해결 (최대 20개 제한), immediateLastNode 추가로 즉시 되돌아가기 방지, forward-only 체크 완화 (tolerance 1.5f)
- **ExcelConverter 버그 수정** (2026-03-10): Excel 저장 시 GameData.asset이 지워지는 문제 해결 - 기존 asset이 있으면 데이터만 업데이트, 없으면 새로 생성
- **Unit 드래그/합성 시스템 구현** (2026-03-10): 드래그 앤 드롭, 같은 등급/이름 유닛 합성, 시각적 피드백(색상), 다음 등급 같은 이름 유닛 생성
- **Unit 시각적 피드백 개선** (2026-03-10): RareColor(그림자) 색상 변경으로 시각적 피드백 개선, 드래그 시 색상 변경
- **Unit 합성 랜덤화** (2026-03-10): 같은 유닛 25% / 다른 유닛 75% 확률로 합성 결과 다양화 (4가지 유닛 균등 확률)
- **Tower ID 정규화** (2026-03-10): 5단계 등급 시스템 지원을 위한 ID 변환 (1-20 → 1-4), 4개 프리팹으로 20개 타워 지원

---

## 🎯 Phase B: 고급 기능 구현 (진행 중)

### 📋 진행 상황
| 단계 | 작업 | 설명 | 상태 | 커밋 |
|------|------|------|------|------|
| **Step 1** | **Enemy 체력바 시스템** | Slider UI로 체력 표시 (머리 위, 흰색 BG + 녹색 Fill) | ✅ 완료 | `1cac56c` |
| **Step 2** | **Bullet 히트 이펙트** | Bullet 충돌 시 ParticleSystem 효과 생성 | ✅ 완료 | `950e165` |
| **Step 3** | **타워 레벨/경험치 시스템** | 합성 시 경험치 획득, 무제한 레벨업, 능력치 증가 | ✅ 완료 | `43cbff1` |
| **Step 4** | **타워 레벨 UI** | 화면 우측에 4개 타워 레벨/경험치 Slider 표시 | ✅ 완료 | `0baac48` |
| Step 5 | 특수 능력 해금 시스템 | 레벨 3/5/7 도달 시 특수 능력 해금 (관통/광역/버프 등) | 📝 예정 | - |

### 📋 상세 진행 내용

#### **Step 1: Enemy 체력바 시스템 + UX 개선** ✅ 완료 (2025-03-11)
- [x] HealthBar.cs 스크립트 생성 (프리팹 방식)
- [x] Enemy.cs 수정 (healthBarPrefab 필드 추가)
- [x] 체력바 위치: Enemy 머리 위 (World Space Canvas)
- [x] TakeDamage() 호출 시 Slider 값 업데이트
- [x] 프리팹 방식으로 변경 (크기 조절 용이)
- [x] **추가: 드래그 중 유닛 공격 비활성화** (`isDragging` 체크)
- [x] **추가: 마우스 호버 시 유닛 크기 10% 증가** (시각적 피드백)
- [x] **추가: 거리 기반 마우스 감지** (`hoverDetectionRadius`)

**구현 방식:**
- `HealthBar` 프리팹을 Unity Inspector에서 연결
- `World Space` Canvas 사용
- 크기: Width 0.8, Height 0.15
- 색상: 흰색 BG + 녹색 Fill

**UX 개선:**
- 드래그 중 자동 공격 중지
- 마우스 호버 시 `transform.localScale` 1.1배 증가
- `hoverDetectionRadius` (기본 0.5)로 감지 범위 조절 가능

**Git Commit:** `1cac56c` - Phase B Step 1 Complete

#### **Step 2: Bullet 히트 이펙트 + 이펙트 가려짐 버그 수정** ✅ 완료 (2025-03-11)
- [x] Bullet.cs에 `hitEffectPrefab` 필드 추가
- [x] `SpawnHitEffect()` 메서드 구현 (이펙트 생성 및 자동 삭제)
- [x] Inspector에서 ParticleSystem 프리팹 연결 가능
- [x] **버그 수정: 이펙트가 Tilemap에 가려지는 문제 해결**
  - UnitMergeManager.cs: `renderer.sortingOrder = 1000` 추가
  - Bullet.cs: `renderer.sortingOrder = 1000` 추가
- [x] 이펙트 2초 후 자동 삭제 (또는 ParticleSystem duration 기준)

**구현 방식:**
- Bullet 프리팹 Inspector에서 `Hit Effect Prefet` 필드에 ParticleSystem 연결
- 추천 이펙트: `CFXR2 Ground Hit`, `CFXR Hit Crit`, `CFXR Magic Poof`
- Sorting Order 1000으로 설정하여 항상 Tilemap 위에 표시

**Git Commit:** `950e165` - Phase B Step 2: Add Bullet hit effect and fix merge effect sorting

#### **Step 3: 타워 레벨/경험치 시스템** ✅ 완료 (2025-03-11)
- [x] TowerLevelData.cs 신규 생성 (데이터 구조)
  - 레벨, 현재 경험치, 다음 레벨 필요 경험치
  - 레벨업 공식: `100 + (currentLevel × 50)`
  - 초과 경험치 이월 처리 (연속 레벨업 지원)
- [x] TowerLevelManager.cs 신규 생성 (Singleton)
  - 4개 타워 타입 지원 (Archer, Wizard, WizardTower, Laser)
  - 등급별 경험치 획득량 설정
  - 레벨업 및 경험치 변경 이벤트 발생
- [x] UnitMergeManager.cs 수정
  - 합성 시 `TowerLevelManager.Instance.AddExp()` 호출
  - Console에 경험치 획득 로그 출력
- [x] 경험치 획득량 설정:
  - Common: +50 EXP
  - Uncommon: +100 EXP
  - Rare: +150 EXP
  - Epic: +200 EXP
  - Legendary: +0 EXP (경험치 없음)

**Unity 설정:**
- Hierarchy → Managers 오브젝트에 `TowerLevelManager` 컴포넌트 추가
- Tower Types: Archer, Wizard, WizardTower, Laser 자동 설정

**Git Commit:** `43cbff1` - Phase B Step 3: Add tower level and experience system

#### **Step 4: 타워 레벨 UI** ✅ 완료 (2025-03-11)
- [x] TowerLevelUI.cs 신규 생성
- [x] 화면 우측 상단에 4개 타워 슬롯 배치 (Archer/Wizard/WizardTower/Laser)
  - 각 슬롯에 Slider 추가 (흰색 BG, 녹색 Fill)
  - 레벨 텍스트 표시 (Lv.0~)
  - **Panel 색상 변경 기능 추가** (레벨별로 색상 변경)
- [x] 레벨별 Panel/슬롯 색상 변경:
  - Lv.0-2: 회색
  - Lv.3-4: 하얀색
  - Lv.5-6: 연두색
  - Lv.7+: 노란색
- [x] TowerLevelManager 이벤트 연결
  - `OnTowerExpChanged` → 경험치 슬라이더 업데이트
  - `OnTowerLevelUp` → 레벨 텍스트 및 색상 업데이트

**Unity 설정:**
- Hierarchy → TowerLevelCanvas 선택
- Inspector → `TowerLevelUI` 컴포넌트 추가
- Tower Slots 배열에 4개 슬롯 연결 (Archer, Wizard, WizardTower, Laser)
- 각 슬롯의 UI 요소 연결 (LevelText, ExpSlider, PanelImage)

**Git Commit:** `0baac48` - Phase B Step 4: Add tower level UI with panel color change

#### **Step 5: 특수 능력 해금 시스템**
- [ ] 레벨 텍스트 표시 (Lv.0~)
- [ ] 레벨별 박스 색상 변경:
  - Lv.0-2: 회색
  - Lv.3-4: 하얀색
  - Lv.5-6: 연두색
  - Lv.7+: 노란색

#### **25. 특수 능력 해금 시스템**
- [ ] 특수 능력 데이터 구조 설계
- [ ] 레벨 3/5/7 도달 시 능력 해금 로직
- [ ] 능력별 구현:
  - **Archer**: Lv.3 관통1, Lv.5 관통2, Lv.7 공격속도20%
  - **Wizard**: Lv.3 광역1칸, Lv.5 광역2칸, Lv.7 공격력30%
  - **WizardTower**: Lv.3 공격력버프10%, Lv.5 공속버프10%, Lv.7 범위+1
  - **Laser**: Lv.3 사거리+1, Lv.5 사거리+2, Lv.7 관통
- [ ] 스텟 보너스에만 레어도 계수 적용

#### **26. Unit.cs 수정 사항**
- [ ] Initialize()에서 능력치 계산: (기본 + 특수) × 레어도
- [ ] 특수 능력 적용 로직 추가
- [ ] ApplySpecialAbilities() 메서드 구현

#### **27. UnitMergeManager.cs 수정 사항**
- [ ] MergeUnits()에 경험치 획득 로직 추가
- [ ] TowerLevelManager.Instance.AddExp() 호출
- [ ] 레벨업 체크 및 UI 업데이트
- [ ] 상위 유닛 생성 로직 유지

#### **28. Unity 설정**
- [ ] Enemy 프리팹에 Slider 체력바 오브젝트 추가
- [ ] Bullet 프리팹에 hitEffectPrefab 연결
- [ ] UI Canvas에 TowerLevelUI 스크립트 연결
- [ ] Managers 오브젝트에 TowerLevelManager 컴포넌트 추가

### 📊 시스템 흐름도

```
[합성] → [경험치 획득] → [레벨업 체크] → [초과분 이월]
                                          ↓
[UI 업데이트] ← [능력치 재계산] ← [특수능력 해금]
                                          ↓
                                [상위 유닛 생성]
```
