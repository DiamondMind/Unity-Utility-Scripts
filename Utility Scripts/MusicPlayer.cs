using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DiamondMind.Prototypes.Tools
{
    /// <summary>
    /// The MusicPlayer class handles the sequential playback of audio clips from an array of sound effects (SFX).
    /// It automatically plays the next clip in the array once the current one finishes.
    /// </summary>
    public class MusicPlayer : MonoBehaviour
    {
        public AudioSource audioSource;
        public AudioClip[] sfxArray;

        int currentSFXIndex;

        private void Awake()
        {
            if (sfxArray.Length > 0 && audioSource != null)
            {
                audioSource.clip = sfxArray[currentSFXIndex];
                audioSource.Play();  
            }
        }

        private void Update()
        {
            // Check if the current SFX has finished playing
            if (!audioSource.isPlaying && sfxArray.Length > 0)
            {
                currentSFXIndex = (currentSFXIndex + 1) % sfxArray.Length;

                audioSource.clip = sfxArray[currentSFXIndex];
                audioSource.Play();  
            }
        }
    }
}
