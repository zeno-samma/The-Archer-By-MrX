using UnityEngine.UI;

namespace OctoberStudio.UI.Armory
{
    public class HeroesGridBehavior : ArmoryGridUI<HeroCardBehavior>
    {
        public Selectable GetFirstItemSelectable()
        {
            if (cards.Count > 0)
            {
                return cards[0].Selectable;
            }
            return null;
        }

        public virtual void Init()
        {
            Clear();

            var heroes = GameController.ArmoryManager.GetAllHeroDataList();

            for (int i = 0; i < heroes.Count; i++)
            {
                var heroData = heroes[i];
                if (heroData != null)
                {
                    var card = cardsPool.GetEntity();

                    var save = GameController.ArmoryManager.GetHeroSave(heroData.Id);

                    card.SetData(heroData, save);
                    cards.Add(card);
                }
            }
        }
    }
}