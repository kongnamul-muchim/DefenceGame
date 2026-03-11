# DefenceGame 작업 로그

## 2024-03-11: 특수능력 시스템 재설계

### 변경 사항
기존 특수능력을 완전히 새로 기획하여 변경했습니다.

#### 능력 변경표

| 타워 | 기존 능력 | 새로운 능력 | 설명 |
|------|----------|------------|------|
| **Archer** | 관통 (Pierce) | **투사체 개수증가 (MultiShot)** | 레벨 3: 2개, 5: 3개, 7: 4개 발사 |
| **Mage** | 광역 (AreaDamage) | **지속피해 바닥 (GroundEffect)** | 레벨 3: 3초, 5: 5초, 7: 7초 지속 |
| **MageTower** | 버프/디버프 | **사거리증가 + 공격력증가** | 레벨 3: 사거리+1, 5: 공격력30%, 7: 사거리+2 |
| **Laser** | 사거리/관통 | **연계공격 (ChainAttack)** | 레벨 3: 1회, 5: 2회, 7: 3회 주변 전이 |

#### 수정된 파일
- `Assets/Scripts/DefenceGame/Data/SpecialAbilityData.cs` - 새로운 능력 타입 enum 추가
- `Assets/Scripts/DefenceGame/Core/SpecialAbilityManager.cs` - 능력 데이터 재설정
- `Assets/Scripts/DefenceGame/Core/Unit.cs` - 새로운 능력 로직 구현
  - MultiShot: 주변 적에게 다중 발사
  - GroundEffect: 공격 시 지속 데미지 바닥 생성
  - ChainAttack: 적중 시 주변 적에게 피해 전이
- `Assets/Scripts/DefenceGame/Core/Enemy.cs` - 둔화 효과 버그 수정 (고정값 -> 실제 값 저장)

#### 새로 생성된 파일
- `Assets/Scripts/DefenceGame/Core/GroundEffect.cs` - 지속 피해 바닥 효과 컴포넌트

### 커밋
- Hash: `44317b1`
- Message: `feat: redesign special abilities system`

### 다음 단계 작업 목록
- [ ] GroundEffect 프리팹 생성 및 Unit에 연결
- [ ] 새로운 능력 테스트
- [ ] 밸런스 조정 (데미지, 지속시간 등)
