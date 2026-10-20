using MrX.Input;
using MrX.Save;
using UnityEngine;

namespace MrX
{
    public class InputSave : ISave
    {
        [SerializeField] protected InputType activeInput;

        public InputType ActiveInput { get => activeInput; set => activeInput = value; }

        public virtual void Flush()
        {

        }
    }
}