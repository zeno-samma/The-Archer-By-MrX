using OctoberStudio.Audio;
using OctoberStudio.Easing;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace OctoberStudio
{
    public class ExperienceManager : MonoBehaviour
    {
        [SerializeField] protected ExperienceData experienceData;
        [SerializeField] protected ExperienceUI experienceUI;
        [SerializeField] protected AudioData levelupSound;

        [Header("Animation settings")]
        [SerializeField] protected float startAcceleration = 1;
        [SerializeField] protected float maxSpeed = 1;
        [SerializeField] protected float endAcceleration = 1;

        public float Speed { get; protected set; } = 0f;

        public float XP { get => ContinuePlayingSave.XP; protected set => ContinuePlayingSave.XP = value; }
        public float TempXP { get; protected set; } 
        public float TargetXP { get; protected set; }
        public int XPLevel { get => ContinuePlayingSave.XPLevel; protected set => ContinuePlayingSave.XPLevel = value; }

        public event UnityAction<int> onXpLevelChanged;

        protected ContinuePlayingSave ContinuePlayingSave { get; set; }

        protected virtual void Awake()
        {
            StageController.RegisterExperienceManager(this);
        }

        public virtual void Init()
        {
            ContinuePlayingSave = GameController.SaveManager.GetSave<ContinuePlayingSave>("Continue Playing");

            if(!ContinuePlayingSave.HasUnfinishedStageData)
            {
                XP = 0;
                XPLevel = 0;
            }

            TempXP = XP;

            TargetXP = experienceData.GetXP(XPLevel);
            EasingManager.DoNextFrame().SetOnFinish(() => experienceUI.SetProgress(XP / TargetXP));
            experienceUI.SetLevelText(XPLevel + 1);
        }

        protected virtual void Update()
        {
            if(TempXP != XP)
            {
                var xpProportion = XP / TargetXP;
                var tempXPProportion = TempXP / TargetXP;

                var distance = xpProportion - tempXPProportion;
                var deccelerationDistance = (Speed * Speed) / (2 * endAcceleration);

                if (distance > deccelerationDistance)
                {
                    if(Speed < maxSpeed)
                    {
                        Speed += startAcceleration * Time.deltaTime;
                        if(Speed > maxSpeed)
                        {
                            Speed = maxSpeed;
                        }
                    }
                } else
                {
                    Speed -= endAcceleration * Time.deltaTime;
                    if (Speed < 0) Speed = 0;
                }

                if (Speed == 0)
                {
                    TempXP = XP; // If speed is zero, we are at the target XP
                } else
                {
                    tempXPProportion += Speed * Time.deltaTime;
                    TempXP = tempXPProportion * TargetXP;
                }

                if (TempXP > XP) TempXP = XP; // Prevent overshooting

                if (TempXP >= TargetXP)
                {
                    var nextTarget = experienceData.GetXP(XPLevel + 1);

                    if (TempXP >= TargetXP + nextTarget)
                    {
                        StartCoroutine(IncreaseLevelCoroutine());
                    }
                    else
                    {
                        IncreaseLevel();
                    }
                }
            }

            experienceUI.SetProgress(TempXP / TargetXP);
        }

        public virtual void AddXP(float xp)
        {
            XP += xp;
        }

        protected virtual IEnumerator IncreaseLevelCoroutine()
        {
            while(TempXP >= TargetXP)
            {
                IncreaseLevel();

                // We are allowing abilities manager to set timescale to zero and show the abilities panel for each upgrade
                yield return new WaitForSeconds(0.001f);
            }

            experienceUI.SetProgress(TempXP / TargetXP);
        }

        protected virtual void IncreaseLevel()
        {
            XPLevel++;
            XP -= TargetXP;
            TempXP -= TargetXP;

            TargetXP = experienceData.GetXP(XPLevel);

            experienceUI.SetLevelText(XPLevel + 1);

            if(levelupSound != null)
            {
                GameController.AudioManager.PlayAudio(levelupSound);
            }

            onXpLevelChanged?.Invoke(XPLevel);
        }

        public virtual bool WillLevelUp(float additionalXP)
        {
            return XP + additionalXP >= TargetXP;
        }
    }
}