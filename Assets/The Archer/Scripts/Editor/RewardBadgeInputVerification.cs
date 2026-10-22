using System;
using System.Reflection;
using MrX.Input;
using MrX.UI;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.UI;

namespace MrX.EditorTools
{
    public static class RewardBadgeInputVerification
    {
        private static void RequireIsolation()
        {
            if (!Application.isPlaying || string.IsNullOrEmpty(SessionState.GetString(Save.SerializationHelper.VerificationDirectoryKey, "")))
                throw new InvalidOperationException("Play Mode with an isolated VerificationSession is required.");
        }

        public static string VerifyBadges()
        {
            RequireIsolation();
            int checks = 0;
            Action<bool, string> check = (ok, message) => { if (!ok) throw new Exception(message); checks++; };
            var badges = UnityEngine.Object.FindAnyObjectByType<MenuRewardBadges>();
            var serialized = new SerializedObject(badges);
            var questDot = (GameObject)serialized.FindProperty("dailyQuestsBadge").objectReferenceValue;
            var achievementDot = (GameObject)serialized.FindProperty("achievementsBadge").objectReferenceValue;
            var quests = GameController.DailyQuestsManager;
            var achievements = GameController.AchievementsManager;
            check(questDot != null && achievementDot != null, "Authored badge references");
            check(!questDot.GetComponent<Image>().raycastTarget && !achievementDot.GetComponent<Image>().raycastTarget, "Dots do not intercept clicks");
            check(questDot.activeSelf == quests.HasClaimableRewards && achievementDot.activeSelf == (achievements.ClaimableCount > 0), "Initial save state reflected");
            check(quests.HasClaimableRewards, "Fresh login reward is claimable");
            foreach (var q in quests.Database.quests) quests.TryClaimQuest(q.id);
            check(quests.HasClaimableRewards && questDot.activeSelf, "Milestone alone keeps daily badge visible");
            foreach (var m in quests.Database.milestones) quests.TryClaimMilestone(m.id);
            check(!quests.HasClaimableRewards && !questDot.activeSelf, "All available daily rewards claimed hides badge");
            var quest = quests.Database.quests[0];
            quests.Record(quest.type, quest.target);
            check(quests.HasClaimableRewards && questDot.activeSelf, "New quest completion shows badge immediately");
            foreach (var q in quests.Database.quests) quests.TryClaimQuest(q.id);
            foreach (var m in quests.Database.milestones) quests.TryClaimMilestone(m.id);
            check(!questDot.activeSelf, "Daily badge clears after quest and milestone claims");
            foreach (var a in achievements.Database.achievements) achievements.TryClaim(a.id);
            check(!achievementDot.activeSelf, "Already claimed achievements do not notify");
            var target = achievements.Database.achievements.Find(a => a.metric == Achievements.AchievementMetric.EnemiesDefeated);
            achievements.Record(target.metric, target.target);
            check(achievementDot.activeSelf && achievements.ClaimableCount > 0, "New achievement reward shows badge");
            badges.enabled = false;
            foreach (var a in achievements.Database.achievements) achievements.TryClaim(a.id);
            badges.enabled = true;
            check(!achievementDot.activeSelf, "Reenable refreshes latest claim state");
            var changed = typeof(Quests.DailyQuestsManager).GetField("Changed", BindingFlags.Instance | BindingFlags.NonPublic);
            int before = ((Delegate)changed.GetValue(quests)).GetInvocationList().Length;
            for (int i = 0; i < 5; i++) { badges.enabled = false; badges.enabled = true; }
            check(((Delegate)changed.GetValue(quests)).GetInvocationList().Length == before, "Reenable does not duplicate listeners");
            quests.Save.EnsureDay(DateTime.UtcNow.AddDays(1), quests.Database.resetUtcOffsetHours);
            check(!quests.HasClaimableRewards, "Day reset clears previous rewards before login refresh");
            quests.RefreshDay();
            check(questDot.activeSelf && quests.HasClaimableRewards, "Next day login refresh restores daily notification");
            return "Reward badges: " + checks + " live checks passed.";
        }

        public static string VerifyInput()
        {
            RequireIsolation();
            var input = GameController.InputManager as InputManager;
            if (input == null || input.Joystick == null) throw new InvalidOperationException("Load a gameplay stage with its authored joystick first.");
            int checks = 0;
            Action<bool, string> check = (ok, message) => { if (!ok) throw new Exception(message); checks++; };
            var tickMethod = typeof(InputManager).GetMethod("Update", BindingFlags.Instance | BindingFlags.NonPublic);
            Action tick = () => tickMethod.Invoke(input, null);
            var previousPad = Gamepad.current;
            var previousKeyboard = Keyboard.current;
            var previousMouse = Mouse.current;
            Gamepad pad = null; Keyboard keyboard = null; Mouse mouse = null;
            try
            {
                pad = InputSystem.AddDevice<Gamepad>("MrX Verification Gamepad");
                keyboard = InputSystem.AddDevice<Keyboard>("MrX Verification Keyboard");
                mouse = InputSystem.AddDevice<Mouse>("MrX Verification Mouse");
                InputSystem.QueueStateEvent(mouse, new MouseState { position = new Vector2(-1000, -1000) }.WithButton(MouseButton.Left));
                InputSystem.Update(); tick();
                check(input.ActiveInput == InputType.UIJoystick, "Mouse selects UI joystick");
                InputSystem.QueueStateEvent(mouse, new MouseState { position = new Vector2(-1000, -1000) });
                InputSystem.QueueStateEvent(pad, new GamepadState { leftStick = new Vector2(.3f, 0) });
                InputSystem.Update(); tick();
                check(input.ActiveInput == InputType.Gamepad && input.MovementValue.x > .05f, "Light left stick works after mouse input");
                InputSystem.QueueStateEvent(pad, new GamepadState { rightStick = new Vector2(0, .7f) });
                InputSystem.Update(); tick();
                check(input.MovementValue.y > .1f && input.ActiveInput == InputType.Gamepad, "Right stick works");
                InputSystem.QueueStateEvent(pad, new GamepadState());
                InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.W));
                InputSystem.Update(); tick();
                check(input.MovementValue.y > .9f && input.ActiveInput == InputType.Keyboard, "WASD works after controller");
                InputSystem.QueueStateEvent(mouse, new MouseState { position = new Vector2(-1000, -1000) }.WithButton(MouseButton.Left));
                InputSystem.Update(); tick();
                check(input.MovementValue.y > .9f, "Held keyboard movement survives mouse click");
                InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.LeftArrow));
                InputSystem.QueueStateEvent(mouse, new MouseState { position = new Vector2(-1000, -1000) });
                InputSystem.Update(); tick();
                check(input.MovementValue.x < -.9f, "Arrow key movement works");
                InputSystem.QueueStateEvent(keyboard, new KeyboardState());
                InputSystem.Update(); tick();
                check(input.MovementValue.sqrMagnitude < .001f, "Releasing all physical controls stops movement");
                InputSystem.QueueStateEvent(mouse, new MouseState { position = new Vector2(-1000, -1000) }.WithButton(MouseButton.Left));
                InputSystem.Update(); tick();
                var pointer = new PointerEventData(EventSystem.current) { position = new Vector2(UnityEngine.Device.Screen.width * .25f, UnityEngine.Device.Screen.height * .25f) };
                input.Joystick.OnPointerDown(pointer);
                pointer.position += Vector2.right * 100;
                input.Joystick.OnDrag(pointer); tick();
                check(input.MovementValue.x > .05f && input.ActiveInput == InputType.UIJoystick, "Mouse/touch drag joystick still moves");
                input.Joystick.OnPointerUp(pointer); tick();
                check(input.MovementValue.sqrMagnitude < .001f, "Releasing joystick stops movement");
                InputSystem.QueueStateEvent(mouse, new MouseState { position = new Vector2(-1000, -1000) });
                InputSystem.QueueStateEvent(pad, new GamepadState { leftStick = new Vector2(.3f, 0) });
                InputSystem.Update(); tick();
                check(input.ActiveInput == InputType.Gamepad, "Controller reconnect/use switches back");
                InputSystem.RemoveDevice(pad); pad = null; tick();
                check(previousPad != null || input.ActiveInput != InputType.Gamepad, "Disconnected only controller falls back");
                check(input.MovementValue.sqrMagnitude < .001f, "Disconnect does not leave stale movement");
                return "Input: " + checks + " simulated device and real joystick checks passed.";
            }
            finally
            {
                if (pad != null) InputSystem.RemoveDevice(pad);
                if (keyboard != null) InputSystem.RemoveDevice(keyboard);
                if (mouse != null) InputSystem.RemoveDevice(mouse);
                if (previousPad != null && previousPad.added) previousPad.MakeCurrent();
                if (previousKeyboard != null && previousKeyboard.added) previousKeyboard.MakeCurrent();
                if (previousMouse != null && previousMouse.added) previousMouse.MakeCurrent();
                if (input.Joystick != null) input.Joystick.ResetJoystick();
                tick();
            }
        }
    }
}
