using UnityEngine;
using UnityEngine.Events;

namespace OctoberStudio.StageCreator
{
    public abstract class EditorGridItem<T>
    {
        public T Source { get; set; }
        public Texture2D DefaultItemTexture { get; set; }

        public abstract void Draw(Rect rect, UnityAction<T> action);
    }
}