using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Events;

namespace OctoberStudio.Easing
{
    [DefaultExecutionOrder(10000)]
    public class EasingManager : MonoBehaviour
    {
        private static EasingManager instance;

        private static List<IEasingCoroutine> coroutinesToStart = new List<IEasingCoroutine>();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            instance = null;
            coroutinesToStart = new List<IEasingCoroutine>();
        }

        public void Awake()
        {
            instance = this;
        }

        public static IEasingCoroutine DoFloat(float from, float to, float duration, UnityAction<float> action, float delay = 0)
        {
            return new FloatEasingCoroutine(from, to, duration, delay, action);
        }

        public static IEasingCoroutine DoAfter(float seconds, UnityAction action, bool unscaledTime = false)
        {
            return new WaitCoroutine(seconds, unscaledTime).SetOnFinish(action);
        }

        public static IEasingCoroutine DoAfter(Func<bool> condition)
        {
            return new WaitForConditionCoroutine(condition);
        }

        public static IEasingCoroutine DoNextFrame()
        {
            return new NextFrameCoroutine();
        }
        public static IEasingCoroutine DoNextFrame(UnityAction action)
        {
            return new NextFrameCoroutine().SetOnFinish(action);
        }

        public static IEasingCoroutine DoNextFixedFrame()
        {
            return new NextFixedFrameCoroutine();
        }

        public static IEasingCoroutine DoRepeatedly(float duration, float interval, UnityAction action)
        {
            return new DoRepeadlyCoroutine(duration, interval, action);
        }

        public static IEasingCoroutine DoTimeScale(float timeScale, float duration)
        {
            var easingCoroutine = new FloatEasingCoroutine(Time.timeScale, timeScale, duration, 0, TimeHelper.SetTimeScale).SetUnscaledTime(true);

            return easingCoroutine;
        }

        public static Coroutine StartCustomCoroutine(IEnumerator coroutine)
        {
            return instance.StartCoroutine(coroutine);
        }

        public static void StartCustomCoroutine(IEasingCoroutine coroutine)
        {
            coroutinesToStart.Add(coroutine);
        }

        public static void RemoveCoroutineFromStart(IEasingCoroutine coroutine)
        {
            coroutinesToStart.Remove(coroutine);
        }

        public static void StopCustomCoroutine(Coroutine coroutine)
        {
            if (instance != null) instance.StopCoroutine(coroutine);
        }

        private void Update()
        {
            if (coroutinesToStart.Count > 0)
            {
                for (int i = 0; i < coroutinesToStart.Count; i++)
                {
                    var coroutine = coroutinesToStart[i];

                    coroutine.Start();
                }

                coroutinesToStart.Clear();
            }
        }
    }

    public interface IEasingCoroutine : IEnumerator
    {
        bool IsActive { get; }
        IEasingCoroutine SetEasing(EasingType easingType);
        IEasingCoroutine SetEasingCurve(AnimationCurve easingCurve);
        IEasingCoroutine SetOnFinish(UnityAction action);
        IEasingCoroutine SetUnscaledTime(bool unscaledTime);
        IEasingCoroutine SetDelay(float delay);
        void Stop();
        void Start();

        Coroutine GetCoroutine();
    }

    public abstract class EmptyCoroutine : IEasingCoroutine
    {
        protected Coroutine coroutine;
        public Coroutine GetCoroutine()
        {
            return coroutine;
        }

        private bool isActive;
        public bool IsActive
        {
            get => isActive;
            protected set
            {
                isActive = value;
                if (!IsActive) IsFinished = true;
            }
        }

        public bool IsFinished { get; protected set; }
        protected UnityAction finishAction;

        protected EasingType easingType = EasingType.Linear;

        protected float delay = -1;

        protected bool unscaledTime;

        protected bool useCurve;

        protected AnimationCurve easingCurve;

        public abstract void Start();

        public EmptyCoroutine()
        {
            IsFinished = false;
        }

        public IEasingCoroutine SetEasing(EasingType easingType)
        {
            this.easingType = easingType;
            useCurve = false;
            return this;
        }

        public IEasingCoroutine SetOnFinish(UnityAction action)
        {
            finishAction = action;
            return this;
        }

        public IEasingCoroutine SetUnscaledTime(bool unscaledTime)
        {
            this.unscaledTime = unscaledTime;
            return this;
        }

        public IEasingCoroutine SetEasingCurve(AnimationCurve curve)
        {
            easingCurve = curve;
            useCurve = true;

            return this;
        }

        public IEasingCoroutine SetDelay(float delay)
        {
            this.delay = delay;

            return this;
        }

        public void Stop()
        {
            if (coroutine != null)
            {
                EasingManager.StopCustomCoroutine(coroutine);
            }
            else
            {
                EasingManager.RemoveCoroutineFromStart(this);
            }

            IsActive = false;
            IsFinished = true;
        }

        #region IEnumerator

        public object Current => null;

        public bool MoveNext()
        {
            return !IsFinished;
        }

        public void Reset()
        {
            Stop();
        }

        #endregion
    }

    public class NextFrameCoroutine : EmptyCoroutine
    {
        public NextFrameCoroutine() : base()
        {
            EasingManager.StartCustomCoroutine(this);
        }

        public override void Start()
        {
            coroutine = EasingManager.StartCustomCoroutine(Coroutine());
        }

        private IEnumerator Coroutine()
        {
            IsActive = true;

            yield return null;

            finishAction?.Invoke();

            IsActive = false;
        }
    }

    public class NextFixedFrameCoroutine : EmptyCoroutine
    {
        public NextFixedFrameCoroutine() : base()
        {
            EasingManager.StartCustomCoroutine(this);
        }

        public override void Start()
        {
            coroutine = EasingManager.StartCustomCoroutine(Coroutine());
        }

        private IEnumerator Coroutine()
        {
            IsActive = true;

            yield return new WaitForFixedUpdate();

            finishAction?.Invoke();

            IsActive = false;
        }
    }

    public class WaitCoroutine : EmptyCoroutine
    {
        protected float duration;

        public WaitCoroutine(float duration, bool unscaledTime = false) : base()
        {
            this.duration = duration;
            this.unscaledTime = unscaledTime;

            EasingManager.StartCustomCoroutine(this);
        }

        public override void Start()
        {
            coroutine = EasingManager.StartCustomCoroutine(Coroutine());
        }

        private IEnumerator Coroutine()
        {
            IsActive = true;

            while (delay > 0)
            {
                yield return null;

                delay -= unscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
            }

            if (unscaledTime)
            {
                yield return new WaitForSecondsRealtime(duration);
            }
            else
            {
                yield return new WaitForSeconds(duration);
            }

            finishAction?.Invoke();

            IsActive = false;
        }
    }

    public class WaitForConditionCoroutine : EmptyCoroutine
    {
        protected Func<bool> condition;

        public WaitForConditionCoroutine(Func<bool> condition) : base()
        {
            this.condition = condition;
            EasingManager.StartCustomCoroutine(this);
        }

        public override void Start()
        {
            coroutine = EasingManager.StartCustomCoroutine(Coroutine());
        }

        protected IEnumerator Coroutine()
        {
            IsActive = true;

            while (delay > 0)
            {
                yield return null;

                delay -= unscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
            }

            do
            {
                yield return null;
            } while (!condition());

            finishAction?.Invoke();

            IsActive = false;
        }
    }

    public class DoRepeadlyCoroutine : EmptyCoroutine
    {
        protected float duration;
        protected float interval;

        protected UnityAction action;

        public DoRepeadlyCoroutine(float duration, float interval, UnityAction action) : base()
        {
            this.duration = duration;
            this.interval = interval;

            this.action = action;

            EasingManager.StartCustomCoroutine(this);
        }

        public override void Start()
        {
            coroutine = EasingManager.StartCustomCoroutine(Coroutine());
        }

        protected virtual IEnumerator Coroutine()
        {
            IsActive = true;

            while (delay > 0)
            {
                yield return null;

                delay -= unscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
            }

            var time = 0f;

            // Unfornunately WaitForSeconds and WaitForSecondsRealtime don't have common ancestor, and I don't want to instantiate new wait every interval
            if (unscaledTime)
            {
                var wait = interval > 0 ? new WaitForSecondsRealtime(interval) : null;

                while (time <= duration)
                {
                    action?.Invoke();

                    yield return wait;

                    time += interval;
                }
            }
            else
            {
                var wait = interval > 0 ? new WaitForSeconds(interval) : null;

                while (time <= duration)
                {
                    action?.Invoke();

                    yield return wait;

                    time += interval;
                }
            }

            finishAction?.Invoke();

            IsActive = false;
        }
    }

    public abstract class EasingCoroutine<T> : EmptyCoroutine
    {
        protected T from;
        protected T to;
        protected float duration;

        protected UnityAction<T> action;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public abstract T Lerp(T a, T b, float t);

        public EasingCoroutine(T from, T to, float duration, float delay, UnityAction<T> action) : base()
        {
            this.from = from;
            this.to = to;
            this.duration = duration;
            this.action = action;
            this.delay = delay;

            EasingManager.StartCustomCoroutine(this);
        }

        public override void Start()
        {
            coroutine = EasingManager.StartCustomCoroutine(Coroutine());
        }

        private IEnumerator Coroutine()
        {
            IsActive = true;

            float time = 0;

            while (time < duration)
            {
                yield return null;

                if (delay > 0)
                {
                    delay -= unscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;

                    if (delay > 0) continue;
                }

                time += unscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
                float t;
                if (useCurve)
                {
                    t = easingCurve.Evaluate(time / duration);
                }
                else
                {
                    t = EasingFunctions.ApplyEasing(time / duration, easingType);
                }

                T value = Lerp(from, to, t);
                action?.Invoke(value);
            }

            action.Invoke(to);
            finishAction?.Invoke();

            IsActive = false;
        }
    }

    public class FloatEasingCoroutine : EasingCoroutine<float>
    {
        public FloatEasingCoroutine(float from, float to, float duration, float delay, UnityAction<float> action) : base(from, to, duration, delay, action)
        {

        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override float Lerp(float a, float b, float t)
        {
            return Mathf.LerpUnclamped(a, b, t);
        }
    }

    public class VectorEasingCoroutine3 : EasingCoroutine<Vector3>
    {
        public VectorEasingCoroutine3(Vector3 from, Vector3 to, float duration, float delay, UnityAction<Vector3> action) : base(from, to, duration, delay, action)
        {

        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override Vector3 Lerp(Vector3 a, Vector3 b, float t)
        {
            return Vector3.LerpUnclamped(a, b, t);
        }
    }

    public class VectorEasingCoroutine2 : EasingCoroutine<Vector2>
    {
        public VectorEasingCoroutine2(Vector2 from, Vector2 to, float duration, float delay, UnityAction<Vector2> action) : base(from, to, duration, delay, action)
        {

        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override Vector2 Lerp(Vector2 a, Vector2 b, float t)
        {
            return Vector2.LerpUnclamped(a, b, t);
        }
    }

    public class ColorEasingCoroutine : EasingCoroutine<Color>
    {
        public ColorEasingCoroutine(Color from, Color to, float duration, float delay, UnityAction<Color> action) : base(from, to, duration, delay, action)
        {

        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override Color Lerp(Color a, Color b, float t)
        {
            return Color.LerpUnclamped(a, b, t);
        }
    }
}