using UnityEngine;

namespace FirstRound
{
    /// <summary>
    /// Manages all game audio including sound effects and background music
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    public class AudioManager : MonoBehaviour
    {
        [Header("Sound Effects")]
        [SerializeField] private AudioClip cardFlipSound;      // Swoosh
        [SerializeField] private AudioClip matchSound;         // Chime
        [SerializeField] private AudioClip mismatchSound;      // Buzzer
        [SerializeField] private AudioClip gameOverSound;      // WinningSound
        
        [Header("Background Music")]
        [SerializeField] private AudioClip backgroundMusic;
        [SerializeField] private bool playMusicOnStart = true;
        
        [Header("Volume Settings")]
        [SerializeField] [Range(0f, 1f)] private float sfxVolume = 1f;
        [SerializeField] [Range(0f, 1f)] private float musicVolume = 0.5f;
        
        // Audio sources
        private AudioSource musicSource;
        private AudioSource sfxSource;
        
        // Mute state
        private bool isMusicMuted = false;
        private bool isSfxMuted = false;
        
        #region Initialization
        
        private void Awake()
        {
            SetupAudioSources();
        }
        
        /// <summary>
        /// Sets up audio sources for music and SFX
        /// </summary>
        private void SetupAudioSources()
        {
            // Music source (loops)
            musicSource = gameObject.AddComponent<AudioSource>();
            musicSource.loop = true;
            musicSource.playOnAwake = false;
            musicSource.volume = musicVolume;
            
            // SFX source (one-shots)
            sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.loop = false;
            sfxSource.playOnAwake = false;
            sfxSource.volume = sfxVolume;
        }
        
        /// <summary>
        /// Initializes audio manager and starts music if enabled
        /// </summary>
        public void Initialize()
        {
            if (playMusicOnStart && backgroundMusic != null)
            {
                PlayBackgroundMusic();
            }
            
            Debug.Log("AudioManager initialized");
        }
        
        #endregion
        
        #region Sound Effects
        
        /// <summary>
        /// Plays card flip sound effect
        /// </summary>
        public void PlayCardFlip()
        {
            PlaySFX(cardFlipSound);
        }
        
        /// <summary>
        /// Plays match success sound effect
        /// </summary>
        public void PlayMatch()
        {
            PlaySFX(matchSound);
        }
        
        /// <summary>
        /// Plays mismatch sound effect
        /// </summary>
        public void PlayMismatch()
        {
            PlaySFX(mismatchSound);
        }
        
        /// <summary>
        /// Plays game over sound effect
        /// </summary>
        public void PlayGameOver()
        {
            PlaySFX(gameOverSound);
        }
        
        /// <summary>
        /// Plays a sound effect
        /// </summary>
        /// <param name="clip"></param>
        private void PlaySFX(AudioClip clip)
        {
            if (clip == null)
            {
                Debug.LogWarning("AudioClip is null, cannot play sound");
                return;
            }
            
            if (isSfxMuted)
                return;
            
            sfxSource.PlayOneShot(clip, sfxVolume);
        }
        
        #endregion
        
        #region Background Music
        
        /// <summary>
        /// Plays background music
        /// </summary>
        public void PlayBackgroundMusic()
        {
            if (backgroundMusic == null)
            {
                Debug.LogWarning("Background music is null");
                return;
            }
            
            if (musicSource.isPlaying)
                return;
            
            musicSource.clip = backgroundMusic;
            musicSource.Play();
            
            Debug.Log("Background music started");
        }
        
        /// <summary>
        /// Stops background music
        /// </summary>
        public void StopBackgroundMusic()
        {
            musicSource.Stop();
        }
        
        /// <summary>
        /// Pauses background music
        /// </summary>
        public void PauseBackgroundMusic()
        {
            musicSource.Pause();
        }
        
        /// <summary>
        /// Resumes background music
        /// </summary>
        public void ResumeBackgroundMusic()
        {
            musicSource.UnPause();
        }
        
        #endregion
        
        #region Volume Control
        
        /// <summary>
        /// Sets SFX volume
        /// </summary>
        /// <param name="volume"></param>
        public void SetSFXVolume(float volume)
        {
            sfxVolume = Mathf.Clamp01(volume);
            sfxSource.volume = sfxVolume;
        }
        
        /// <summary>
        /// Sets music volume
        /// </summary>
        /// <param name="volume"></param>
        public void SetMusicVolume(float volume)
        {
            musicVolume = Mathf.Clamp01(volume);
            musicSource.volume = musicVolume;
        }
        
        /// <summary>
        /// Toggles SFX mute
        /// </summary>
        public void ToggleSFXMute()
        {
            isSfxMuted = !isSfxMuted;
            Debug.Log($"SFX muted: {isSfxMuted}");
        }
        
        /// <summary>
        /// Toggles music mute
        /// </summary>
        public void ToggleMusicMute()
        {
            isMusicMuted = !isMusicMuted;
            musicSource.mute = isMusicMuted;
            Debug.Log($"Music muted: {isMusicMuted}");
        }
        
        #endregion
        
        #region Getters
        
        public float GetSFXVolume() => sfxVolume;
        
        public float GetMusicVolume() => musicVolume;
        
        public bool IsSFXMuted() => isSfxMuted;
        
        public bool IsMusicMuted() => isMusicMuted;
        
        #endregion
    }
}