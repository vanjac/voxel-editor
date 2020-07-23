using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FootstepSounds : MonoBehaviour
{
    private const float VOLUME = 0.5f;

    [System.Serializable]
    public struct FootstepSoundEntry
    {
        public MaterialSound sound; // unused!
        public float volume;
        public AudioClip[] left, right;
    }

    public List<FootstepSoundEntry> sounds = new List<FootstepSoundEntry>();
    private AudioSource audioSource;

    void Start()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.volume = VOLUME;
        audioSource.loop = false;
        audioSource.playOnAwake = false;
    }

    private FootstepSoundEntry GetEntry(MaterialSound sound)
    {
        FootstepSoundEntry entry = sounds[(int)sound];
        if (entry.sound != sound)
            Debug.LogError("Incorrect footstep entry!");
        return entry;
    }

    private void PlayRandomFromArray(AudioClip[] clips, float volume)
    {
        AudioClip clip = clips[Random.Range(0, clips.Length)];
        audioSource.PlayOneShot(clip, volume);
    }

    public void PlayLeftFoot(MaterialSound sound)
    {
        FootstepSoundEntry entry = GetEntry(sound);
        PlayRandomFromArray(entry.left, entry.volume);
    }

    public void PlayRightFoot(MaterialSound sound)
    {
        FootstepSoundEntry entry = GetEntry(sound);
        PlayRandomFromArray(entry.right, entry.volume);
    }
}
