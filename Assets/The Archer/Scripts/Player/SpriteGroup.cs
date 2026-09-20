using OctoberStudio.Extensions;
using System.Collections.Generic;
using UnityEngine;

namespace OctoberStudio
{
    public class SpriteGroup : MonoBehaviour
    {
        protected List<SpriteRenderer> spriteRenderers = new List<SpriteRenderer>();
        protected Dictionary<SpriteRenderer, float> spriteAlphaValues = new Dictionary<SpriteRenderer, float>();

        [SerializeField, HideInInspector] protected float alpha = 1f;
        public float Alpha
        {
            get => alpha;
            set => SetAlpha(value);
        }

        public void SetAlpha(float value)
        {
            alpha = Mathf.Clamp01(value);
            for (int i = 0; i < spriteRenderers.Count; i++)
            {
                if (spriteRenderers[i] != null)
                {
                    var targetAlpha = spriteAlphaValues[spriteRenderers[i]] * alpha;
                    spriteRenderers[i].SetAlpha(targetAlpha);
                }
                else
                {
                    spriteRenderers.RemoveAt(i);
                    i--;
                }
            }
        }

        protected virtual void Start()
        {
            Collect(true);
        }

        protected virtual void Collect(bool clear)
        {
            spriteRenderers.Clear();

            if (clear) spriteAlphaValues.Clear();
            GetComponentsInChildren(true, spriteRenderers);

            for (int i = 0; i < spriteRenderers.Count; i++)
            {
                if (!spriteAlphaValues.ContainsKey(spriteRenderers[i]))
                {
                    spriteAlphaValues[spriteRenderers[i]] = spriteRenderers[i].color.a;
                }
            }
        }
    }
}