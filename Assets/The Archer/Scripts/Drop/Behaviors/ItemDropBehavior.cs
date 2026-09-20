using OctoberStudio.Armory;

namespace OctoberStudio.Drop
{
    public class ItemDropBehavior : DropBehavior
    {
        public ItemData ItemData { get; set; }

        public override void OnPickedUp()
        {
            base.OnPickedUp();

            gameObject.SetActive(false);
        }
    }
}