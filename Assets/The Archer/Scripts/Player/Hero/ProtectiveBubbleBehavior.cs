using OctoberStudio.Easing;
using UnityEngine;

namespace OctoberStudio
{
    public class ProtectiveBubbleBehavior : MonoBehaviour
    {
        protected static readonly int _Color = Shader.PropertyToID("_Color");

        [SerializeField] protected GameObject bubbleObject;
        [SerializeField] protected MeshRenderer bubbleRenderer;
        [SerializeField] protected ParticleSystem explosionParticle;

        protected Color bubbleColor;
        protected Color bubbleTransparentColor;

        protected IEasingCoroutine bubbleFadeCoroutine;

        protected virtual void Awake()
        {
            bubbleColor = bubbleRenderer.material.GetColor(_Color);
            bubbleTransparentColor = bubbleColor;
            bubbleTransparentColor.a = 0;
        }

        public void Show()
        {
            bubbleFadeCoroutine.StopIfExists();

            bubbleObject.gameObject.SetActive(true);
            bubbleRenderer.material.SetColor(_Color, bubbleColor);
        }

        public virtual void Hide(bool explosion)
        {
            if (explosion)
            {
                explosionParticle.Play();
                bubbleObject.SetActive(false);
            }
            else
            {
                bubbleFadeCoroutine = bubbleRenderer.material.DoColor(_Color, bubbleTransparentColor, 0.3f).SetOnFinish(() => bubbleObject.SetActive(false));
            }
        }
    }
}