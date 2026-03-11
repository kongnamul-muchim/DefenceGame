# DefenceGame Phase B Step 5 진행 상황

**최종 업데이트**: 2026-03-11
**현재 브랜치**: master
**최신 커밋**: `c189c1e` - Fix level up event handling and add OnTowerExpChanged event

---

## 📋 현재 진행 중인 작업

### Phase B Step 5: 특수 능력 해금 시스템
**상태**: 🚧 진행 중 (버그 수정 중)

**구현된 기능**:
- [x] SpecialAbilityData.cs - 능력 타입 Enum (SlowEffect 추가)
- [x] SpecialAbilityManager.cs - 타워별 특수 능력 관리
- [x] Bullet.cs - 관통 기능 및 광역 공격 시각화
- [x] Enemy.cs - 느려짐 효과 메서드
- [x] Unit.cs - 특수 능력 적용 로직
- [x] TowerLevelUI.cs - 클릭으로 레벨업 기능

**타워별 특수 능력**:
- **Archer**: Lv.3 관통1, Lv.5 관통2, Lv.7 공격속도20%
- **Mage**: Lv.3 광역1칸, Lv.5 광역2칸, Lv.7 공격력30%
- **MageTower**: Lv.3 공격력버프10%, Lv.5 이동속도30%감소, Lv.7 범위+1
- **Laser**: Lv.3 사거리+1, Lv.5 사거리+2, Lv.7 관통

---

## ⚠️ 알려진 문제

### 1. 레벨이 0으로 표시됨 (🔴 심각)
**증상**:
```
[WizardTower] Current level for MageTower: 0
[WizardTower] Found 0 unlocked abilities for MageTower at level 0
```

**원인 분석**:
- TowerLevelManager는 별도의 데이터 저장소를 사용
- 유닛이 생성될 때 현재 레벨을 제대로 가져오지 못함
- 이벤트 구독은 되어 있으나 초기 레벨 동기화 문제

**시도한 해결책**:
- [x] 타워 이름 매핑 수정 (WizardTower → MageTower)
- [x] 이벤트 구독 추가 (OnTowerExpChanged)
- [x] Initialize 시점에서 레벨 확인
- [ ] **미해결**: 타이밍 문제로 추정

**다음 시도**:
1. TowerLevelManager.InitializeTowerLevels 확인
2. Awake/Start 시점에서의 데이터 동기화 확인
3. TowerLevelData가 제대로 생성되는지 확인

### 2. MageTower 버프 범위 표시 안됨 (🟡 중간)
**증상**: 마우스 호버 시 주황색 사각형이 보이지 않음

**원인**:
- LineRenderer 설정은 완료됨
- 다만 Initialize 시점에서 buffRangeRenderer가 null일 수 있음

**상태**: 수정 중

### 3. 느려짐 효과 미적용 (🟡 중간)
**증상**: 적이 파란색으로 변하지 않고 속도가 줄지 않음

**원인**:
- 레벨이 0이어서 slowEffectValue가 0임
- 레벨 문제 해결 시 자동으로 해결될 것으로 예상

---

## 🔧 수정된 파일 목록

### 완료된 수정
1. **SpecialAbilityData.cs** - SlowEffect 타입 추가
2. **SpecialAbilityManager.cs** - MageTower 능력 변경 (SpeedBuff → SlowEffect)
3. **Unit.cs** - 타워 타입 추출 로직 수정 (WizardTower → MageTower 매핑)
4. **Unit.cs** - NullReferenceException 수정
5. **Unit.cs** - 이벤트 구독 개선 (OnTowerExpChanged 추가)
6. **Bullet.cs** - 광역 공격 범위 확대 (1.5f → 2.5f)
7. **Enemy.cs** - 느려짐 효과 메서드 추가

### 진행 중인 수정
- **Unit.cs** - 레벨 동기화 문제 해결 중

---

## 📝 다음 작업 목록

### 우선순위 1 (필수)
- [ ] 레벨 0 문제 해결
  - TowerLevelManager.InitializeTowerLevels() 확인
  - TowerLevelData 생성 시점 확인
  - Awake/Start 순서 확인
- [ ] MageTower 버프 범위 시각화 확인
  - LineRenderer 설정 확인
  - 마우스 호버 이벤트 확인

### 우선순위 2 (기능 개선)
- [ ] 느려짐 효과 테스트
- [ ] 광역 공격 테스트
- [ ] 관통 공격 테스트

### 우선순위 3 (마무리)
- [ ] 디버그 로그 정리
- [ ] 최종 테스트
- [ ] Phase B 완료 문서화

---

## 🎮 테스트 방법

### MageTower 테스트
1. Unity Editor 실행
2. Hierarchy → Managers 오브젝트 선택
3. TowerLevelManager 컴포넌트 확인 (towerTypes 배열 확인)
4. Play 버튼 클릭
5. UI에서 MageTower 슬롯 클릭 (여러 번 클릭하여 레벨업)
6. SpaceBar로 가챠 실행하여 WizardTower 소환
7. Console에서 다음 로그 확인:
   ```
   Tower MageTower leveled up to level X!
   [WizardTower] Level up event received: MageTower -> level X
   [WizardTower] Buff range visualization setup complete
   ```

### 문제 발생 시 확인 사항
- Managers 오브젝트에 TowerLevelManager 있는지 확인
- Managers 오브젝트에 SpecialAbilityManager 있는지 확인
- Console에 빨간 에러가 뜨는지 확인
- TowerLevelUI가 Scene에 있는지 확인

---

## 📁 관련 파일 경로

```
Assets/Scripts/DefenceGame/Core/
├── Unit.cs                    # 특수 능력 적용 (수정 중)
├── Bullet.cs                  # 광역/관통 공격
├── Enemy.cs                   # 느려짐 효과
├── TowerLevelManager.cs       # 레벨 관리
├── SpecialAbilityManager.cs   # 능력 관리
└── UnitDragSystem.cs          # 드래그 시스템

Assets/Scripts/DefenceGame/Data/
├── SpecialAbilityData.cs      # 능력 타입 정의
├── TowerLevelData.cs          # 레벨 데이터 구조
└── TowerData.cs               # 타워 데이터

Assets/Scripts/DefenceGame/UI/
└── TowerLevelUI.cs            # 레벨 UI 및 테스트용 클릭 기능
```

---

## 💡 참고 사항

### 타워 이름 매핑
- **엑셀/Prefab**: WizardTower, Wizard
- **코드 내부**: MageTower, Mage
- **매핑 로직**: `ExtractTowerType()` 메서드에서 처리

### 레벨업 시스템 흐름
```
[UI 클릭] → TowerLevelUI.OnSlotClicked()
                ↓
    TowerLevelManager.AddExp("MageTower", Epic)
                ↓
    TowerLevelData.AddExp(200) → Level Up
                ↓
    OnTowerLevelUp?.Invoke("MageTower", newLevel)
                ↓
    Unit.OnTowerLevelUp() → ApplySpecialAbilities()
```

### 현재 블로킹 이슈
레벨이 0으로 표시되는 문제로 인해 모든 특수 능력이 적용되지 않음.
이 문제 해결 시 나머지 기능은 정상 작동할 것으로 예상.

---

## 🔍 디버깅 팁

### 유용한 로그 검색어
- `[ApplySpecialAbilities]` - 특수 능력 적용 시작
- `[WizardTower] Current level` - 현재 레벨 확인
- `[WizardTower] Found X unlocked abilities` - 해금된 능력 수
- `Tower MageTower leveled up` - 레벨업 이벤트
- `[SetupBuffRange]` - 버프 범위 시각화 설정

### 확인해야 할 Inspector 설정
1. **Managers 오브젝트**:
   - TowerLevelManager: towerTypes = ["Archer", "Mage", "MageTower", "Laser"]
   - SpecialAbilityManager: 컴포넌트 추가됨

2. **TowerLevelCanvas**:
   - TowerLevelUI 컴포넌트 추가됨
   - Tower Slots 배열에 4개 슬롯 연결됨

3. **WizardTower 프리팹**:
   - Unit 컴포넌트 추가됨
   - SpriteRenderer 있음
