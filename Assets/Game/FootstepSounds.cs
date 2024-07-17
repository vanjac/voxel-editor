using System.Collections.Generic;
using UnityEngine;

public class FootstepSounds : MonoBehaviour {
    private const float VOLUME = 0.35f;

    private AudioSource audioSource;

    void Awake() {
        AssetPack.Current().EnsureMaterialSoundsLoaded();
    }

    void Start() {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.volume = VOLUME;
        audioSource.loop = false;
        audioSource.playOnAwake = false;
    }

    private void PlayRandomFromList(List<AudioClip> clips, float volume) {
        AudioClip clip = clips[Random.Range(0, clips.Count)];
        audioSource.PlayOneShot(clip, volume);
    }

    public void PlayLeftFoot(MaterialSound sound) {
        var data = AssetPack.Current().GetMaterialSoundData(sound);
        PlayRandomFromList(data.left, data.volume);
    }

    public void PlayRightFoot(MaterialSound sound) {
        var data = AssetPack.Current().GetMaterialSoundData(sound);
        PlayRandomFromList(data.right, data.volume);
    }
}
