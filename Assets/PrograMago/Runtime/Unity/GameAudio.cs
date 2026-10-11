using System.Collections.Generic;
using PrograMago.Domain;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace PrograMago.UnityIntegration
{
    public enum GameSound
    {
        Hover, Click, MapOpen, GrimoireOpen, PageTurn, MapStep,
        NormalSpell, FireSpell, WaterSpell, ElectricSpell, Damage, Victory, Defeat, Completion
    }

    // A single lifetime for music and effects, including additive navigation scenes.
    public sealed class GameAudio : MonoBehaviour
    {
        private static GameAudio instance;
        private readonly Dictionary<GameSound, AudioClip> clips = new Dictionary<GameSound, AudioClip>();
        private readonly Dictionary<GameSound, float> lastPlayed = new Dictionary<GameSound, float>();
        private readonly AudioSource[] voices = new AudioSource[8];
        private readonly bool[] resultVoices = new bool[8];
        private readonly float[] voiceGains = new float[8];
        public static float HoverGainForScene(string scene) => scene == MainMenuView.SceneName ? 1f : .5f;
        private AudioSource music;
        private float duckUntil;
        private bool initialized;

        public static GameAudio Instance
        {
            get {
                if (instance == null) new GameObject("GameAudio").AddComponent<GameAudio>();
                return instance;
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => instance = null;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize() { _ = Instance; }

        private void Awake()
        {
            if (instance != null && instance != this) { Destroy(gameObject); return; }
            instance = this;
            DontDestroyOnLoad(gameObject);
            music = CreateSource("Music", true);
            music.volume = GameAudioSettings.Music * .3f;
            music.clip = Resources.Load<AudioClip>("Audio/Music");
            if (music.clip != null) music.Play();
            foreach (GameSound sound in System.Enum.GetValues(typeof(GameSound))) {
                string file = sound == GameSound.Click ? "Hover" : sound == GameSound.Completion ? "Victory" :
                    sound == GameSound.Defeat ? "Damage" : sound.ToString();
                clips[sound] = Resources.Load<AudioClip>("Audio/" + file);
            }
            for (int i = 0; i < voices.Length; i++) voices[i] = CreateSource("Effect " + i, false);
            gameObject.AddComponent<AudioListener>();
            GameAudioSettings.Changed += ApplyVolumes;
            SceneManager.sceneLoaded += SceneLoaded;
            initialized = true;
            ConfigureScene();
            ApplyVolumes();
        }

        private AudioSource CreateSource(string name, bool loop)
        {
            var item = new GameObject(name);
            item.transform.SetParent(transform);
            var source = item.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.loop = loop;
            source.spatialBlend = 0;
            source.ignoreListenerPause = true;
            return source;
        }

        private void SceneLoaded(Scene scene, LoadSceneMode mode) => ConfigureScene();

        private void ConfigureScene()
        {
            foreach (var listener in FindObjectsByType<AudioListener>(FindObjectsSortMode.None))
                if (listener.gameObject != gameObject) listener.enabled = false;
            foreach (var button in FindObjectsByType<Button>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                if (button.GetComponent<ButtonJuice>() == null) button.gameObject.AddComponent<ButtonJuice>();
        }

        public static bool Play(GameSound sound) => Instance.PlayEffect(sound);

        public static bool PlaySpell(CombatElement element) => Play(element switch {
            CombatElement.Fire => GameSound.FireSpell,
            CombatElement.Water => GameSound.WaterSpell,
            CombatElement.Electric => GameSound.ElectricSpell,
            _ => GameSound.NormalSpell
        });

        private bool PlayEffect(GameSound sound)
        {
            if (!initialized || !clips.TryGetValue(sound, out var clip) || clip == null ||
                GameAudioSettings.Master <= 0 || GameAudioSettings.Effects <= 0) return false;
            float now = Time.unscaledTime;
            float interval = sound == GameSound.MapStep ? .12f : sound == GameSound.Hover ? .06f : .035f;
            if (lastPlayed.TryGetValue(sound, out float last) && now - last < interval) return false;
            bool result = sound == GameSound.Victory || sound == GameSound.Defeat || sound == GameSound.Completion;
            int slot = System.Array.FindIndex(voices, source => !source.isPlaying);
            if (slot < 0 && result) {
                slot = System.Array.FindIndex(resultVoices, value => !value);
                if (slot >= 0) voices[slot].Stop();
            }
            if (slot < 0) return false;
            lastPlayed[sound] = now;
            resultVoices[slot] = result;
            voiceGains[slot] = sound == GameSound.Hover ? HoverGainForScene(SceneManager.GetActiveScene().name) : 1f;
            voices[slot].clip = clip;
            if (result) duckUntil = now + Mathf.Min(clip.length, 3f);
            voices[slot].volume = GameAudioSettings.Effects * .65f;
            voices[slot].Play();
            ApplyVolumes();
            return true;
        }

        private void Update() => ApplyVolumes();

        private void ApplyVolumes()
        {
            if (music == null) return;
            int count = 0;
            bool resultPlaying = false;
            for (int i = 0; i < voices.Length; i++) {
                if (voices[i] == null || !voices[i].isPlaying) continue;
                count++;
                resultPlaying |= resultVoices[i];
            }
            music.volume = GameAudioSettings.Music * .3f * (resultPlaying && Time.unscaledTime < duckUntil ? .4f : 1f);
            // The sum of voice gains stays <= .65 even during a burst of attacks.
            float gain = GameAudioSettings.Effects * .65f / Mathf.Max(1, count);
            for (int i = 0; i < voices.Length; i++) if (voices[i] != null) voices[i].volume = gain * voiceGains[i];
        }

        private void OnDestroy()
        {
            if (instance != this) return;
            GameAudioSettings.Changed -= ApplyVolumes;
            SceneManager.sceneLoaded -= SceneLoaded;
            instance = null;
        }
    }
}
