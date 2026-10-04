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
        public Sprite iceGolemIdleSprite;
        public RuntimeAnimatorController iceGolemController;
        public Sprite fireElementalIdleSprite;
        public RuntimeAnimatorController fireElementalController;
        public Sprite aquaticSlimeIdleSprite;
        public RuntimeAnimatorController aquaticSlimeController;

        public Sprite GetEnemyIdleSprite(string name) => name switch
        {
            "Boneco de Treinamento" => trainingDummyIdleSprite,
            "Golem de Gelo" => iceGolemIdleSprite,
            "Elemental de Fogo" => fireElementalIdleSprite,
            "Slime Aquático" => aquaticSlimeIdleSprite,
            _ => null
        };

        public RuntimeAnimatorController GetEnemyController(string name) => name switch
        {
            "Boneco de Treinamento" => trainingDummyController,
            "Golem de Gelo" => iceGolemController,
            "Elemental de Fogo" => fireElementalController,
            "Slime Aquático" => aquaticSlimeController,
            _ => null
        };

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
