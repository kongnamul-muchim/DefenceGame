namespace DefenceGame.Data
{
    /// <summary>
    /// 등급별 배율 계산 유틸리티 클래스
    /// Common=0, Uncommon=1, Rare=2, Epic=3, Legendary=4
    /// </summary>
    public static class GradeMultiplier
    {
        // 기본 배율: Common=1.0, Uncommon=1.2, Rare=1.5, Epic=2.0, Legendary=3.0
        private static readonly float[] BaseMultipliers = { 1.0f, 1.2f, 1.5f, 2.0f, 3.0f };
        
        // 약한 배율 (Laser 연계 피해용): Common=0.7, Uncommon=0.85, Rare=1.0, Epic=1.3, Legendary=1.8
        private static readonly float[] WeakMultipliers = { 0.7f, 0.85f, 1.0f, 1.3f, 1.8f };
        
        /// <summary>
        /// 기본 배율 반환 (Common=1.0 ~ Legendary=3.0)
        /// </summary>
        public static float GetMultiplier(GradeType grade)
        {
            int index = (int)grade;
            if (index < 0 || index >= BaseMultipliers.Length)
                return 1.0f;
            return BaseMultipliers[index];
        }
        
        /// <summary>
        /// 약한 배율 반환 - 등급이 낮으면 기존보다 약함 (Laser 연계용)
        /// Common=0.7, Uncommon=0.85, Rare=1.0, Epic=1.3, Legendary=1.8
        /// </summary>
        public static float GetWeakMultiplier(GradeType grade)
        {
            int index = (int)grade;
            if (index < 0 || index >= WeakMultipliers.Length)
                return 1.0f;
            return WeakMultipliers[index];
        }
        
        /// <summary>
        /// 값에 등급 배율 적용
        /// </summary>
        public static float ApplyMultiplier(float baseValue, GradeType grade)
        {
            return baseValue * GetMultiplier(grade);
        }
        
        /// <summary>
        /// 값에 약한 등급 배율 적용 (Laser용)
        /// </summary>
        public static float ApplyWeakMultiplier(float baseValue, GradeType grade)
        {
            return baseValue * GetWeakMultiplier(grade);
        }
        
        /// <summary>
        /// 등급에 따 추가 백분율 반환 (예: Common=0%, Uncommon=20%...)
        /// </summary>
        public static float GetBonusPercent(GradeType grade)
        {
            return (GetMultiplier(grade) - 1.0f) * 100f;
        }
    }
}
