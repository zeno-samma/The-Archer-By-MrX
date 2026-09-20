using UnityEngine.Events;

namespace OctoberStudio
{
    public interface IDefeatable
    {
        void SubscribeOnDefeat(UnityAction<IDefeatable> action);
        void UnsubscribeOnDefeat(UnityAction<IDefeatable> action);

        void OnDefeatEndedEventFired();
    }
}