# DefenceGame 작업 로그

## 2024-03-11: 특수능력 시스템 재설계

### 변경 사항
기존 특수능력을 완전히 새로 기획하여 변경했습니다.

#### 능력 변경표

| 타워 | 기존 능력 | 새로운 능력 | 설명 |
|------|----------|------------|------|
| **Archer** | 관통 (Pierce) | **투사체 개수증가 (MultiShot)** | 레벨 3: 2개, 5: 3개, 7: 공격속도 50% 증가 |
| **Wizard** | 광역 (AreaDamage) | **지속피해 바닥 (GroundEffect)** | 10% 확률로 1.3초 지속 피해 바닥 생성 |
| **WizardTower** | 버프/디버프 | **사거리증가 + 공격력증가** | 레벨 3: 사거리+0.5, 5: 공격력30%, 7: 사거리+1 |
| **Laser** | 사거리/관통 | **연계공격 (ChainAttack)** | 레벨 3: 1회, 5: 2회, 7: 3회 주변 전이 |

#### 수정된 파일
- `Assets/Scripts/DefenceGame/Data/SpecialAbilityData.cs` - 새로운 능력 타입 enum 추가
- `Assets/Scripts/DefenceGame/Core/SpecialAbilityManager.cs` - 능력 데이터 재설정
- `Assets/Scripts/DefenceGame/Core/Enemy.cs` - 둔화 효과 버그 수정 (고정값 -> 실제 값 저장)

#### 새로 생성된 파일
- `Assets/Scripts/DefenceGame/Core/GroundEffect.cs` - 지속 피해 바닥 효과 컴포넌트

### 커밋
- Hash: `44317b1`
- Message: `feat: redesign special abilities system`

---

## 2024-03-12: 코드 리팩토링 - 컴포넌트 분리

### 변경 사항
Unit.cs가 너무 무거워져 (~1000줄) 유지보수가 어려워져 컴포넌트로 분리했습니다.

#### 분리된 컴포넌트

| 파일 | 설명 | 라인 수 |
|------|------|---------|
| **Unit.cs** | 핵심 데이터와 컴포넌트 조율 | ~100줄 |
| **UnitAttack.cs** | 공격 로직 (타겟 찾기, 투사체 발사) | ~150줄 |
| **UnitUI.cs** | UI/시각화 (범위 표시, 호버 효과) | ~120줄 |
| **UnitDrag.cs** | 드래그 기능 | ~30줄 |
| **UnitAbility.cs** | 특수능력 관리 | ~100줄 |

#### 삭제된 코드
- `ApplyMageTowerEffects()` - 사용하지 않는 버프/디버프 로직
- `SetupBuffRangeVisualization()` - 버프 범위 시각화
- `ReceiveAttackBuff()` / `RemoveAttackBuff()` - 버프 관리 메서드
- Deprecated 필드들: `attackBuffValue`, `speedBuffValue`, `slowEffectValue`, `buffRange`, `buffRangeRenderer`

#### 개선사항
- **단일 책임 원칙**: 각 컴포넌트가 하나의 역할만 담당
- **코드 가독성**: 파일당 100~150줄로 관리 가능
- **유지보수성**: 개별 기능 수정 시 다른 부분 영향 최소화
- **성능**: 컴포넌트 캐싱으로 GetComponent 호출 감소

### 커밋
- Hash: `1a753e9`
- Message: `refactor: Split Unit.cs into separate components`
- Changes: 616 insertions(+), 5881 deletions(-)

### 다음 단계 작업 목록
- [x] 컴포넌트 분리 및 리팩토링
- [x] 불필요한 코드 제거
- [x] 컴파일 에러 수정
- [ ] Unity에서 컴포넌트 연결 확인
- [ ] 기능 테스트 (공격, UI, 드래그, 특수능력)

---

## 2024-03-13: 컴파일 에러 수정

### 변경 사항
리팩토링 후 발생한 컴파일 에러들을 수정했습니다.

#### 수정된 파일
- `Assets/Scripts/DefenceGame/Core/Unit.cs` - 메서드 접근 제한자 변경 및 신규 메서드 추가
- `Assets/Scripts/DefenceGame/Core/UnitUI.cs` - 색상 관리 메서드 추가
- `Assets/Scripts/DefenceGame/Core/UnitAttack.cs` - 프로퍼티 접근 방식 수정
- `Assets/Scripts/DefenceGame/Core/UnitDragSystem.cs` - 직접 필드 접근 제거

#### 수정 내용
1. **GetGradeColor**: `private` → `public`으로 변경
2. **GetTowerType()**: 메서드 호출 → `TowerType` 프로퍼티 접근으로 변경
3. **SetRareColor/RestoreGradeColor**: Unit.cs와 UnitUI.cs에 추가
4. **rareColorRenderer 직접 접근**: UnitDragSystem.cs에서 메서드 호출로 변경

---

## 2024-03-13: 특수능력 등급 기반 강화 시스템 구현

### 변경 사항
특수능력이 타워의 등급에 따라 더 강력해지도록 시스템을 개선했습니다.

#### 등급 배율표

| 등급 | 배율 | 약한 배율 (Laser용) |
|------|------|-------------------|
| Common | 1.0x | 0.7x |
| Uncommon | 1.2x | 0.85x |
| Rare | 1.5x | 1.0x |
| Epic | 2.0x | 1.3x |
| Legendary | 3.0x | 1.8x |

#### 타워별 등급 기반 강화 효과

| 타워 | 능력 | 등급 효과 |
|------|------|----------|
| **Archer** | 공격속도 증가 | 레벨 7의 50% 증가가 등급에 비례 (Common=50%, Legendary=150%) |
| **Wizard** | GroundEffect 확률 | 기본 10%가 등급에 비례 (Common=10%, Legendary=30%) |
| **WizardTower** | 공격력 증가 | 레벨 5의 30% 증가가 등급에 비례 (Common=30%, Legendary=90%) |
| **Laser** | 연계 피해량 | 첫 타는 정상, 연계 시 등급 배율 적용 (Common=70%, Legendary=180%) |

#### 수정된 파일
- `Assets/Scripts/DefenceGame/Data/GradeMultiplier.cs` - 등급별 배율 계산 유틸리티 (신규)
- `Assets/Scripts/DefenceGame/Core/UnitAbility.cs` - 등급 기반 버프 계산 추가
- `Assets/Scripts/DefenceGame/Core/Bullet.cs` - Laser 연계 공격 등급 기반 데미지 적용
- `Assets/Scripts/DefenceGame/Core/UnitAttack.cs` - Laser 연계 공격 시 등급 배율 전달

#### 주요 변경사항
1. **GradeMultiplier 클래스**: 등급별 배율을 중앙에서 관리
2. **UnitAbility.CalculateGradeBasedValues()**: 등급에 따른 최종 능력 값 계산
3. **Bullet.chainDamageMultiplier**: Laser 연계 공격 시 등급 기반 데미지 감소/증가
4. **디버그 로그**: 각 능력 적용 시 등급과 계산된 값 출력

### 커밋
- Hash: `2fa35f8`
- Message: `feat: Add grade-based special ability scaling`
- Changes: 새 파일 1개, 수정 7개 파일 (291 insertions, 27 deletions)

---

## 2024-03-13: 등급 기반 특수능력 테스트 매니저 추가

### 변경 사항
등급 기반 특수능력을 테스트하기 위한 테스트 매니저를 추가했습니다.

#### 기능
- **자동 소환**: 타워가 레벨 7에 도달하면 해당 타워의 모든 등급(Common~Legendary)을 랜덤 순서로 소환
- **수동 테스트**: 인스펙터에서 ContextMenu로 각 타워별 테스트 가능
- **등급별 색상**: 소환된 유닛의 등급에 따른 색상 확인 가능

#### 설정 옵션
- `enableTestMode`: 테스트 모드 활성화/비활성화
- `autoSpawnOnLevel7`: 레벨 7 자동 소환 여부
- `spawnInterval`: 등급별 소환 간격 (초)

#### 새로 생성된 파일
- `Assets/Scripts/DefenceGame/Core/GradeAbilityTestManager.cs` - 테스트 매니저 (200줄)

#### 테스트 방법
1. Hierarchy에 빈 GameObject 생성
2. `GradeAbilityTestManager` 컴포넌트 추가
3. `enableTestMode` 체크
4. 게임 플레이 중 타워를 합성하여 레벨 7 달성
5. 또는 인스펙터에서 `Test Spawn XXX` 메뉴 클릭

### 디버그 로그
콘솔에서 다음 로그로 등급 기반 능력 확인 가능:
- `[UnitAbility] Archer Speed Increase: Base=0.5, Grade=Rare, Final=0.75`
- `[UnitAbility] Wizard GroundEffect Chance: Base=0.1, Grade=Epic, Final=0.2`
- `[GradeAbilityTest] 소환됨: Archer (Legendary) - 배율: 3.00`

---

## 2024-03-14: 합성 경험치 획득 제거

### 변경 사항
합성(merge)을 통한 타워 레벨 경험치 획득을 제거했습니다.

**기존 시스템:**
- 합성 시 해당 타워 타입에 경험치 추가
- 예: Common Archer 2개 합성 → Archer 타워에 Common 등급 경험치만큼 추가

**변경된 시스템:**
- 합성 시 경험치 획득 없음
- 타워 레벨업은 오직 적 처치를 통해서만 가능

#### 수정된 파일
- `Assets/Scripts/DefenceGame/Core/UnitMergeManager.cs` - 합성 시 경험치 획득 코드 제거 (lines 84-89)

#### 삭제된 코드
```csharp
// 4. 경험치 획득 (합성하는 유닛의 등급 기준)
if (TowerLevelManager.Instance != null)
{
    TowerLevelManager.Instance.AddExp(unitName, originalGrade);
    Debug.Log($"[Merge] Added {originalGrade} exp to {unitName}");
}
```

#### 주석 번호 수정
기존: 3 → 4 → 5 → 6 → 7
변경: 3 → 4 → 5 → 6 (경험치 획득 단계 제거로 인한 번호 축소)

### 커밋
- Hash: `8be6aba`
- Message: `refactor: Remove exp gain from merge`
- Changes: 1개 파일 수정 (7 lines removed)

---

## 2024-03-14: UnitAbility 테스트 모드 추가

### 변경 사항
특수효과가 UI 클릭 레벨업 시 적용되지 않는 문제를 해결했습니다.

**문제 원인:**
- UnitAbility.Initialize()에서 TowerLevelManager.GetTowerLevel() 호출 시 레벨이 0으로 반환
- 특수능력은 레벨 3/5/7에 unlock되므로 레벨 0에서는 적용되지 않음

**해결 방안:**
- `autoApplyCurrentLevel` 옵션 추가 (기본값: true)
- Initialize 시 TowerLevelManager에서 현재 레벨을 조회하여 자동 적용
- ForceApplyLevel() 메서드 추가하여 특정 레벨의 능력 강제 적용 가능

#### 수정된 파일
- `Assets/Scripts/DefenceGame/Core/UnitAbility.cs`
  - `autoApplyCurrentLevel` 필드 추가 (Inspector에서 확인/조작 가능)
  - `currentTowerLevel`, `currentTowerType` 디버그 필드 추가
  - `ForceApplyLevel(int level)` 메서드 추가
  - Initialize()에 테스트 모드 로직 추가

#### 사용 방법
1. Unit Prefab의 UnitAbility 컴포넌트 확인
2. `Auto Apply Current Level` 체크됨 확인 (기본값)
3. TowerLevelUI에서 타워 클릭하여 레벨업
4. 새로 소환되는 유닛에 해당 레벨의 특수능력 자동 적용

#### 인스펙터 표시 항목
- **Current Tower Level**: 현재 적용된 타워 레벨 (읽기 전용)
- **Current Tower Type**: 현재 타워 타입 (읽기 전용)
- **Auto Apply Current Level**: 테스트 모드 활성화/비활성화

### 커밋
- Hash: `TBD`
- Message: `feat: Add test mode to UnitAbility for auto-applying current tower level`
- Changes: 1개 파일 수정

---

## 2024-03-14: 버그 수정 및 폴더 구조 개선

### 변경 사항
여러 가지 버그를 수정하고 코드 폴더 구조를 개선했습니다.

#### 1. Archer 화살 버그 수정

**문제:**
- 화살이 무한으로 생성되고 적과 충돌하지 않아도 사라지지 않음
- 적과 닿아도 데미지가 들어가지 않고 뚫고 지나감

**해결:**
- `Bullet.cs`: lifetime을 5초에서 1초로 변경 (line 9)
- `Bullet.cs`: `OnTriggerEnter2D` 수정하여 모든 적과의 충돌 감지 (line 353-360)
  - 기존: `enemy == target` 조건만 체크
  - 변경: `!hitEnemies.Contains(enemy)` 조건 추가로 중복 타격 방지

#### 2. Wizard GroundEffect 데미지 버그 수정

**문제:**
- GroundEffect가 생성되지만 적에게 데미지가 들어가지 않음

**해결:**
- `GroundEffect.cs`: `ApplyTickDamage()` 메서드 개선 (line 87-118)
  - Trigger collider와 Non-trigger collider 모두 체크
  - Debug 로그 추가로 데미지 적용 확인 가능

#### 3. 공격력 및 사거리 계산 방식 변경

**기존 방식:**
- TowerData의 기본값만 사용

**새로운 방식 (Excel UnitGrades 시트 참조):**
- **공격력**: `(기본공격력 + 레벨추가공격력) × 레어도공격력`
- **사거리**: `기본사거리 × 레어도사거리`

**수정된 파일:**
- `Assets/Scripts/DefenceGame/Data/TowerData.cs`: `LevelBonusAttackPower` 필드 추가
- `Assets/Scripts/DefenceGame/Core/Unit.cs`: 
  - `Initialize()` 메서드에 등급 기반 스탯 계산 추가
  - `GetGradeMultiplier()` 메서드 추가로 Excel 데이터 참조

#### 4. 폴더 구조 재정비

**기존:**
```
Core/
  ├── Bullet.cs
  ├── Enemy.cs
  ├── GameManager.cs
  ├── ... (모든 파일이 한 폴더에)
```

**변경:**
```
Core/
  ├── Units/
  │   ├── Unit.cs
  │   ├── UnitAttack.cs
  │   ├── UnitAbility.cs
  │   ├── UnitUI.cs
  │   ├── UnitDrag.cs
  │   ├── UnitDragSystem.cs
  │   ├── UnitMergeManager.cs
  │   ├── UnitPlacementManager.cs
  │   └── GradeAbilityTestManager.cs
  ├── Projectiles/
  │   ├── Bullet.cs
  │   └── GroundEffect.cs
  ├── Enemies/
  │   ├── Enemy.cs
  │   ├── PathAgent.cs
  │   └── Pathfinder.cs
  ├── Managers/
  │   ├── GameManager.cs
  │   ├── WaveManager.cs
  │   ├── GachaManager.cs
  │   ├── TowerLevelManager.cs
  │   └── SpecialAbilityManager.cs
  └── Grid/
      └── GridSystem.cs
```

**개선사항:**
- **가독성**: 파일 역할별로 구분되어 찾기 쉬움
- **유지보수성**: 관련 파일들이 함께 있어 수정이 용이
- **네임스페이스**: 기존 `DefenceGame.Core` 네임스페이스 유지로 호환성 보장

### 커밋
- Hash: `62f22f5`
- Message: `fix: Arrow lifetime, damage detection, stats calculation, and folder reorganization`
- Changes: 50 files changed, 486 insertions(+), 80 deletions

### 다음 단계 작업 목록
- [ ] Unity에서 Excel 데이터 확인 (UnitGrades 시트 값 정상 로드 여부)
- [ ] Archer 화살 데미지 테스트
- [ ] Wizard GroundEffect 데미지 테스트
- [ ] 등급별 스탯 계산 검증 (Common vs Legendary 공격력/사거리 비교)

---

## 2024-03-14: 충돌 감지 및 사거리 버그 수정

### 변경 사항
충돌 감지와 사거리 계산 문제를 해결했습니다.

#### 1. Bullet 충돌 감지 문제 해결

**문제:**
- Archer 화살이 적을 관통하고 지나감
- OnTriggerEnter2D가 작동하지 않음

**원인:**
- Bullet 프리팹에 Rigidbody2D가 없음
- OnTriggerEnter2D는 Rigidbody2D가 있어야 작동

**해결:**
- `Bullet.cs`: Awake()에서 자동으로 Rigidbody2D와 CircleCollider2D 추가
  - `rb = gameObject.AddComponent<Rigidbody2D>()`
  - `rb.gravityScale = 0`, `rb.isKinematic = true`
  - `col = gameObject.AddComponent<CircleCollider2D>()`
  - `col.isTrigger = true`, `col.radius = 0.2f`

#### 2. GroundEffect 데미지 문제 해결

**문제:**
- GroundEffect가 생성되지만 적에게 데미지가 들어가지 않음
- OverlapCircleAll이 Trigger Collider를 감지하지 못함

**해결:**
- `GroundEffect.cs`: OnTriggerEnter2D/OnTriggerExit2D 사용으로 변경
  - `OnTriggerEnter2D`: 적이 GroundEffect에 들어오면 enemiesInEffect 리스트에 추가
  - `OnTriggerExit2D`: 적이 나가면 리스트에서 제거
  - `ApplyTickDamage()`: 리스트의 적들에게 주기적으로 데미지
- Awake()에서 CircleCollider2D 자동 추가

#### 3. 사거리 계산 보정

**문제:**
- 사거리 계산 결과가 너무 커서 최솟값이 8
- 유저 요청: 3 정도로 줄일 것

**해결:**
- `Unit.cs` (line 86): 사거리 계산에 보정값 추가
  - 기존: `range = data.Range * gradeRangeMultiplier`
  - 변경: `range = data.Range * gradeRangeMultiplier / 2.5f`
  - 결과: 최솟값이 8에서 약 3.2로 감소

### 커밋
- Hash: `59b44bc`
- Message: `fix: Bullet/GroundEffect collision detection and range calculation`
- Changes: Bullet.cs, GroundEffect.cs, Unit.cs 수정

### 테스트 방법
1. Unity에서 Bullet 프리팹 확인 (Rigidbody2D와 Collider2D 자동 추가됨)
2. GroundEffect 프리팹 확인 (CircleCollider2D 자동 추가됨)
3. 게임 실행 후 Archer 화살이 적에게 데미지 들어가는지 확인
4. Wizard가 GroundEffect 생성 후 적이 밟으면 데미지 들어가는지 확인
5. 사거리가 적절한지 확인 (약 3 유닛)

---

## 2024-03-14: Wizard GroundEffect 및 Laser 확산 공격 개선

### 변경 사항
테스트 완료 후 밸런스 조정 및 새로운 기능을 추가했습니다.

#### 1. Wizard GroundEffect 밸런스 조정

**변경 전:**
- 지속시간: 1.3초
- DPS: 공격력의 50%

**변경 후:**
- 지속시간: **5초** (약 4배 증가)
- DPS: 공격력의 **20%** (절반 감소)
- 총 데미지: 공격력 × 1.0 (유지)

**목적:**
- 더 오랜 시간 지속되지만 초당 데미지는 낮춰 전략적 요소 강화
- 적이 바닥을 오래 밟아야 큰 데미지를 입는 메커니즘

**수정된 파일:**
- `Assets/Scripts/DefenceGame/Core/Units/UnitAbility.cs` (line 260-262)

#### 2. Laser 등급 기반 확산(ChainAttack) 공격 추가

**새로운 기능:**
Laser 타워의 연계(확산) 공격 횟수가 등급에 따라 결정됩니다.

| 등급 | 확산 횟수 | 설명 |
|------|----------|------|
| **Common** | 0 | 확산 없음 (단일 타겟) |
| **Uncommon** | 1 | 1회 확산 (주변 1명) |
| **Rare** | 1 | 1회 확산 (주변 1명) |
| **Epic** | 2 | 2회 확산 (주변 2명) |
| **Legendary** | 3 | 3회 확산 (주변 3명) |

**구현 방식:**
- `UnitAbility.CalculateGradeBasedValues()`에 등급별 chainAttackCount 계산 로직 추가
- SpecialAbilityManager의 값과 등급 기반 값 중 큰 값을 사용
- 기존 레벨 기반 능력(3/5/7레벨)과 중첩 가능

**수정된 파일:**
- `Assets/Scripts/DefenceGame/Core/Units/UnitAbility.cs`
  - `GetGradeBasedChainAttackCount()` 메서드 추가
  - CalculateGradeBasedValues()에 Laser 로직 추가

#### 3. Enemy Prefab Collider 설정

**완료된 작업:**
- 모든 Enemy Prefab에 BoxCollider2D 또는 CircleCollider2D 추가
- Bullet/GroundEffect와의 충돌 감지 정상 작동 확인

**수정된 파일:**
- `Assets/Prefabs/Enemies/*.prefab` - Collider2D 컴포넌트 추가

### 커밋
- Hash: `7661ec5`
- Message: `feat: Update Wizard GroundEffect duration and add Laser grade-based chain attack`
- Changes: 16 files changed, 444 insertions(+), 440 deletions(-)

### 다음 단계 작업 목록
- [ ] Laser 등급별 확산 공격 테스트 (Common~Legendary)
- [ ] Wizard GroundEffect 5초 지속 및 DPS 테스트
- [ ] 전체 밸런스 테스트 및 조정
