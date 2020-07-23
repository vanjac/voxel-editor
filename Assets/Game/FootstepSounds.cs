using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FootstepSounds : MonoBehaviour
{
    [System.Serializable]
    public struct FootstepSoundEntry
    {
        public MaterialSound sound; // unused!
        public float volume;
        public AudioClip[] left, right;
    }
    public List<FootstepSoundEntry> sounds = new List<FootstepSoundEntry>();
}
