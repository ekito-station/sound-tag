using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class KeyController : MonoBehaviourPunCallbacks
{
    public AudioSource audioSource1;
    public AudioClip key_loop1;
    public AudioClip key_loop2;
    public AudioSource audioSource2;
    public AudioClip keyIndicator;

    // Start is called before the first frame update
    void Awake()
    {
        int soundType = Random.Range(1, 3);
        Debug.Log("Key soundType: " + soundType);
        if (soundType == 1)
        {
            audioSource1.clip = key_loop1;
        }
        else
        {
            audioSource1.clip = key_loop2;
        }
        audioSource1.Play();
    }

    private void OnTriggerEnter(Collider other) 
    {
        if (other.gameObject.tag == "MainCamera")
        {
            audioSource2.clip = keyIndicator;
            audioSource2.Play();
        }
    }

    private void OnTriggerExit(Collider other) 
    {
        if (other.gameObject.tag == "MainCamera")
        {
            audioSource2.Stop();
        }
    }    
}
