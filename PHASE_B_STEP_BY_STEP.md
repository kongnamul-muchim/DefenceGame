# Phase B 단계별 진행 Tasklist

> ⚠️ **중요**: 각 단계를 완료하고 **반드시 테스트**한 후 다음 단계로 진행하세요!

---

## 📋 준비 단계 (Phase B 시작 전)

### Pre-check
- [ ] Unity Editor 실행
- [ ] 게임 정상 작동 확인 (SpaceBar 가챠, 드래그, 합성)
- [ ] Console 창 열어놓기 (Ctrl+Shift+C)
- [ ] Git 백업 또는 현재 상태 저장

---

# 🔷 STEP 1: Enemy 체력바 시스템
**목표**: Enemy 머리 위에 체력바 표시

## 1-1. HealthBar 스크립트 생성
**파일**: `Assets/Scripts/DefenceGame/Core/HealthBar.cs`

작업 내용:
- [ ] World Space Canvas 생성
- [ ] Slider UI 설정 (흰색 BG + 녹색 Fill)
- [ ] Enemy 머리 위 위치 설정 (Y +0.8f)

## 1-2. Enemy.cs 수정
작업 내용:
- [ ] HealthBar 필드 추가
- [ ] Initialize()에서 HealthBar 생성 호출
- [ ] TakeDamage()에서 체력바 업데이트

## 1-3. Unity 설정
- [ ] 아무 작업 필요 없음 (스크립트가 자동으로 Canvas 생성)

## 1-4. 테스트
- [ ] ▶️ Play 버튼 클릭
- [ ] Enemy 스폰 확인
- [ ] Enemy 머리 위에 흰색 사각형(체력바 배경) 확인
- [ ] Enemy 머리 위에 녹색 바(현재 체력) 확인
- [ ] 유닛이 Enemy 공격 시 녹색 바가 줄어드는지 확인
- [ ] Console에 에러 없는지 확인

## 1-5. 문제 해결
**체력바가 보이지 않음:**
- Enemy 프리팹에 SpriteRenderer 있는지 확인
- Canvas가 World Space 모드인지 확인

**체력바가 회전함:**
- LateUpdate()에서 Camera 방향으로 회전하는 코드 확인

---

**✅ STEP 1 완료 확인:**
- [ ] Enemy마다 머리 위에 체력바가 표시됨
- [ ] 공격받을 때 체력바가 실시간으로 감소함
- [ ] 에러 없이 정상 작동함

**다음 단계로 이동**: STEP 2

---

# 🔷 STEP 2: Bullet 히트 이펙트
**목표**: Bullet이 Enemy에 닿으면 파티클 이펙트 생성

## 2-1. Bullet.cs 수정
작업 내용:
- [ ] `hitEffectPrefab` 필드 추가
- [ ] `OnTriggerEnter2D()`에서 이펙트 생성
- [ ] 2초 후 자동 삭제 코드 추가

## 2-2. Unity 설정 (중요!)
**위치**: Project 창 → Bullet 프리팹 선택

- [ ] Inspector에서 Bullet 컴포넌트 확인
- [ ] **Hit Effect Prefet** 필드에 ParticleSystem 프리팹 연결
  - 추천: `Assets/JMO Assets/Cartoon FX Remaster/CFXR Prefabs/`
  - 사용 가능한 이펙트:
    - `CFXR2 Ground Hit`
    - `CFXR Hit Crit`
    - `CFXR Magic Poof`
- [ ] **Apply** 버튼 클릭

## 2-3. 테스트
- [ ] ▶️ Play 버튼 클릭
- [ ] 유닛이 원거리 공격 시 Bullet 발사 확인
- [ ] Bullet이 Enemy에 닿으면 **이펙트(폭발/스파크 등)** 생성 확인
- [ ] 이펙트가 2초 후 사라지는지 확인
- [ ] Console에 에러 없는지 확인

## 2-4. 문제 해결
**이펙트가 안 보임:**
- Hit Effect Prefet 필드에 프리팹이 연결되어 있는지 확인
- 이펙트 프리팹에 ParticleSystem 컴포넌트 있는지 확인
- 이펙트 크기가 너무 작은지 확인 (Scale 확인)

---

**✅ STEP 2 완료 확인:**
- [ ] Bullet이 Enemy에 닿으면 이펙트가 보임
- [ ] 이펙트가 자동으로 사라짐
- [ ] 에러 없이 정상 작동함

**다음 단계로 이동**: STEP 3

---

# 🔷 STEP 3: 타워 레벨/경험치 시스템 (기본)
**목표**: 합성 시 경험치 획득 및 레벨업

## 3-1. 데이터 파일 생성
**파일**: `Assets/Scripts/DefenceGame/Data/TowerLevelData.cs`

작업 내용:
- [ ] `TowerLevelData` 클래스 생성
- [ ] 레벨, 현재 경험치, 다음 레벨 필요 경험치 필드
- [ ] 경험치 계산 공식: `100 + (currentLevel × 50)`
- [ ] 레벨업 체크 메서드 (초과 경험치 이월 지원)

## 3-2. 매니저 생성
**파일**: `Assets/Scripts/DefenceGame/Core/TowerLevelManager.cs`

작업 내용:
- [ ] Singleton 패턴 구현
- [ ] 타워별 레벨 데이터 저장 (Dictionary)
- [ ] `AddExp()` 메서드 구현
  - Common: +50 EXP
  - Uncommon: +100 EXP
  - Rare: +150 EXP
  - Epic: +200 EXP
  - Legendary: +0 EXP
- [ ] 레벨업 이벤트 발생

## 3-3. Unity 설정
**위치**: Hierarchy → Managers 오브젝트

- [ ] Managers 오브젝트 선택
- [ ] Inspector에서 **Add Component** 클릭
- [ ] "TowerLevelManager" 검색 후 추가
- [ ] Tower Types 필드 확인:
  - Element 0: Archer
  - Element 1: Wizard
  - Element 2: WizardTower
  - Element 3: Laser

## 3-4. 합성 연동
**파일**: `Assets/Scripts/DefenceGame/Core/UnitMergeManager.cs`

작업 내용:
- [ ] `MergeUnits()`에 경험치 획득 코드 추가
- [ ] 합성 직후 `TowerLevelManager.Instance.AddExp()` 호출

## 3-5. 테스트
- [ ] ▶️ Play 버튼 클릭
- [ ] SpaceBar로 Archer 유닛 2개 가챠
- [ ] 2개 Archer 드래그하여 합성
- [ ] Console에 "Added 50 exp to Archer" 로그 확인
- [ ] 계속 합성하여 Archer 레벨이 오르는지 확인
- [ ] Console에 "Tower Archer leveled up to level X" 로그 확인
- [ ] Console에 에러 없는지 확인

## 3-6. 문제 해결
**경험치가 증가하지 않음:**
- TowerLevelManager가 Managers 오브젝트에 있는지 확인
- UnitMergeManager.cs에 using DefenceGame.Core; 추가 확인
- 합성이 실제로 실행되는지 확인 (로그 확인)

---

**✅ STEP 3 완료 확인:**
- [ ] 합성 시 경험치가 증가함 (Console 로그 확인)
- [ ] 일정 경험치 이상 시 레벨업함
- [ ] 에러 없이 정상 작동함

**다음 단계로 이동**: STEP 4

---

# 🔷 STEP 4: 타워 레벨 UI
**목표**: 화면에 각 타워의 레벨과 경험치 표시

## 4-1. TowerLevelUI 스크립트 생성
**파일**: `Assets/Scripts/DefenceGame/UI/TowerLevelUI.cs`

작업 내용:
- [ ] 4개 타워 슬롯 데이터 구조 정의
- [ ] 타워별 레벨/경험치 표시 로직
- [ ] 레벨업 시 UI 업데이트 이벤트 연결
- [ ] 레벨별 색상 변경 (회색→하얀색→연두색→노란색)

## 4-2. Unity UI 설정 (중요!)

### 4-2-1. Canvas 생성
- [ ] Hierarchy 우클릭 → UI → Canvas
- [ ] 이름: "TowerLevelCanvas"
- [ ] Render Mode: **Screen Space - Overlay**
- [ ] Canvas Scaler: Scale With Screen Size
- [ ] Reference Resolution: 1920 x 1080

### 4-2-2. 타워 슬롯 4개 생성
**각 슬롯마다 반복:**

**Archer 슬롯:**
- [ ] Hierarchy 우클릭 → UI → Panel
- [ ] 이름: "ArcherSlot"
- [ ] Rect Transform 설정:
  - Anchor: **Top Right**
  - Pos X: -120, Pos Y: -50
  - Width: 200, Height: 60
- [ ] Image 컴포넌트 → Color → **Alpha: 0.3** (반투명)

**Archer 슬롯 자식 오브젝트:**
1. **이름 텍스트**
   - [ ] ArcherSlot 우클릭 → UI → Text - TextMeshPro
   - [ ] 이름: "NameText"
   - [ ] 텍스트: "Archer"
   - [ ] 위치: 슬롯 상단 중앙

2. **레벨 텍스트**
   - [ ] ArcherSlot 우클릭 → UI → Text - TextMeshPro
   - [ ] 이름: "LevelText"
   - [ ] 텍스트: "Lv.0"
   - [ ] 위치: 슬롯 좌측

3. **경험치 슬라이더**
   - [ ] ArcherSlot 우클릭 → UI → Slider
   - [ ] 이름: "ExpSlider"
   - [ ] 배경: 흰색
   - [ ] Fill: 녹색
   - [ ] 위치: 슬롯 하단
   - [ ] Width: 180

**Wizard 슬롯:**
- [ ] 동일하게 "WizardSlot" 생성 (Pos Y: -130)

**WizardTower 슬롯:**
- [ ] 동일하게 "WizardTowerSlot" 생성 (Pos Y: -210)

**Laser 슬롯:**
- [ ] 동일하게 "LaserSlot" 생성 (Pos Y: -290)

### 4-2-3. 스크립트 연결
- [ ] TowerLevelCanvas 선택
- [ ] Inspector → Add Component → "TowerLevelUI"
- [ ] Tower Slots 배열 크기: 4
- [ ] 각 슬롯 연결:
  - Element 0: ArcherSlot 연결 (Tower Type: "Archer")
  - Element 1: WizardSlot 연결 (Tower Type: "Wizard")
  - Element 2: WizardTowerSlot 연결 (Tower Type: "WizardTower")
  - Element 3: LaserSlot 연결 (Tower Type: "Laser")
- [ ] 각 슬롯의 UI 요소 연결:
  - Level Text: LevelText 오브젝트
  - Exp Slider: ExpSlider 오브젝트
  - Background Image: Slot의 Image 컴포넌트
  - Fill Image: Slider의 Fill Image

## 4-3. 테스트
- [ ] ▶️ Play 버튼 클릭
- [ ] 화면 우측 상단에 4개 슬롯이 보이는지 확인
- [ ] Archer 유닛 2개 합성
- [ ] ArcherSlot의 경험치 바가 증가하는지 확인
- [ ] 계속 합성하여 레벨업 시 "Lv.1"로 변경되는지 확인
- [ ] 레벨 3 도달 시 슬롯 배경이 흰색으로 변경되는지 확인
- [ ] Console에 에러 없는지 확인

## 4-4. 문제 해결
**슬롯이 보이지 않음:**
- Canvas의 Render Mode 확인 (Screen Space - Overlay)
- 슬롯의 색상 Alpha 값 확인 (0이면 완전 투명)
- 슬롯 위치가 화면 안에 있는지 확인

**경험치 바가 업데이트되지 않음:**
- TowerLevelUI 스크립트의 이벤트 연결 확인
- Slider의 Value 범위 확인 (0~1)

---

**✅ STEP 4 완료 확인:**
- [ ] 화면 우측에 4개 타워 슬롯이 표시됨
- [ ] 합성 시 경험치 바가 증가함
- [ ] 레벨업 시 레벨 텍스트가 변경됨
- [ ] 에러 없이 정상 작동함

**다음 단계로 이동**: STEP 5

---

# 🔷 STEP 5: 특수 능력 해금 시스템
**목표**: 레벨 3/5/7 도달 시 특수 능력 해금

## 5-1. 데이터 파일 생성 ✅ 완료
**파일**: `Assets/Scripts/DefenceGame/Data/SpecialAbilityData.cs`

작업 내용:
- [x] `SpecialAbilityType` Enum 정의 (Pierce, AreaDamage 등)
- [x] `SpecialAbility` 클래스 정의
- [x] 각 타워별 특수 능력 데이터 정의

## 5-2. 매니저 생성 ✅ 완료
**파일**: `Assets/Scripts/DefenceGame/Core/SpecialAbilityManager.cs`

작업 내용:
- [x] Singleton 패턴 구현
- [x] 타워별 특수 능력 목록 저장
- [x] Archer: 관통(3), 관통2(5), 공격속도20%(7)
- [x] Mage: 광역1(3), 광역2(5), 공격력30%(7)
- [x] MageTower: 공격력버프10%(3), 이동속도30%감소(5), 범위+1(7)
- [x] Laser: 사거리+1(3), 사거리+2(5), 관통(7)
- [x] 레벨에 따른 능력 해금 메서드

## 5-3. Unit.cs 수정 ✅ 완료
**파일**: `Assets/Scripts/DefenceGame/Core/Unit.cs`

작업 내용:
- [x] 특수 능력 필드 추가 (pierceCount, areaDamageRadius 등)
- [x] `Initialize()`에 특수 능력 적용 로직 추가
- [x] `ApplySpecialAbilities()` 메서드 구현
- [x] 레벨업 이벤트 수신하여 능력 재적용

## 5-4. Bullet.cs 수정 ✅ 완료
**파일**: `Assets/Scripts/DefenceGame/Core/Bullet.cs`

작업 내용:
- [x] `Initialize()`에 `pierce` 파라미터 추가
- [x] 관통 로직 구현 (pierceCount만큼 적 관통)
- [x] `FindNextTarget()` 메서드 구현

## 5-5. Unity 설정 ⚠️ 필요
**위치**: Hierarchy → Managers 오브젝트

- [ ] Managers 오브젝트 선택
- [ ] Inspector에서 **Add Component** 클릭
- [ ] "SpecialAbilityManager" 검색 후 추가

## 5-6. 테스트
- [ ] ▶️ Play 버튼 클릭
- [ ] Archer만 계속 합성하여 레벨 3 달성
- [ ] Console에 특수 능력 해금 로그 확인
- [ ] Archer가 관통 공격 하는지 확인 (한 번에 2명 공격)
- [ ] Wizard 레벨 3: 광역 공격 확인
- [ ] Laser 레벨 3: 사거리 증가 확인
- [ ] Console에 에러 없는지 확인

## 5-7. 문제 해결
**능력이 적용되지 않음:**
- SpecialAbilityManager가 Managers 오브젝트에 있는지 확인
- Unit.cs에 using DefenceGame.Data; 추가 확인
- Unit Initialize()가 호출되는지 확인

**관통이 안 됨:**
- Bullet Initialize()에 pierce 파라미터 전달 확인
- pierceCount 값 확인

---

**✅ STEP 5 완료 확인:**
- [x] SpecialAbilityData.cs 생성됨
- [x] SpecialAbilityManager.cs 생성됨
- [x] Unit.cs에 특수 능력 적용 로직 추가됨
- [x] Bullet.cs에 관통 기능 구현됨
- [ ] Unity에서 SpecialAbilityManager 컴포넌트 추가 필요

---

# 🎉 Phase B 전체 완료!

**최종 확인 사항:**
- [ ] Enemy 체력바 시스템: ✅
- [ ] Bullet 히트 이펙트: ✅
- [ ] 타워 레벨/경험치 시스템: ✅
- [ ] 타워 레벨 UI: ✅
- [ ] 특수 능력 해금 시스템: ✅

**모든 단계를 완료했습니다!**
