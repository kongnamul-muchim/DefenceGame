# DefenceGame 진행 상황

## 📅 마지막 업데이트
- **날짜**: 2026-03-09
- **Git Commit**: `7f3b485` - Fix GameManager: Add IGameManagerService interface implementation

---

## ✅ 완료된 작업 (Completed)

### Phase 0: 프로젝트 설정
| # | 작업 | 설명 | 상태 |
|---|------|------|------|
| 1 | Framework DI/EventBus 재사용 | BallShotGame 프레임워크 통합 | ✅ 완료 |
| 2 | 폴더 구조 생성 | Core, Data, Services, UI 등 | ✅ 완료 |
| 3 | Git 초기화 및 커밋 | Initial commit | ✅ 완료 |

### Phase 0.5: Excel 데이터 시스템
| # | 작업 | 설명 | 상태 |
|---|------|------|------|
| 4 | ExcelDataReader 통합 | ExcelDataReader, DataSet, Packaging DLL 설치 | ✅ 완료 |
| 5 | 데이터 클래스 정의 | EnemyData, TowerData, WaveData, GachaData, UnitGradeData | ✅ 완료 |
| 6 | 리플렉션 기반 컨버터 | List<T> 자동 매핑, 범용 변환기 | ✅ 완료 |
| 7 | Unity Editor 메뉴 | Tools > DefenceGame > Convert Excel to GameData | ✅ 완료 |
| 8 | 파일 자동 감시 | FileSystemWatcher로 Excel 변경 시 자동 변환 | ✅ 완료 |
| 9 | 샘플 엑셀 생성기 | DocumentFormat.OpenXml 기반 샘플 생성 | ✅ 완료 |

### Phase 1: 코어 시스템
| # | 작업 | 설명 | 상태 |
|---|------|------|------|
| 10 | **GridSystem** | 맵을 Grid 노드로 변환, Wall Tilemap 감지 | ✅ 완료 |
| 11 | **A* Pathfinder** | Wall 회피 최단 경로 계산 (한 번 계산 후 고정) | ✅ 완료 |
| 12 | **PathAgent** | 경로 따라 이동, Waypoint 단위 이동 | ✅ 완료 |
| 13 | **GameManager** | Castle HP (20→0), 생존 시간, 점수, 게임 상태 | ✅ 완료 |
| 14 | **WaveManager** | Excel 데이터 기반 웨이브, 겹치지 않는 스폰 | ✅ 완료 |
| 15 | **Enemy 클래스** | A* 경로 이동, Castle 도달 시 HP 감소, 처치 보상 | ✅ 완료 |

---

## 🚧 진행 중인 작업 (In Progress)
없음

---

## 📋 남은 작업 (Pending)

### Phase 2: 적 시스템 마무리
| # | 작업 | 설명 | 예상 시간 |
|---|------|------|-----------|
| 16 | Enemy 스프라이트 연동 | 64x64 monsters 텍스처 매핑 | 20분 |
| 17 | Enemy 프리팹 설정 | SpriteRenderer, Collider2D 설정 | 15분 |
| 18 | Enemy 체력바 UI | HP 바 표시 | 20분 |

### Phase 3: 유닛(타워) 시스템
| # | 작업 | 설명 | 예상 시간 |
|---|------|------|-----------|
| 19 | **Unit 데이터 연동** | player.png 텍스처 사용 | 20분 |
| 20 | **Unit Gacha** | 랜덤 등급/위치 배치 (Wall 타일에만) | 40분 |
| 21 | **Unit Attack - 근접** | player 근접 공격 애니메이션/데미지 | 30분 |
| 22 | **Unit Attack - 원거리** | 투사체 발사, 적 타겟팅 | 40분 |
| 23 | **Unit Placement** | Wall Tilemap에만 배치 가능 검증 | 20분 |

### Phase 4: UI 및 게임 완성
| # | 작업 | 설명 | 예상 시간 |
|---|------|------|-----------|
| 24 | **Game Over System** | 20마리 통과 시 패배 화면 | 20분 |
| 25 | **Score System UI** | 생존시간, 처치수 표시 | 20분 |
| 26 | **UI 구현** | HP바, Score, Wave, Gacha 버튼 | 40분 |
| 27 | **게임 시작/재시작** | 메인 메뉴, 재시작 기능 | 20분 |

### Phase 5: 테스트 및 밸런스
| # | 작업 | 설명 | 예상 시간 |
|---|------|------|-----------|
| 28 | **통합 테스트** | 전체 시스템 테스트 | 30분 |
| 29 | **Excel 밸런스 조정** | 데이터 튜닝 | 20분 |
| 30 | **성능 최적화** | 오브젝트 풀링 등 | 30분 |

---

## 📁 폴더 구조

```
Assets/
├── Framework/              # DI & EventBus (재사용)
│   ├── Core/
│   │   ├── EventBus.cs
│   │   ├── GameService.cs
│   │   └── IService.cs
│   └── Events/
├── Plugins/
│   └── ExcelDataReader/
│       ├── ExcelDataReader.dll
│       ├── ExcelDataReader.DataSet.dll
│       ├── System.IO.Packaging.dll
│       └── DocumentFormat.OpenXml.dll
├── Scripts/
│   └── DefenceGame/
│       ├── Core/
│       │   ├── GridSystem.cs         ✅
│       │   ├── Pathfinder.cs         ✅
│       │   ├── PathAgent.cs          ✅
│       │   ├── GameManager.cs        ✅
│       │   ├── WaveManager.cs        ✅
│       │   └── Enemy.cs              ✅
│       ├── Data/
│       │   ├── GameDataSO.cs         ✅
│       │   ├── EnemyData.cs          ✅
│       │   ├── TowerData.cs          ✅
│       │   ├── WaveData.cs           ✅
│       │   ├── GachaData.cs          ✅
│       │   ├── UnitGradeData.cs      ✅
│       │   ├── GradeType.cs          ✅
│       │   └── Editor/
│       │       ├── ExcelConverter.cs ✅
│       │       ├── ExcelSampleGenerator.cs ✅
│       │       ├── GameDataEditorMenu.cs   ✅
│       │       └── GameDataSettingsWindow.cs ✅
│       ├── Services/
│       ├── Events/
│       └── UI/
├── Prefabs/
│   ├── Enemies/            # 적 프리팹들 ⏳
│   ├── Units/
│   └── Projectiles/
├── ScriptableObjects/
│   └── GameData.asset      # Excel 변환 결과
└── Scenes/
    └── PlayScenes.unity    # 메인 게임 씬
```

---

## 🔧 주요 시스템 설명

### GridSystem
- **크기**: 28x14 Grid
- **좌표**: (-18, -7) ~ (9, 6)
- **기능**: Wall Tilemap 자동 감지, 4방향 이웃 탐색

### A* Pathfinder
- **알고리즘**: A* (A-Star) Pathfinding
- **방식**: 한 번 계산 후 고정 (요청사항)
- **장애물**: Wall Tile 회피

### GameManager
- **Castle HP**: 20 → 0 (게임오버)
- **Castle 영역**: 3x3 (중심 기준)
- **점수 공식**: 생존시간×10 + 처치수×100
- **상태**: Playing / GameOver

### WaveManager
- **데이터 소스**: GameDataSO (Excel 변환)
- **스폰 방식**: 겹치지 않게 랜덤 오프셋
- **진행**: 자동 (3초 간격)

### Enemy
- **이동**: A* 경로 따라 이동
- **Castle 도달**: HP 감소 (1)
- **보상**: 처치 시 골드

---

## 📊 엑셀 데이터 구조

### Enemies 시트
| Id | Name | Health | Speed | RewardGold |
|----|------|--------|-------|------------|
| 1 | Slime | 100 | 2.0 | 10 |
| 2 | Goblin | 200 | 3.0 | 20 |
| 3 | Orc | 500 | 2.5 | 50 |
| 4 | DarkKnight | 1500 | 2.0 | 100 |
| 5 | Dragon | 5000 | 1.5 | 500 |

### Towers 시트
| Id | Name | AttackPower | AttackSpeed | Range | Grade |
|----|------|-------------|-------------|-------|-------|
| 1 | Archer | 10 | 1.0 | 5.0 | Common |
| 2 | Mage | 25 | 0.8 | 6.0 | Rare |
| 3 | Cannon | 50 | 0.5 | 4.0 | Epic |
| 4 | Laser | 100 | 2.0 | 8.0 | Legendary |

### Waves 시트
| WaveNumber | EnemyId | Count | SpawnInterval | HealthMultiplier |
|------------|---------|-------|---------------|------------------|
| 1 | 1 | 5 | 2.0 | 1.0 |
| 2 | 1 | 8 | 1.8 | 1.1 |
| 3 | 2 | 5 | 1.5 | 1.2 |

---

## 🚀 다음 작업 순서

1. **Unity Scene 설정** (수동 작업)
   - Grid 오브젝트에 GridSystem 컴포넌트 추가
   - Managers 오브젝트 생성 (Pathfinder, GameManager, WaveManager)
   - Castle Transform 연결

2. **Phase 2**: Enemy 스프라이트 연동

3. **Phase 3**: 유닛(타워) 시스템 구현

4. **Phase 4**: UI 및 게임 완성

---

## 🐛 알려진 이슈

1. **EnemySpawnPoint 오타**: "Enermy" → "Enemy" (수정 필요)
2. **Castle Sprite**: 투명 상태 (의도된 설정)

---

## 📝 비고
- **개발 환경**: Unity 2D URP
- **의존성**: ExcelDataReader, DocumentFormat.OpenXml
- **디자인 패턴**: DI (Service Locator), Event Bus (Pub/Sub)
- **경로 탐색**: A* 알고리즘, 4방향 이동
