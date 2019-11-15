
using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using SimpleJSON;

namespace octopussy
{
    /*
    A playlist
    */
    public class PlayList
    {
        int previous = -1;
        public int next = 0;

        float _pitchlow = -0.07f;
        float _pitchhigh = +0.07f;
        float pitchlow;
        float pitchhigh;

        public float pitchshift
        {
            set
            {
                pitchlow = 1.0f + value - _pitchlow;
                pitchhigh = 1.0f + value + _pitchhigh;
            }
        }

        readonly List<NamedAudioClip> audioClips;

        public PlayList(List<NamedAudioClip> clips, float _shift = 0f)
        {
            audioClips = clips;
            pitchshift = _shift;
        }

        public void playRandomDelayedIfClear(AudioSource audioSource, float volume = 1.0f, float minDelay = 0, float maxDelay = 1)
        {
            playDelayedIfClear(audioSource, volume, UnityEngine.Random.Range(minDelay, maxDelay));
        }

        public void playDelayedIfClear(AudioSource audioSource, float volume = 1.0f, float delay = 0)
        {
            Debug.Log(audioClips.Count);
            Debug.Log(next);
            NamedAudioClip nac = audioClips[next];

            if (audioSource != null && nac.clipToPlay != null)
            {
                if (!audioSource.isPlaying)
                {
                    audioSource.clip = nac.clipToPlay;
                    audioSource.volume = volume;
                    audioSource.pitch = UnityEngine.Random.Range(pitchlow, pitchhigh);
                    audioSource.PlayDelayed(delay);
                    previous = next;
                    next = (int)UnityEngine.Random.Range(0f, (float)audioClips.Count - 1);
                    // take next if its the same again
                    if (next == previous) next = (next + 1) % audioClips.Count;
                }
            }
        }

        public void playRandom(AudioSource audioSource, float volume = 1.0f, float delay = 0)
        {
            NamedAudioClip nac = audioClips[next];
            if (audioSource != null && nac.clipToPlay != null)
            {
                audioSource.clip = audioClips[next].clipToPlay;
                audioSource.volume = volume;
                audioSource.pitch = UnityEngine.Random.Range(pitchlow, pitchhigh);
                audioSource.PlayDelayed(delay);
                previous = next;
                next = (int)UnityEngine.Random.Range(0f, (float)audioClips.Count - 1);
                // take next if its the same again
                if (next == previous) next = (next + 1) % audioClips.Count;
            }
        }

        public void playNext(AudioSource audioSource, float volume = 1.0f)
        {
            NamedAudioClip nac = audioClips[next];
            audioSource.clip = nac.clipToPlay;
            audioSource.volume = volume;
            audioSource.pitch = UnityEngine.Random.Range(pitchlow, pitchhigh);
            next = (next + 1) % audioClips.Count;
            audioSource.Play();
        }
    }
}
