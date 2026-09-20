using System;
using System.Collections;

namespace OctoberStudio
{
    public class WaitUntilTrue : IEnumerator
    {
        public bool IsActive { get; protected set; } = true;

        public object Current => null;

        public static implicit operator bool(WaitUntilTrue value)
        {
            return value.IsActive;
        }

        public static implicit operator Func<bool>(WaitUntilTrue value)
        {
            return value.MoveNext;
        }

        public bool MoveNext()
        {
            return IsActive;
        }

        public void Reset()
        {
            IsActive = true;
        }

        public void Complete()
        {
            IsActive = false;
        }
    }
}