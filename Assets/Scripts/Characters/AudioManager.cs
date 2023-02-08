using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
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
        [FormerlySerializedAs("sounds")]
        [SerializeField] private List<ClipInfo> callableSounds;
        [SerializeField] private List<AudioClip> ambientSounds;
        
        [SerializeField] [Range(0, 100)] private float minPauseTime = 2f;
        [SerializeField] [Range(0, 100)] private float maxPauseTime = 4f;
        
        [SerializeField] [Range(0.01f, 2)] private float ambientMultiplier = 0.5f;
        
        private AudioSource _audioSource;
        private float _originalVolume;

        private void Start()
        {
            _audioSource = GetComponent<AudioSource>();
            _originalVolume = _audioSource.volume;

            if (ambientSounds.Count > 0)
            {
                StartCoroutine(UpdateAmbient());
            }
        }
        
        private void OnValidate()
        {
            if (minPauseTime > maxPauseTime)
                minPauseTime = maxPauseTime - 0.1f;
        }
        
        private IEnumerator UpdateAmbient()
        {
            while (true)
            {
                var randomOffset = Random.Range(minPauseTime, maxPauseTime);
                
                yield return new WaitWhile(() => _audioSource.isPlaying);
                yield return new WaitForSeconds(randomOffset);
                
                var randomIndex = Random.Range(0, ambientSounds.Count);
                PlayAmbientSound(randomIndex);
            }

            yield return null;
        }
        
        private void PlayAmbientSound(int index)
        {
            if (index < 0 || index >= ambientSounds.Count)
                throw new IndexOutOfRangeException("Wrong index for ambientSound!");
            
            var audioClip = ambientSounds[index];
            PlaySoundInternal(audioClip, 1f, 1f, false);
        }

        public void PlaySound(string clipName)
        {
            PlaySoundWithRandomPitch(clipName, 1f, 1f);
        }

        public void PlaySoundWithRandomPitch(string clipName, float minPitch, float maxPitch)
        {
            var clipInfo = callableSounds.Find(clipInfo => clipInfo.name == clipName);
            if (clipInfo == null)
                throw new KeyNotFoundException($"Not found '{clipName}' clip in registered list");
            
            PlaySoundInternal(clipInfo.sound, minPitch, maxPitch, true);
        }

        private void PlaySoundInternal(AudioClip audioClip, float minPitch, float maxPitch, bool highPriority)
        {
            if (_audioSource.isPlaying && !highPriority)
                return;
            
            _audioSource.volume = highPriority ? _originalVolume : (_originalVolume * ambientMultiplier);
            _audioSource.pitch = Random.Range(minPitch, maxPitch);
            _audioSource.PlayOneShot(audioClip);
        }
    }
}
