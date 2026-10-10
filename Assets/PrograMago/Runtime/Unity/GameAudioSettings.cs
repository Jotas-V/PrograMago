using UnityEngine;
namespace PrograMago.UnityIntegration
{
    public static class GameAudioSettings
    {
        public const string MasterKey = "PrograMago.Volume.Master";
        public const string MusicKey = "PrograMago.Volume.Music";
        public const string EffectsKey = "PrograMago.Volume.Effects";
        public static float Master => Read(MasterKey);
        public static float Music => Read(MusicKey);
        public static float Effects => Read(EffectsKey);
        private static float Read(string key) => Mathf.Clamp01(PlayerPrefs.GetFloat(key, 1f));
        public static void Set(string key, float value)
        {
            if (key != MasterKey && key != MusicKey && key != EffectsKey)
                throw new System.ArgumentException("Configuração de áudio desconhecida.", nameof(key));
            PlayerPrefs.SetFloat(key, Mathf.Clamp01(value)); PlayerPrefs.Save();
            AudioListener.volume = Master;
        }
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Apply() => AudioListener.volume = Master;
    }
}
