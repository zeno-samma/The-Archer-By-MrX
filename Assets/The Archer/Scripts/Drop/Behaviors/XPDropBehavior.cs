namespace OctoberStudio.Drop
{
    public class XPDropBehavior : DropBehavior
    {
        public float XPAmount { get; set; } = 1f;

        public override void OnPickedUp()
        {
            base.OnPickedUp();

            StageController.ExperienceManager.AddXP(XPAmount);

            gameObject.SetActive(false);
        }
    }
}
