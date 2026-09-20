using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

namespace OctoberStudio.StageCreator
{
    public class EditorGridView<T, K> where K : EditorGridItem<T>, new()
    {
        protected List<K> items = new List<K>();
        public int ItemsCount => items.Count;

        protected Vector2 scrollPosition = Vector2.zero;

        protected float width = 500f;
        protected Texture2D defaultItemTexture;

        public EditorGridView(Texture2D defaultItemTexture)
        {
            this.defaultItemTexture = defaultItemTexture;
        }

        public virtual List<K> GetItems()
        {
            return items;
        }

        public virtual Vector2Int GetContentDimentions(float parentWidth, Rect itemSize, float itemOffset)
        {
            int x = items.Count;
            for(int i = 0; i < items.Count; i++)
            {
                var itemEndX = (i + 1) * itemSize.width + i * itemOffset;

                if(itemEndX > parentWidth)
                {
                    x = i;
                    break;
                }
            }

            var itemCount = items.Count;
            if(itemCount == 0)
            {
                itemCount = 1;
                x = 1;
            }

            int y = Mathf.CeilToInt(items.Count / (float)x);

            return new Vector2Int(x, y);
        }

        public virtual Rect GetContentRect(float x, float y, Vector2Int size, Rect itemSize, float itemOffset)
        {
            return new Rect(x, y, size.x * itemSize.width + (size.x - 1) * itemOffset, size.y * itemSize.height + (size.y - 1) * itemOffset);
        }

        float paintDifference = 0;

        public virtual void Draw(float maxHeight, Rect itemSize, float padding, float itemOffset, UnityAction<T> onClickAction)
        {
            var lastRect = GUILayoutUtility.GetLastRect();

            var fullRect = new Rect(lastRect);
            fullRect.y += lastRect.height;
            fullRect.width = StageCreatorWindow.Instance.position.width - lastRect.x;
            var fullContentRect = fullRect.Shrink(padding + 1);

            var size = GetContentDimentions(fullContentRect.width, itemSize, itemOffset);
            var contentRect = GetContentRect(fullContentRect.x, fullContentRect.y, size, itemSize, itemOffset);
           
            var outlineRect = new Rect(contentRect);
            outlineRect.Grow(padding + 1);
            bool scrollBarShown = false;
            if (outlineRect.height > maxHeight)
            {
                outlineRect.height = maxHeight;
                scrollBarShown = true;
            }
            var scrollRect = outlineRect.Shrink(1);

            var rect = GUILayoutUtility.GetRect(0, outlineRect.height, GUILayout.MinHeight(outlineRect.height), GUILayout.MaxHeight(outlineRect.height), GUILayout.Height(outlineRect.height));

            if (Event.current.type == EventType.Repaint)
            {
                paintDifference = Mathf.Abs(rect.height - outlineRect.height);
            }

            EditorGUILayout.Space(paintDifference);

            if (scrollBarShown)
            {
                outlineRect.width += GUI.skin.verticalScrollbar.fixedWidth;
                scrollRect.width += GUI.skin.verticalScrollbar.fixedWidth;
            }

            EditorGUI.DrawRect(outlineRect, new Color(0.1f, 0.1f, 0.1f, 1));
            EditorGUI.DrawRect(scrollRect, new Color(0.2f, 0.2f, 0.2f));

            var viewRect = new Rect(padding, padding, contentRect.width, contentRect.height);
            scrollPosition = GUI.BeginScrollView(outlineRect, scrollPosition, viewRect);
            scrollPosition.x = 0;

            int x = 0;
            int y = 0;

            for (int i = 0; i < items.Count; i++)
            {
                var itemEndX = (x + 1) * itemSize.width + x * itemOffset;

                if (itemEndX > contentRect.width)
                {
                    x = 0;
                    y++;
                }

                var xpos = padding + 1 + x * itemSize.width + x * itemOffset;
                var ypos = padding + 1 + y * itemSize.height + y * itemOffset;
                var itemRect = new Rect(xpos,
                                        ypos,
                                        itemSize.width, itemSize.height);
                items[i].Draw(itemRect, onClickAction);

                x++;
            }

            GUI.EndScrollView();
        }

        protected float GetGridHeight(float width, float padding, Rect itemSize, float itemOffset)
        {
            int x = 0;
            int y = 0;

            for (int i = 0; i < items.Count; i++)
            {
                if (x * (itemSize.width + itemOffset) + itemSize.width + padding * 2 >= width)
                {
                    x = 0;
                    y++;
                }

                x++;
            }

            if (x * (itemSize.width + itemOffset) + itemSize.width >= width)
            {
                x = 0;
                y++;
            }

            return y * (itemSize.height + itemOffset) + itemSize.height + padding * 2;
        }

        public void SetItems(List<T> newItems)
        {
            items.Clear();
            foreach (var item in newItems)
            {
                var element = new K { Source = item, DefaultItemTexture = defaultItemTexture };

                items.Add(element);
            }
        }
    }
}