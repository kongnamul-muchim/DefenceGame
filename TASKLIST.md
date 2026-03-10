# DefenceGame 진행 상황

## 📅 마지막 업데이트
- **날짜**: 2026-03-10
- **Git Commit**: `4a3a6f5` - Add randomized pathfinding and enemy movement improvements

---

## ✅ 완료된 작업 (Completed)

### Phase 1: 코어 시스템 (완료)
| # | 작업 | 설명 | 상태 |
|---|------|------|------|
| 1 | **Framework DI/EventBus** | BallShotGame 프레임워크 통합 | ✅ 완료 |
| 2 | **Excel 데이터 시스템** | ExcelDataReader, 데이터 클래스, 자동 변환 | ✅ 완료 |
| 3 | **GridSystem** | Tilemap 연결, Wall/Barrier 감지 | ✅ 완료 |
| 4 | **A* Pathfinder** | 랜덤 경로, 웨이포인트 시스템 | ✅ 완료 |
| 5 | **PathAgent** | 경로 따라 이동, 스프라이트 방향 전환 | ✅ 완료 |
| 6 | **GameManager** | Castle HP, 생존 시간, 점수, 골드 시스템 | ✅ 완료 |
| 7 | **WaveManager** | Excel 기반 웨이브, 다양한 Enemy 스폰 | ✅ 완료 |
| 8 | **Enemy 시스템** | 랜덤 경로 이동, Castle 도달, 처치 보상 | ✅ 완료 |
| 9 | **GachaManager** | 50골드 가챠, Excel 확률 연동 | ✅ 완료 |

---

## 📋 남은 작업 (Pending)

### Phase 2: Enemy 시스템 마무리
| # | 작업 | 설명 | 예상 시간 |
|---|------|------|-----------|
| 10 | Enemy 체력바 UI | HP 바 표시 | 20분 |

### Phase 3: Unit(타워) 시스템
| # | 작업 | 설명 | 예상 시간 |
|---|------|------|-----------|
| 11 | **Unit 배치 시스템** | SpaceBar 가챠 → 바로 배치 | 30분 |
| 12 | **Unit 데이터 연동** | TowerData Excel 연동 | 20분 |
| 13 | **Unit 공격 - 근접** | 근접 공격, 애니메이션 | 30분 |
| 14 | **Unit 공격 - 원거리** | 투사체 발사, 적 타겟팅 | 40분 |
| 15 | **Unit Placement** | Ground Tilemap에만 배치 가능 | 20분 |

### Phase 4: UI 및 게임 완성
| # | 작업 | 설명 | 예상 시간 |
|---|------|------|-----------|
| 16 | **UI 시스템** | HP바, Score, Wave, Gold 표시 | 40분 |
| 17 | **Game Over UI** | 패배 화면, 재시작 버튼 | 20분 |

### Phase 5: 테스트 및 밸런스
| # | 작업 | 설명 | 예상 시간 |
|---|------|------|-----------|
| 18 | 통합 테스트 | 전체 시스템 테스트 | 30분 |
| 19 | Excel 밸런스 조정 | 데이터 튜닝 | 20분 |

---

## 🎮 조작법

| 키 | 기능 |
|----|------|
| **SpaceBar** | 가챠 실행 (50골드 소모) → 유닛 자동 배치 |

---

## 📊 엑셀 데이터 구조

### GachaProbabilities 시트
| Grade | Probability | Cost |
|-------|-------------|------|
| Common | 0.60 | 50 |
| Rare | 0.25 | 50 |
| Epic | 0.10 | 50 |
| Legendary | 0.05 | 50 |

### Towers 시트
| Id | Name | AttackPower | AttackSpeed | Range | Grade |
|----|------|-------------|-------------|-------|-------|
| 1 | Archer | 10 | 1.0 | 5.0 | Common |
| 2 | Mage | 25 | 0.8 | 6.0 | Rare |
| 3 | Cannon | 50 | 0.5 | 4.0 | Epic |
| 4 | Laser | 100 | 2.0 | 8.0 | Legendary |

---

## 🚀 다음 작업 순서

1. **SpaceBar 가챠 시스템** - SpaceBar 누르면 50골드 소모 후 랜덤 위치에 유닛 배치
2. **Unit 공격 시스템** - 근접/원거리 공격 구현
3. **UI 시스템** - 게임 상태 표시
4. **테스트 및 밸런스**

---

## 📝 비고
- **개발 환경**: Unity 2D URP
- **의존성**: ExcelDataReader, DocumentFormat.OpenXml
- **디자인 패턴**: DI (Service Locator), Event Bus (Pub/Sub)
- **경로 탐색**: A* 알고리즘, 랜덤 웨이포인트
