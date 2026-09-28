using UnityEngine;

namespace PrograMago.UnityIntegration
{
    // The containing object is tagged EditorOnly and is stripped from player builds.
    [ExecuteAlways]
    public sealed class ArenaEditorPreview : MonoBehaviour
    {
        [SerializeField] private GameplayBootstrapper presentation;
        [SerializeField] private GameObject wizard;
        [SerializeField] private GameObject enemy;

        public void Configure(GameplayBootstrapper owner, GameObject wizardPreview, GameObject enemyPreview)
        {
            presentation = owner;
            wizard = wizardPreview;
            enemy = enemyPreview;
        }

        private void OnEnable()
        {
            if (UnityEngine.Application.IsPlaying(gameObject)) gameObject.SetActive(false);
        }

#if UNITY_EDITOR
        private void LateUpdate()
        {
            if (!UnityEngine.Application.isPlaying && presentation != null)
                presentation.RefreshEditorArena(wizard, enemy);
        }
#endif
    }
}
