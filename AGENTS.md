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
