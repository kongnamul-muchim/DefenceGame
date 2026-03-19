# DefenceGame 작업 로그

## 2026-03-12: SOLID 원칙 기반 아키텍처 리팩토링

### 주요 변경사항

#### 1. GameManager 분리 (SRP 적용)
**기존:** GameManager가 모든 게임 상태를 관리 (227줄)
**변경:** 4개의 전문 시스템 매니저로 분리

| 새 시스템 | 단일 책임 | 파일 |
|-----------|----------|------|
| **GoldManager** | 골드 획득/소비 관리 | `Core/Systems/Economy/GoldManager.cs` |
| **ScoreManager** | 점수/생존시간/처치수 관리 | `Core/Systems/Progression/ScoreManager.cs` |
| **CastleManager** | 성 체력/방어 관리 | `Core/Systems/Defense/CastleManager.cs` |
| **GameStateManager** | 게임 상태(Playing/GameOver) 관리 | `Core/Systems/State/GameStateManager.cs` |

**GameManager:** Facade 패턴으로 변경 - 기존 API 유지하며 새 시스템에 위임

#### 2. WaveManager 개선
**기존:** 웨이브 관리 + 적 스폰 로직 혼재 (353줄)
**변경:** EnemySpawner 분리

| 구성요소 | 책임 | 파일 |
|----------|------|------|
| **WaveManager** | 웨이브 타이밍/흐름 관리 | `Core/Managers/WaveManager.cs` (187줄) |
| **EnemySpawner** | 적 생성 및 초기화 | `Core/Systems/Spawning/EnemySpawner.cs` |

#### 3. 새 폴더 구조
```
Core/
├── Managers/          # 기존 매니저들
├── Systems/           # 새로 추가 - 세분화된 시스템
│   ├── Economy/       # GoldManager
│   ├── Progression/   # ScoreManager
│   ├── Defense/       # CastleManager
│   ├── State/         # GameStateManager
│   └── Spawning/      # EnemySpawner
├── Units/
├── Enemies/
├── Projectiles/
├── Grid/
└── Interfaces/
```

#### 4. UI 파일 업데이트
- **GameUI.cs:** 새 시스템 매니저들의 이벤트 구독으로 변경
- **TowerLevelUI.cs:** Managers 네임스페이스 using 추가

#### 5. Enemy.cs 개선
- CastleManager 직접 참조로 변경 (의존성 역전)
- GameManager Facade를 통한 간접 호출 제거

### 적용된 SOLID 원칙

| 원칙 | 적용 내용 |
|------|----------|
| **S (Single Responsibility)** | GameManager → 4개 시스템, WaveManager → EnemySpawner 분리 |
| **O (Open/Closed)** | 새 시스템 추가 시 기존 코드 수정 없이 확장 가능 |
| **I (Interface Segregation)** | 각 시스템이 독립적인 이벤트 제공 |
| **D (Dependency Inversion)** | Enemy.cs가 CastleManager 직접 사용 |

### 생성된 파일
- `Core/Systems/Economy/GoldManager.cs` (55줄)
- `Core/Systems/Progression/ScoreManager.cs` (76줄)
- `Core/Systems/Defense/CastleManager.cs` (90줄)
- `Core/Systems/State/GameStateManager.cs` (80줄)
- `Core/Systems/Spawning/EnemySpawner.cs` (122줄)

### 리팩토링된 파일
- `Core/Managers/GameManager.cs` - Facade 패턴 (68줄, 기존 227줄)
- `Core/Managers/WaveManager.cs` - 단순화 (187줄, 기존 353줄)
- `Core/Enemies/Enemy.cs` - CastleManager 사용
- `UI/GameUI.cs` - 새 시스템 이벤트 구독
- `UI/TowerLevelUI.cs` - Managers 네임스페이스 추가

### 커밋
- Hash: `627afb0`
- Message: `refactor: Apply SOLID principles - split GameManager into system managers`

---

## 2024-03-11 ~ 2024-03-15: 특수능력 시스템 및 핵심 기능 구현

### 주요 변경사항 요약

#### 1. 특수능력 시스템 재설계 (2024-03-11)
**변경된 능력:**
| 타워 | 기존 능력 | 새로운 능력 | 설명 |
|------|----------|------------|------|
| **Archer** | 관통 | **MultiShot** | 레벨 3: 2개, 5: 3개, 7: 공격속도 50% 증가 |
| **Wizard** | 광역 | **GroundEffect** | 10% 확률로 5초 지속 피해 바닥 생성 |
| **WizardTower** | 버프/디버프 | **Range+Attack** | 레벨 3: 사거리+0.5, 5: 공격력30%, 7: 사거리+1 |
| **Laser** | 사거리/관통 | **ChainAttack** | 등급 기반 확산 (Common:0 ~ Legendary:3) |

**새 파일:**
- `SpecialAbilityData.cs`, `SpecialAbilityManager.cs`, `GroundEffect.cs`, `GradeMultiplier.cs`

**커밋:** `44317b1` - feat: redesign special abilities system

---

#### 2. 컴포넌트 분리 리팩토링 (2024-03-12)
Unit.cs(1000줄+)를 5개 컴포넌트로 분리:
- **Unit.cs**: 핵심 데이터와 조율 (~100줄)
- **UnitAttack.cs**: 공격 로직 (~150줄)
- **UnitUI.cs**: UI/시각화 (~120줄)
- **UnitDrag.cs**: 드래그 기능 (~30줄)
- **UnitAbility.cs**: 특수능력 관리 (~100줄)

**삭제:** 사용하지 않는 버프/디버프 로직, 필드 5개

**커밋:** `1a753e9` - refactor: Split Unit.cs into separate components

---

#### 3. 등급 기반 특수능력 강화 (2024-03-13)
**등급 배율표:**
| 등급 | 배율 | 약한 배율 (Laser용) |
|------|------|-------------------|
| Common | 1.0x | 0.7x |
| Uncommon | 1.2x | 0.85x |
| Rare | 1.5x | 1.0x |
| Epic | 2.0x | 1.3x |
| Legendary | 3.0x | 1.8x |

**적용 효과:**
- Archer: 공격속도 증가폭이 등급에 비례
- Wizard: GroundEffect 확률이 등급에 비례 (10% → 30%)
- WizardTower: 공격력 증가폭이 등급에 비례 (30% → 90%)
- Laser: 연계 피해량이 등급에 비례 (70% → 180%)

**커밋:** `2fa35f8` - feat: Add grade-based special ability scaling

---

#### 4. 버그 수정 및 밸런스 조정 (2024-03-14)
**수정된 버그:**
1. **Archer 화살**: lifetime 5초→1초, OnTriggerEnter2D로 충돌 감지
2. **GroundEffect**: Trigger collider 체크 추가, 데미지 정상 적용
3. **Laser Common**: 확산 없음(0)으로 수정
4. **파티클**: loop=true로 설정하여 지속시간 동안 재생

**밸런스 변경:**
- Wizard GroundEffect: 지속시간 1.3초→5초, DPS 50%→20%
- 사거리 계산: 보정값 2.5f 추가 (최소값 8→3.2)

**커밋들:**
- `62f22f5` - fix: Arrow lifetime, damage detection, stats calculation
- `59b44bc` - fix: Bullet/GroundEffect collision detection and range calculation
- `7661ec5` - feat: Update Wizard GroundEffect duration and add Laser grade-based chain attack
- `bb5334e` - fix: Laser Common grade chain attack and GroundEffect particle loop
- `96e9fe2` - feat: Change Laser level abilities to Attack Up + Speed Down

---

#### 5. 폴더 구조 개선 (2024-03-14)
**변경 전:** 모든 파일이 Core/에 존재
**변경 후:**
```
Core/
├── Units/ (Unit.cs, UnitAttack.cs, UnitAbility.cs, UnitUI.cs, ...)
├── Enemies/ (Enemy.cs, EnemyHealth.cs, EnemyMovement.cs, EnemyStatusEffect.cs)
├── Projectiles/ (Bullet.cs, GroundEffect.cs)
├── Managers/ (GameManager.cs, WaveManager.cs, TowerLevelManager.cs, ...)
├── Grid/ (GridSystem.cs)
└── Interfaces/ (IDamageable.cs, IInitializable.cs, IStatProvider.cs)
```

**커밋:** `62f22f5` (folder reorganization 포함)

---

#### 6. 테스트 코드 정리 (2024-03-15)
**삭제:**
- `GradeAbilityTestManager.cs` (200줄)
- `UnitAbility.cs`: Test Mode 필드, Debug.Log 15개
- `Unit.cs`: Debug.Log
- `TowerLevelUI.cs`: 테스트 버튼 로직

**커밋:** `9e70785` - refactor: Remove test code and Debug.Logs

---

#### 7. 적 처치 경험치 시스템 (2024-03-15)
**구현 기능:**
1. **합성 시 색상 변경 버그 수정**: Unit.Initialize에 SetGradeColor 추가
2. **적 처치 경험치**: lastAttacker 추적 → 타워 타입에 경험치 추가 (등급별 배율 적용)
3. **필드 유닛 레벨업**: OnTowerLevelUp 이벤트로 필드 유닛 자동 레벨업
4. **가챠 소환**: UnitAbility.Initialize에서 현재 레벨 조회 및 적용

**등급별 경험치 배율:** Common 1.0x ~ Legendary 3.0x

**주요 변경 파일:**
- `Enemy.cs`: lastAttacker 필드, TakeDamage(Unit attacker) 추가
- `TowerLevelManager.cs`: AddExpFromKill() 메서드 추가
- `Bullet.cs`: attacker 필드, Initialize 파라미터 추가
- `UnitAttack.cs`: 모든 공격에 attacker 정보 전달

**커밋:** `fc51ff7` - feat: Add enemy kill exp system and fix merge color bug

---

## SOLID 원칙 기반 리팩토링 (2024-03-15)

### 적용 원칙
- **S (Single Responsibility)**: Enemy를 Health/Movement/StatusEffect로 분리
- **I (Interface Segregation)**: IDamageable, IInitializable, IStatProvider 인터페이스 정의
- **D (Dependency Inversion)**: 인터페이스 기반 의존성 관리

### 새 파일
- `Core/Interfaces/IDamageable.cs`
- `Core/Interfaces/IInitializable.cs`
- `Core/Interfaces/IStatProvider.cs`
- `Core/Enemies/EnemyHealth.cs`
- `Core/Enemies/EnemyMovement.cs`
- `Core/Enemies/EnemyStatusEffect.cs`

### 리팩토링된 파일
- `Enemy.cs`: 컴포넌트 조율 역할만 담당 (197줄 → 120줄)

---

## 변경사항 통계
| 기간 | 커밋 수 | 파일 변경 | 추가 | 삭제 |
|------|---------|----------|------|------|
| 2024-03-11 ~ 03-15 | 11 | 50+ | 2,500+ | 1,200+ |

### 다음 단계 작업
- [ ] Unity 빌드 테스트
- [ ] 성능 프로파일링
- [ ] 통합 테스트 (전체 기능)

---

## 2024-03-15: 무한 웨이브 시스템 및 밸런스 개선

### 구현 내용

#### 1. 무한 웨이브 시스템
- 웨이브 6 이후 7, 8, 9... 무한 진행
- 1-6 웨이브 데이터 순환 사용
- 체력 배율: `기본 × (1 + (웨이브-1) × 10%)`
  - 웨이브 7: +60%, 웨이브 12: +110%, 웨이브 20: +190%

#### 2. Elite 적 시스템 (웨이브 7+)
- 생성 확률: 10%
- 보라색 색상 (0.6, 0.2, 0.8)
- 체력 2배, 골드 3배, 크기 1.2배
- 이름에 "Elite_" 접두사 추가

#### 3. 웨이브 클리어 골드 보너스
- 웨이브 1-6: 50-100 골드 (10씩 증가)
- 웨이브 7+: 105골드부터 5씩 증가

#### 4. 타워 밸런스 조정
- **Archer 레벨 7**: 공격속도 +50% → +30%
- **Unit.cs**: 공격속도에 등급 배율 적용

#### 5. 버그 수정
- **GroundEffect 킬 크레딧**: Wizard에게 경험치 지급
- **UnitAbility**: attacker 정보 GroundEffect에 전달

#### 6. 피격 시각 효과
- 적 피격 시 투명한 흰색으로 0.1초 플래시
- EnemyHealth.cs에 구현

#### 7. 보스 웨이브 개선
- 보스가 죽을 때까지 다음 웨이브로 넘어가지 않음
- WaveManager가 현재 보스를 추적 (currentBoss)
- 보스 처치 후 3초 대기 후 다음 웨이브 시작

### 변경된 파일

| 파일 | 변경 사항 |
|------|----------|
| **WaveManager.cs** | 무한 웨이브, Elite 시스템, 골드 보너스, 보스 체크 |
| **SpecialAbilityManager.cs** | Archer 레벨7 50%→30% |
| **Unit.cs** | 공격속도 등급 배율 적용 |
| **GroundEffect.cs** | attacker 필드 추가, TakeDamage에 attacker 전달 |
| **UnitAbility.cs** | SpawnGroundEffect에 unit 전달 |
| **EnemyHealth.cs** | 피격 플래시 효과 추가 |
| **GachaManager.cs** | 가챠 가격 스케일링 (이전 커밋) |

### 체력 배율 예시
```
웨이브 1 (슬라임): 30 HP
웨이브 6 (보스): 5,000 HP
웨이브 7 (슬라임): 30 × 1.6 = 48 HP
웨이브 12 (보스): 5,000 × 2.1 = 10,500 HP
웨이브 20 (보스): 5,000 × 2.9 = 14,500 HP
```

### 커밋
- Hash: `7680c1a`
- Message: `feat: Implement infinite wave system and balance adjustments`

### 다음 단계 작업
- [ ] Unity에서 Elite 적 색상 확인
- [ ] 웨이브 7+ 체력 스케일링 테스트
- [ ] 피격 플래시 효과 확인
