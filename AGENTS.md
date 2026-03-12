# DefenceGame 작업 로그

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
