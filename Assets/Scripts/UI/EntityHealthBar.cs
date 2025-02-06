using UnityEngine;

using UZSG.Entities;

using UZSG.UI;

namespace UZSG
{
    public class EntityHealthBar : MonoBehaviour
    {
        /// <summary>
        /// Health bars are visible to the Player, hence referenced.
        /// </summary>
        public Player Player { get; set; }
        /// <summary>
        /// The entity this HealthBar is attached to.
        /// </summary>
        Entity target;
        /// <summary>
        /// The actual Health Bar UI component.
        /// </summary>
        [SerializeField] EntityHealthBarUI healthBarUI;

        void Awake()
        {
            target = GetComponentInParent<Entity>();
        }

        void Update()
        {
            if (healthBarUI.IsVisible)
            {
                var screenPosition = Player.MainCamera.WorldToScreenPoint(transform.position);
                if (screenPosition.z > 0)
                {
                    healthBarUI.Rect.anchoredPosition = screenPosition;
                    healthBarUI.Show();
                }
                else
                {
                    healthBarUI.Hide();
                }
            }
        }

        public void Initialize()
        {
            healthBarUI ??= Game.UI.Create<EntityHealthBarUI>("Entity Health Bar (UI)", parent: Game.UI.HealthBarCanvas.transform);
            healthBarUI.Hide();

            if (target.Attributes.TryGet("health", out var health))
            {
                healthBarUI.BindAttribute(health);
            }
            if (target.Attributes.TryGet("level", out var level))
            {
                healthBarUI.Level = Mathf.FloorToInt(level.Value);
            }
        }

        public void Show()
        {
            healthBarUI.Show();
        }

        public void Hide()
        {
            healthBarUI.Hide();
        }
    }
}