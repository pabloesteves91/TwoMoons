using System.Collections.Generic;
using UnityEngine;

namespace TwoMoons
{
    /// <summary>Spielt Musik und Soundeffekte. Die Lautstärken werden gespeichert.</summary>
    public class SoundManager : MonoBehaviour
    {
        const string MusicVolumeKey = "TwoMoons.MusicVolume";
        const string EffectsVolumeKey = "TwoMoons.EffectsVolume";

        static SoundManager instance;
        static AudioLibrary library;
        static readonly Dictionary<string, AudioClip> cache = new Dictionary<string, AudioClip>();

        AudioSource music;
        AudioSource effects;

        public static float MusicVolume
        {
            get => PlayerPrefs.GetFloat(MusicVolumeKey, 0.5f);
            set
            {
                PlayerPrefs.SetFloat(MusicVolumeKey, Mathf.Clamp01(value));
                if (instance != null) instance.music.volume = MusicVolume;
            }
        }

        public static float EffectsVolume
        {
            get => PlayerPrefs.GetFloat(EffectsVolumeKey, 0.8f);
            set
            {
                PlayerPrefs.SetFloat(EffectsVolumeKey, Mathf.Clamp01(value));
                if (instance != null) instance.effects.volume = EffectsVolume;
            }
        }

        public static void Init(AudioLibrary audioLibrary)
        {
            library = audioLibrary;
            cache.Clear();
        }

        void Awake()
        {
            instance = this;
            music = gameObject.AddComponent<AudioSource>();
            music.loop = true;
            music.playOnAwake = false;
            music.volume = MusicVolume;
            effects = gameObject.AddComponent<AudioSource>();
            effects.playOnAwake = false;
            effects.volume = EffectsVolume;
        }

        void Start()
        {
            music.clip = Get(Sfx.Music);
            music.Play();
        }

        void OnDestroy()
        {
            if (instance == this) instance = null;
            PlayerPrefs.Save();
        }

        public static void Play(string key)
        {
            if (instance == null) return;
            instance.effects.PlayOneShot(Get(key));
        }

        static AudioClip Get(string key)
        {
            if (cache.TryGetValue(key, out var clip) && clip != null) return clip;

            if (library != null)
            {
                var field = typeof(AudioLibrary).GetField(key);
                clip = field?.GetValue(library) as AudioClip;
            }
            if (clip == null) clip = PlaceholderSounds.CreateClip(key);

            cache[key] = clip;
            return clip;
        }
    }
}
