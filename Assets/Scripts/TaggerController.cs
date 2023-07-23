using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class TaggerController : MonoBehaviourPunCallbacks
{
    [System.NonSerialized] public bool isWalking;
    private Vector3 target;

    public AudioSource audioSource1;
    public AudioClip walkSound;
    public AudioClip runSound;
    public AudioSource audioSource2;
    public AudioClip searchVoice1;
    public AudioClip searchVoice2;
    public AudioClip searchVoice3;
    public AudioClip spotVoice1;
    public AudioClip spotVoice2;
    public AudioClip spotVoice3;

    [System.NonSerialized] public Vector3 dir;
    public float walkSpeed = 0.01f;
    public float runSpeed = 0.01f;
    public float moveTime = 2.0f;   // > 1.0f
    public float runTime = 2.0f;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (isWalking)
        {
            transform.Translate(dir.x, dir.y, dir.z);
        }
        else
        {
            this.transform.position = Vector3.MoveTowards(this.transform.position, target, runSpeed);
        }
    }

    public void StartTagger()
    {
        StartCoroutine("MoveTagger");
        isWalking = true;
        photonView.RPC(nameof(PlayTaggerSound), RpcTarget.All, true);
        // audioSource1.clip = walkSound;
        // audioSource1.Play();


    }
    // public void StopTagger()
    // {
    //     StopCoroutine("MoveTagger");
    // }
    [PunRPC]
    public void PlayTaggerSound(bool _isWalking)
    {
        GameObject tagger = GameObject.FindGameObjectWithTag("Tagger");
        AudioSource audioSource = tagger.GetComponent<AudioSource>();
        if (_isWalking)
        {
            audioSource.clip = walkSound;
        }
        else
        {
            audioSource.clip = runSound;
        }
        audioSource.Play();
    }

    IEnumerator MoveTagger()
    {
        while (true)
        {
            if (isWalking)
            {
                // 声を流す
                photonView.RPC(nameof(PlayTaggerVoice), RpcTarget.All, true);
                // int soundType = Random.Range(1, 4);
                // switch (soundType)
                // {
                //     case 1:
                //         audioSource2.PlayOneShot(searchVoice1);
                //         break;
                //     case 2:
                //         audioSource2.PlayOneShot(searchVoice2);
                //         break;
                //     case 3:
                //         audioSource2.PlayOneShot(searchVoice3);
                //         break;
                //     default:
                //         break;
                // }
                // 向きを決める
                int randNum = Random.Range(0, 8);
                switch (randNum)
                {
                    case 0:
                        dir = new Vector3(1.0f, 0.0f, 0.0f);
                        break;
                    case 1:
                        dir = new Vector3(0.7f, 0.0f, -0.7f);
                        break;
                    case 2:
                        dir = new Vector3(0.0f, 0.0f, -1.0f);
                        break;
                    case 3:
                        dir = new Vector3(-0.7f, 0.0f, -0.7f);
                        break;
                    case 4:
                        dir = new Vector3(-1.0f, 0.0f, 0.0f);
                        break;
                    case 5:
                        dir = new Vector3(-0.7f, 0.0f, 0.7f);
                        break;
                    case 6:
                        dir = new Vector3(0.0f, 0.0f, 1.0f);
                        break;
                    case 7:
                        dir = new Vector3(0.7f, 0.0f, 0.7f);
                        break;
                    default:
                        break;
                }
                dir *= walkSpeed;
                // target = this.transform.position + dir;
            }
            else
            {
                // 声を流す
                photonView.RPC(nameof(PlayTaggerVoice), RpcTarget.All, false);
                // int soundType = Random.Range(1, 4);
                // switch (soundType)
                // {
                //     case 1:
                //         audioSource2.PlayOneShot(spotVoice1);
                //         break;
                //     case 2:
                //         audioSource2.PlayOneShot(spotVoice2);
                //         break;
                //     case 3:
                //         audioSource2.PlayOneShot(spotVoice3);
                //         break;
                //     default:
                //         break;
                // }
            }
            yield return new WaitForSeconds(moveTime);
        }
    }

    [PunRPC]
    public void PlayTaggerVoice(bool _isWalking)
    {
        GameObject tagger = GameObject.FindGameObjectWithTag("Tagger");
        AudioSource audioSource = tagger.GetComponent<AudioSource>();

        if (_isWalking)
        {
            int soundType = Random.Range(1, 4);
            switch (soundType)
            {
                case 1:
                    audioSource2.PlayOneShot(searchVoice1);
                    break;
                case 2:
                    audioSource2.PlayOneShot(searchVoice2);
                    break;
                case 3:
                    audioSource2.PlayOneShot(searchVoice3);
                    break;
                default:
                    break;
            }
        }
        else
        {
            int soundType = Random.Range(1, 4);
            switch (soundType)
            {
                case 1:
                    audioSource2.PlayOneShot(spotVoice1);
                    break;
                case 2:
                    audioSource2.PlayOneShot(spotVoice2);
                    break;
                case 3:
                    audioSource2.PlayOneShot(spotVoice3);
                    break;
                default:
                    break;
            }
        }
    }

    private void OnTriggerEnter(Collider other) 
    {
        if (other.gameObject.tag == "Wall")
        {
            if (isWalking)
            {
                // 壁で反射させる
                Vector3 normal = other.gameObject.transform.forward;
                dir = Vector3.Reflect(dir, normal);
            }
            else
            {

                isWalking = true;
            }
        }
    }

    public void ChasePlayer(Vector3 _target)
    {
        isWalking = false;
        target = _target;
        // 音を変える
        photonView.RPC(nameof(PlayTaggerSound), RpcTarget.All, false);
        // audioSource1.clip = runSound;
        // audioSource1.Play();
        // 一定時間後に戻る
        Invoke("StopChase", runTime);
    }

    public void StopChase()
    {
        isWalking = true;
        // 音を戻す
        photonView.RPC(nameof(PlayTaggerSound), RpcTarget.All, true);
        // audioSource1.clip = walkSound;
        // audioSource1.Play();
    }
}
