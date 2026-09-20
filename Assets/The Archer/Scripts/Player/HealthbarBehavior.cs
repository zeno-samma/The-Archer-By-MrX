using OctoberStudio.Easing;
using OctoberStudio.Extensions;
using TMPro;
using UnityEngine;

namespace OctoberStudio
{
    public class HealthbarBehavior : MonoBehaviour
    {
        [SerializeField] protected SpriteGroup spriteGroup;

        [Space]
        [SerializeField] protected Transform maskTransform;
        [SerializeField] protected Transform whiteMaskTransform;
        [SerializeField] protected Transform invincibilityMaskTransform;

        [Space]
        [SerializeField] protected float maskMaxPosition;
        [SerializeField] protected float maskMaxScale;

        [Space]
        [SerializeField] protected float invincibilityMaxPosition;
        [SerializeField] protected float invincibilityMaxScale;

        [Space]
        [SerializeField] protected TMP_Text hpText;

        [Space]
        [SerializeField] protected float whiteMaskDuration = 0.3f;
        [SerializeField] protected AnimationCurve whiteMaskCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

        public float MaxHP { get; protected set; }
        public float HP { get; protected set; }

        public bool IsZero => HP <= 0;
        public bool IsMax => HP >= MaxHP;

        protected bool autoShowOnChaned;
        protected bool autoHideWhenMax;

        protected IEasingCoroutine showHideCoroutine;
        protected IEasingCoroutine whiteMaskCoroutine;

        protected bool isShown = false;

        protected float maskT;

        protected InvincibilityData invincibilityData;

        public virtual void Init(float maxHP)
        {
            MaxHP = maxHP;
            HP = MaxHP;

            maskT = 1f;

            Redraw();
        }

        protected virtual void LateUpdate()
        {
            transform.forward = Camera.main.transform.forward;

            if (whiteMaskCoroutine.ExistsAndActive() || invincibilityData != null)
            {
                Redraw();
            }
        }

        public virtual void SetInvincibilityData(InvincibilityData data)
        {
            invincibilityData = data;

            Redraw();
        }

        public virtual void SetAutoShowOnChanged(bool value)
        {
            autoShowOnChaned = value;
        }

        public virtual void SetAutoHideWhenMax(bool value)
        {
            autoHideWhenMax = value;
            if (HP == MaxHP) ForceHide();
        }

        public virtual int AddHP(float value)
        {
            if (value < 0)
            {
                return Subtract(-value);
            }

            var displayedHP = Mathf.CeilToInt(HP);

            HP += value;
            if (HP > MaxHP)
            {
                HP = MaxHP;
                if (autoHideWhenMax) Hide();
            }

            var addedHP = Mathf.CeilToInt(HP) - displayedHP;

            whiteMaskCoroutine.StopIfExists();
            whiteMaskCoroutine = EasingManager.DoFloat(maskT, HP / MaxHP, whiteMaskDuration, (value) => maskT = value).SetEasingCurve(whiteMaskCurve).SetOnFinish(Redraw);

            Redraw();

            return addedHP;
        }

        public virtual int AddPercentage(float percent)
        {
            var hpAmount = MaxHP * percent / 100f;

            return AddHP(hpAmount);
        }

        public virtual int Subtract(float value)
        {
            if (value < 0)
            {
                return AddHP(-value);
            }

            var displayedHP = Mathf.CeilToInt(HP);

            HP -= value;

            whiteMaskCoroutine.StopIfExists();

            if (HP <= 0)
            {
                HP = 0;
                Hide();
            }
            else
            {
                if (autoShowOnChaned && !isShown) Show();

                whiteMaskCoroutine.StopIfExists();
                whiteMaskCoroutine = EasingManager.DoFloat(maskT, HP / MaxHP, whiteMaskDuration, (value) => maskT = value).SetEasingCurve(whiteMaskCurve).SetOnFinish(Redraw);

                Redraw();
            }

            var subtructedHP = displayedHP - Mathf.CeilToInt(HP);

            return subtructedHP;
        }

        public virtual void ResetHP(float duration = 0)
        {
            maskT = 1;

            if (duration > 0)
            {
                EasingManager.DoFloat(0, MaxHP, duration, (hp) =>
                {
                    HP = hp;
                    Redraw();
                });

                Show();
            }
            else
            {
                HP = MaxHP;
                Redraw();
            }
        }

        public virtual int ChangeMaxHP(float newMaxHP, bool scaleHP = true)
        {
            var oldMaxHP = MaxHP;
            MaxHP = newMaxHP;

            var addedHP = 0;
            if (scaleHP)
            {
                var scaleFactor = newMaxHP / oldMaxHP;

                var newHP = HP * scaleFactor;

                addedHP = AddHP(newHP - HP);
            }
            else
            {
                Redraw();
            }

            return addedHP;
        }

        public virtual void Redraw()
        {
            var t = HP / MaxHP;
            maskTransform.localPosition = Vector3.left * maskMaxPosition * (1 - t);
            maskTransform.localScale = maskTransform.localScale.SetX(maskMaxScale * t);

            whiteMaskTransform.localPosition = Vector3.left * maskMaxPosition * (1 - maskT);
            whiteMaskTransform.localScale = whiteMaskTransform.localScale.SetX(maskMaxScale * maskT);

            if (hpText != null)
            {
                hpText.text = Mathf.CeilToInt(HP).ToString();
            }

            if (invincibilityMaskTransform != null)
            {
                if (invincibilityData != null)
                {
                    if (invincibilityData.duration <= 0)
                    {
                        invincibilityMaskTransform.localPosition = Vector3.zero;
                        invincibilityMaskTransform.localScale = maskTransform.localScale.SetX(invincibilityMaxScale);
                    }
                    else
                    {
                        var time = Time.time - invincibilityData.startTime;
                        t = time / invincibilityData.duration;

                        invincibilityMaskTransform.localPosition = Vector3.left * invincibilityMaxPosition * (t);
                        invincibilityMaskTransform.localScale = maskTransform.localScale.SetX(invincibilityMaxScale * (1 - t));
                    }
                }
                else
                {
                    invincibilityMaskTransform.localPosition = Vector3.left * invincibilityMaxPosition;
                    invincibilityMaskTransform.localScale = maskTransform.localScale.SetX(0);
                }
            }
        }

        public virtual void Show()
        {
            Redraw();

            isShown = true;

            showHideCoroutine.StopIfExists();
            showHideCoroutine = spriteGroup.DoAlpha(1f, 0.3f).SetEasing(EasingType.SineOut);
        }

        public virtual void Hide()
        {
            isShown = false;

            showHideCoroutine.StopIfExists();
            showHideCoroutine = spriteGroup.DoAlpha(0f, 0.3f).SetEasing(EasingType.SineOut);
        }

        public virtual void ForceHide()
        {
            isShown = false;

            showHideCoroutine.StopIfExists();
            spriteGroup.SetAlpha(0);
        }
    }
}