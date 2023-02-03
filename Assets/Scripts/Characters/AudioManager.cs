using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Characters
{
    [Serializable]
    public class ClipInfo
    {
        public string name;
        public AudioClip sound;
    }
    
    [RequireComponent(typeof(AudioSource))]
    public class AudioManager : MonoBehaviour
    {
        [SerializeField] 
        private List<ClipInfo> sounds;
        private AudioSource _audioSource;
        
        private void Start()
        {
            _audioSource = GetComponent<AudioSource>();
        }

        public void PlaySound(string clipName)
        {
            PlaySoundWithRandomPitch(clipName, 1f, 1f);
        }
        
        public void PlaySoundWithRandomPitch(string clipName, float minPitch, float maxPitch)
        {
            var clipInfo = sounds.Find(clipInfo => clipInfo.name == clipName);
            if (clipInfo == null)
                throw new KeyNotFoundException($"Not found '{clipName}' clip in registered list");

            _audioSource.pitch = Random.Range(minPitch, maxPitch);
            _audioSource.PlayOneShot(clipInfo.sound);
        }
    }
}
