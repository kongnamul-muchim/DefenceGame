using UnityEngine;
using UnityEngine.UI;

namespace DefenceGame.Core
{
    public class HealthBar : MonoBehaviour
    {
        [Header("UI Components")]
        public Slider healthSlider;
        public Image backgroundImage;
        public Image fillImage;
        
        [Header("Colors")]
        public Color backgroundColor = Color.white;
        public Color fillColor = new Color(0.2f, 0.8f, 0.2f); // Darker green
        
        private float maxHealth;
        
        public void Initialize(float maxHp)
        {
            maxHealth = maxHp;
            
            if (healthSlider != null)
            {
                healthSlider.maxValue = maxHealth;
                healthSlider.value = maxHealth;
            }
            
            // Apply colors
            if (backgroundImage != null)
            {
                backgroundImage.color = backgroundColor;
            }
            
            if (fillImage != null)
            {
                fillImage.color = fillColor;
            }
        }
        
        public void UpdateHealth(float currentHealth)
        {
            if (healthSlider != null)
            {
                healthSlider.value = currentHealth;
            }
        }
    }
}
