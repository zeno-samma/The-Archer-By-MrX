using OctoberStudio.Easing;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace OctoberStudio.UI
{
    public class BossfightProgressbarBehavior : MonoBehaviour
    {
        [SerializeField] protected RectMask2D rectMask;
        [SerializeField] protected CanvasGroup canvasGroup;
        [SerializeField] protected TMP_Text bossNameText;

        protected List<HealthbarBehavior> healthbars = new List<HealthbarBehavior>();

        public virtual void Show(string bossName)
        {
            bossNameText.text = bossName;

            gameObject.SetActive(true);
            canvasGroup.alpha = 0f;
            canvasGroup.DoAlpha(1f, 0.3f);
        }

        public virtual void Hide()
        {
            canvasGroup.DoAlpha(0f, 0.3f).SetOnFinish(() =>
            {
                gameObject.SetActive(false);
                Clear();
            });
        }

        public virtual void AddHealthbar(HealthbarBehavior healthbar)
        {
            healthbars.Add(healthbar);
        }

        private void Update()
        {
            if (healthbars.Count > 0)
            {
                var maxHP = 0f;
                var hp = 0f;
                for (int i = 0; i < healthbars.Count; i++)
                {
                    var healthbar = healthbars[i];

                    maxHP += healthbar.MaxHP;
                    hp += healthbar.HP;
                }

                if (maxHP == 0)
                {
                    SetProgress(0);
                }
                else
                {
                    SetProgress(hp / maxHP);
                }
            }
        }

        protected virtual void Clear()
        {
            healthbars.Clear();
        }

        protected virtual void SetProgress(float progress)
        {
            Vector4 padding = rectMask.padding;
            padding.z = rectMask.rectTransform.rect.width * (1 - progress);
            rectMask.padding = padding;
        }
    }
}