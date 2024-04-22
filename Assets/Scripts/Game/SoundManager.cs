using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public AudioClip killSound;
    // Start is called before the first frame update
    public void PlayKillSound()
    {
        AudioSource audio = GetComponent<AudioSource>();
        audio.clip = killSound;
        audio.Play();
    }

    public void PlayMoveSound(UnitController unit)
    {
        int n = Random.Range(0,unit.moveSounds.Count);
        AudioSource audio = GetComponent<AudioSource>();
        audio.clip = unit.moveSounds[n];
        audio.Play();
    }
}
