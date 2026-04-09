using UnityEngine;

namespace APIExample
{
    public class CharacterAudioPlayer : MonoBehaviour
    {
        // Not serialize
        private AudioSource _audioSource;

        private void Awake() => _audioSource = GetComponent<AudioSource>();

        private void Start() => _audioSource.clip = null;

        public void SetAudioClip(AudioClip clip) => _audioSource.clip = clip;

        public void PlayCurrentAudioClip() 
        {
            if (_audioSource.clip == null)
            {
                Debug.LogError("[CharacterAudioPlayer] audio clip ref is null!");                
                return;
            }

            _audioSource.Play();
        }
    }
}
