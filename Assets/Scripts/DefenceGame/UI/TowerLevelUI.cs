using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DefenceGame.Core;
using DefenceGame.Data;

namespace DefenceGame.UI
{
    public class TowerLevelUI : MonoBehaviour
    {
        [System.Serializable]
        public class TowerSlot
        {
            public string towerType;
            public TextMeshProUGUI levelText;
            public Slider expSlider;
            public Image backgroundImage;
            public Image fillImage;
            public Image panelImage; // 슬롯 전체 Panel의 Image (색상 변경용)
        }
        
        [Header("UI Settings")]
        public TowerSlot[] towerSlots = new TowerSlot[4];
        
        [Header("Colors")]
        public Color backgroundColor = new Color(1, 1, 1, 0.3f); // 흰색, 투명도 30%
        public Color fillColor = Color.green;
        public Color[] levelColors = new Color[4]
        {
            new Color(0.5f, 0.5f, 0.5f, 0.3f),  // Lv.0-2: 회색, 투명
            new Color(1, 1, 1, 0.3f),            // Lv.3-4: 흰색, 투명
            new Color(0, 1, 0, 0.3f),            // Lv.5-6: 녹색, 투명
            new Color(1, 1, 0, 0.3f)             // Lv.7+: 노란색, 투명
        };
        
        private void Start()
        {
            InitializeUI();
            
            // Subscribe to events
            if (TowerLevelManager.Instance != null)
            {
                TowerLevelManager.Instance.OnTowerExpChanged += OnTowerExpChanged;
                TowerLevelManager.Instance.OnTowerLevelUp += OnTowerLevelUp;
            }
        }
        
        private void OnDestroy()
        {
            // Unsubscribe from events
            if (TowerLevelManager.Instance != null)
            {
                TowerLevelManager.Instance.OnTowerExpChanged -= OnTowerExpChanged;
                TowerLevelManager.Instance.OnTowerLevelUp -= OnTowerLevelUp;
            }
        }
        
        private void InitializeUI()
        {
            // Initialize all tower slots
            for (int i = 0; i < towerSlots.Length; i++)
            {
                if (i < TowerLevelManager.Instance.towerTypes.Length)
                {
                    towerSlots[i].towerType = TowerLevelManager.Instance.towerTypes[i];
                    UpdateSlotUI(i);
                }
            }
        }
        
        private void OnTowerExpChanged(string towerType)
        {
            // Find the slot and update it
            for (int i = 0; i < towerSlots.Length; i++)
            {
                if (towerSlots[i].towerType == towerType)
                {
                    UpdateSlotUI(i);
                    break;
                }
            }
        }
        
        private void OnTowerLevelUp(string towerType, int newLevel)
        {
            // Find the slot and update it
            for (int i = 0; i < towerSlots.Length; i++)
            {
                if (towerSlots[i].towerType == towerType)
                {
                    UpdateSlotUI(i);
                    break;
                }
            }
        }
        
        private void UpdateSlotUI(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= towerSlots.Length) return;
            
            TowerSlot slot = towerSlots[slotIndex];
            if (TowerLevelManager.Instance == null) return;
            
            TowerLevelData data = TowerLevelManager.Instance.GetTowerLevelData(slot.towerType);
            
            // Update level text
            if (slot.levelText != null)
            {
                slot.levelText.text = $"Lv.{data.level}";
            }
            
            // Update exp slider
            if (slot.expSlider != null)
            {
                slot.expSlider.value = data.GetExpPercentage();
            }
            
            // Update colors based on level
            Color bgColor = GetLevelColor(data.level);
            if (slot.backgroundImage != null)
            {
                slot.backgroundImage.color = bgColor;
            }
            
            if (slot.fillImage != null)
            {
                slot.fillImage.color = fillColor;
            }
            
            // Update Panel color (슬롯 전체 배경)
            if (slot.panelImage != null)
            {
                slot.panelImage.color = bgColor;
            }
        }
        
        private Color GetLevelColor(int level)
        {
            if (level <= 2)
                return levelColors[0]; // Gray
            else if (level <= 4)
                return levelColors[1]; // White
            else if (level <= 6)
                return levelColors[2]; // Green
            else
                return levelColors[3]; // Yellow
        }
        
        private void Update()
        {
            // Update all slots (for initialization and runtime updates)
            for (int i = 0; i < towerSlots.Length; i++)
            {
                UpdateSlotUI(i);
            }
        }
    }
}
