using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public AudioClip killSound;
    // Start is called before the first frame update
    public void PlayKillSound()
    {
        this.gameObject.SetActive(true);
        AudioSource audio = GetComponent<AudioSource>();
        audio.clip = killSound;
        audio.Play();
    }
}
