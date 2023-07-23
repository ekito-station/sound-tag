using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PhoneController : MonoBehaviourPunCallbacks
{
    public AudioClip failedSound;
    public AudioClip caughtSound;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void TellFailure()
    {
        photonView.RPC(nameof(GameFailed), RpcTarget.Others);
    }

    [PunRPC]
    public void GameFailed()
    {
        // 脱出失敗の音声を流す
        GameObject arCamera = GameObject.FindGameObjectWithTag("MainCamera");
        AudioSource audioSource = arCamera.GetComponent<AudioSource>();
        audioSource.PlayOneShot(failedSound);
        // スコアをリセットする
        PlayerController playerController = arCamera.GetComponent<PlayerController>();
        playerController.score = 0;
    }

    public void TellCaught()
    {
        Debug.Log("TellCaught");
        photonView.RPC("OtherCaught", RpcTarget.Others);
    }

    [PunRPC]
    public void OtherCaught()
    {
        Debug.Log("OtherCaught");
#if !UNITY_EDITOR
        GameObject speaker = GameObject.FindGameObjectWithTag("Speaker");
        AudioSource audioSource = speaker.GetComponent<AudioSource>();
        audioSource.PlayOneShot(caughtSound);
        Debug.Log("Played caughtSound");
#endif
    }

    public void TellToTagger()
    {
        photonView.RPC(nameof(TaggerChasePlayer), RpcTarget.All);
    }

    [PunRPC]
    public void TaggerChasePlayer()
    {
        GameObject tagger = GameObject.FindGameObjectWithTag("Tagger");
        TaggerController taggerController = tagger.GetComponent<TaggerController>();
        taggerController.ChasePlayer(this.transform.position);
        Debug.Log("ChasePlayer: " + this.transform.position);
    }
}
