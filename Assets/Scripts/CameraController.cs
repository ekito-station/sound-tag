using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public AudioSource audioSource;
    public AudioClip catchSound;
    public AudioClip caughtSound;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other) {
        if (other.gameObject.tag == "It") audioSource.PlayOneShot(caughtSound);
        if (other.gameObject.tag == "Player") audioSource.PlayOneShot(catchSound);
    }
}
