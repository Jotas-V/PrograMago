using UnityEngine;

namespace PrograMago.UnityIntegration
{
    [CreateAssetMenu(menuName = "PrograMago/Visual Catalog")]
    public sealed class TCC40VisualCatalog : ScriptableObject
    {
        public Sprite spectralWizard;
        public RuntimeAnimatorController pyromancerController;
        public RuntimeAnimatorController hydromancerController;
        public RuntimeAnimatorController electromancerController;
        public Sprite trainingDummyIdleSprite;
        public RuntimeAnimatorController trainingDummyController;

        public Sprite workspacePanel;
        public Sprite codePanel;
        public Sprite codeTray;
        public Sprite victoryPanel;
        public Sprite[] battleButtonStates = new Sprite[4];
        public Sprite[] actionButtonStates = new Sprite[4];

        public Sprite[] arenaWindFrames;
        public Sprite[] arenaLeafFrames;
        public Sprite[] arenaFireflyFrames;
    }
}
