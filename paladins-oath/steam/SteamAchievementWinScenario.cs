using Sirenix.OdinInspector;
using UnityEngine;

namespace FireBiscuit {

    [HideMonoScript]
    [CreateAssetMenu(fileName = "Achievement__Win Scenario", menuName = "Achievement/Win Scenario")]
    public class SteamAchievementWinScenario : SteamAchievement {
        [Required, SerializeField] private ScenarioData scenario;
        [Required, SerializeField] private OathData[] validOaths = new OathData[0];
        [SerializeField] private TimeOfDay validTimeOfDay = null;

        protected override void onNewCrusade(CrusadeState crusade) { }

        protected override bool shouldUnlock(CrusadeState crusade) {
            // If the player resumes an old save (before this achievement was implemented)
            // Their previously completed scenarios will be excluded. This is deemed acceptable as this achievement
            // was added before the game official launch.
            if (!crusade.ScenarioState.IsCompleted()) {
                return false;
            }
            if (crusade.CrusadeConfig.ScenarioData != scenario) {
                return false;
            }
            if (validOaths.Length > 0 && !validOaths.Contains(crusade.PlayerState.Oath)) {
                return false;
            }
            if (validTimeOfDay != null) {
                bool isMet = true;
                foreach (TimeOfDay timeOfDay in crusade.CrusadeConfig.TimeOfDays) {
                    if (timeOfDay != validTimeOfDay) {
                        isMet = false;
                        break;
                    }
                }
                if (!isMet) {
                    return false;
                }
            }
            return true;
        }

        protected override void clearState() { }
    }
}
