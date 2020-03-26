using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class voiceTrigger : MonoBehaviour
{
    public AudioClip voiceOver;
    AudioSource audioSource;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    void OnTriggerEnter(Collider other)
    {
        if (voiceOver != null)
        {
            audioSource.PlayOneShot(voiceOver, 0.7f);
        }
    }
}
