using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

namespace OctoberStudio.StageCreator
{
    public class RoomPropGridItem : EditorGridItem<GameObject>
    {
        protected Texture2D previewTexture;

        protected UnityAction onClickAction;

        protected bool IsTextureLoaded => previewTexture != null;

        protected GUIContent buttonContent;

        public override void Draw(Rect rect, UnityAction<GameObject> onClick)
        {
            if(buttonContent == null)
            {
                buttonContent = new GUIContent(DefaultItemTexture, Source.name);
            }

            if(previewTexture == null)
            {
                previewTexture = AssetPreview.GetAssetPreview(Source);

                if(previewTexture != null)
                {
                    buttonContent.image = previewTexture;
                }
            }

            if (GUI.Button(rect, buttonContent))
            {
                onClick?.Invoke(Source);
            }

            var lavelRect = new Rect(rect.x, rect.y + rect.height - 20, rect.width, 20);
            GUI.Label(lavelRect, Source.name);
        }
    }
}