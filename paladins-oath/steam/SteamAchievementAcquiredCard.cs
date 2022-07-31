using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace FireBiscuit {

    [HideMonoScript]
    [CreateAssetMenu(fileName = "Achievement__Acquired Card", menuName = "Achievement/Acquired Card")]
    public class SteamAchievementAcquiredCard : SteamAchievement {
        // One-Of
        [Required, SerializeField] private CardType[] validCardTypes = new CardType[0];
        [Required, SerializeField] private CardData[] validCards = new CardData[0];

        private InitState initState = null;

        protected override void onNewCrusade(CrusadeState crusade) {
            // If the player resumes an old save (before this achievement was implemented)
            // Their current cards will be excluded. This is deemed acceptable as this achievement
            // was added before the game official launch.
            HashSet<string> startingCardIds = CollectionPools.HashSet<string>();
            try {
                Types.CollectGuids(crusade.PlayerState.Hand, startingCardIds);
                Types.CollectGuids(crusade.PlayerState.DiscardedCards, startingCardIds);
                Types.CollectGuids(crusade.PlayerState.DrawPile, startingCardIds);

                this.initState = new InitState(
                    startingCardIds: startingCardIds
                );
            } finally {
                CollectionPools.Return(startingCardIds);
            }
        }

        protected override bool shouldUnlock(CrusadeState crusade) {
            if (initState == null) {
                return false;
            }
            bool isUnlocked = false;
            processCards(crusade.PlayerState.Hand, out isUnlocked);
            if (isUnlocked) {
                return true;
            }
            processCards(crusade.PlayerState.DiscardedCards, out isUnlocked);
            if (isUnlocked) {
                return true;
            }
            processCards(crusade.PlayerState.DrawPile, out isUnlocked);
            if (isUnlocked) {
                return true;
            }
            // Player might have acquired and thrown the card in the same session via spell or other effects.
            processCards(crusade.PlayerState.ThrownCards, out isUnlocked);
            if (isUnlocked) {
                return true;
            }
            return false;
        }

        private void processCards(IEnumerable<PlayerCardState> cards, out bool isAchievementUnlocked) {
            if (initState == null) {
                isAchievementUnlocked = false;
                return;
            }
            HashSet<string> validCardIds = CollectionPools.HashSet<string>();
            try {
                if (validCards.Length > 0) {
                    Types.CollectGuids(validCards, validCardIds);
                }


                foreach (PlayerCardState card in cards) {
                    if (initState.StartingCardIds.Contains(card.GUID)) {
                        continue; // skip starting cards, only consider acquired cards
                    }
                    CardData cardData = card.Entity;
                    if (validCardTypes.Length > 0 && validCardTypes.Contains(cardData.CardType)) {
                        isAchievementUnlocked = true;
                        return;
                    }
                    if (validCardIds.Count > 0 && validCardIds.Contains(cardData.GUID)) {
                        isAchievementUnlocked = true;
                        return;
                    }
                }

                isAchievementUnlocked = false;

            } finally {
                CollectionPools.Return(validCardIds);
            }
        }

        protected override void clearState() {
            initState = null;
        }

        private class InitState {
            private readonly HashSet<string> startingCardIds = new HashSet<string>();

            public InitState(IEnumerable<string> startingCardIds) {
                this.startingCardIds.UnionWith(startingCardIds);
            }

            public IReadOnlyHashSet<string> StartingCardIds => startingCardIds.AsReadOnly();
        }
    }
}
