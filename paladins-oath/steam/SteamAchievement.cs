using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace FireBiscuit {

    public abstract class SteamAchievement : ScriptableObjectWithGUID, ITypeWithEnvironmentAvailability {
        [Required, SerializeField] private BuildEnvironment[] environmentAvailability = new BuildEnvironment[0];
        [Required, SerializeField] private string steamAchievementId;
        [Required, SerializeField] private bool shouldTriggerDuringTutorial = false;

        private bool isUnlocked = false;
        private Action onUnlocked = null;
        private Action onCleared = null;

        public bool IsAvailableInCurrentEnvironment() => environmentAvailability.Length == 0 || environmentAvailability.Contains(GameConfig.Instance.CurrentEnvironment);
        public string SteamAchievementId => steamAchievementId;
        public bool IsUnlocked => isUnlocked;

        private string lastCrusadeId = null;

        public void Initialize(bool isInitUnlocked, Action onUnlocked, Action onCleared) {
            this.onUnlocked = onUnlocked;
            this.onCleared = onCleared;
            this.isUnlocked = isInitUnlocked;
        }

        public void Clear() {
            if (!this.IsUnlocked) {
                return; // already cleared
            }
            this.isUnlocked = false;
            this.lastCrusadeId = null;
            clearState();
            onCleared?.Invoke();
        }

        public void Evaluate(CrusadeState crusade) {
            if (this.isUnlocked) {
                return; // already unlocked
            }
            if (!shouldTriggerDuringTutorial && crusade.CrusadeConfig.ScenarioData is ITutorialScenarioData) {
                return; // no triggering during tutorials
            }

            if (lastCrusadeId == null || crusade.SaveFileId != lastCrusadeId) {
                this.lastCrusadeId = crusade.SaveFileId;
                onNewCrusade(crusade);
                return; // cannot unlock when the crusade starts, the player hasn't done anything yet.
            }
            this.isUnlocked = shouldUnlock(crusade);
            if (this.isUnlocked) {
                onUnlocked?.Invoke();
            }
        }

        // Always called before shouldUnlock when a new crusade was detected.
        // Opportunity for the achievement to reset its state.
        protected abstract void onNewCrusade(CrusadeState crusade);
        // Called to evaluate if the achievement conditions have changed.
        protected abstract bool shouldUnlock(CrusadeState crusade);
        protected abstract void clearState();

        public override string ToString() => $"SteamAchievement> {SteamAchievementId}";
    }
}
