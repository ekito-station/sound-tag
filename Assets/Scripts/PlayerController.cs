using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using Unity.VisualScripting;
using Random = UnityEngine.Random;

// public class PlayerController : MonoBehaviourPunCallbacks
public class PlayerController : MonoBehaviourPunCallbacks, IPunOwnershipCallbacks
{
    [System.NonSerialized] public int score;
    // [System.NonSerialized] public List<GameObject> keys = new List<GameObject>();
    [System.NonSerialized] public List<Vector3> keysPos = new List<Vector3>();

    public AudioSource audioSource1;
    public AudioClip getOneSound;
    public AudioClip getTwoSound;
    public AudioClip getThreeSound;
    public AudioClip escapedSound;
    public AudioClip loseOneSound;
    public AudioClip loseTwoSound;
    public AudioClip loseThreeSound;
    public AudioClip failedSound;
    public AudioSource audioSource2;
    public AudioClip keyGet1Sound;
    public AudioClip keyGet2Sound;
    public AudioClip keyGet3Sound;
    public AudioClip keyGet4Sound;
    public AudioClip eraseSound;
    public AudioSource audioSource3;
    public AudioClip noise1;
    public AudioClip noise2;
    public AudioSource audioSource4;
    public AudioClip wallIndicator;

    private Vector3 prePos;
    public float calcTime = 1.0f;
    public float sqrSpeedTh = 1.0f;

    public RoomManager roomManager;
    public Camera arCamera;
    private GameObject phone;
    private PhoneController phoneController;

    public override void OnEnable() 
    {
        base.OnEnable();
        PhotonNetwork.AddCallbackTarget(this);
    }

    public override void OnDisable() 
    {
        base.OnDisable();
        PhotonNetwork.RemoveCallbackTarget(this);
    }

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine("CalcSpeed");
        prePos = this.transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void InstantiatePhone()
    {
        phone = PhotonNetwork.Instantiate("Phone", this.transform.position, Quaternion.identity);
        phone.transform.SetParent(this.transform);
        phoneController = phone.GetComponent<PhoneController>();
    }

    private void OnTriggerEnter(Collider other) 
    {
        GameObject otherObj = other.gameObject;
        
        // 壁からの音が聞こえ始める
        if (otherObj.tag == "Wall")
        {
            audioSource4.clip = wallIndicator;
            audioSource4.Play();
        }
        
        if (roomManager.pageNum == 0)   // Play
        {
            if (otherObj.tag == "Tagger")
            {
                CaughtByTagger();
            }
            else if (otherObj.tag == "Key")
            {
                GetKey(other.gameObject);
            }
        }
        else if (roomManager.pageNum == 1 && roomManager.modeNum == 2)  // Edit - Delete
        {
            if (otherObj.tag == "Key" || otherObj.tag == "Wall")
            {
                // 音を鳴らす
                audioSource2.PlayOneShot(eraseSound);
                // 衝突したオブジェクトを削除
                // otherObj.GetComponent<PhotonView>().RequestOwnership();
                DestroyObj(otherObj);
            }
            else if (otherObj.tag == "Point")
            {
                audioSource2.PlayOneShot(eraseSound);
                Destroy(otherObj);
            }
        }
    }

    private void OnTriggerStay(Collider other) 
    {
        GameObject otherObj = other.gameObject;
        if (roomManager.pageNum == 1 && roomManager.modeNum == 1)  // Edit - Move
        {
            if (Input.GetMouseButton(0))
            {
                if (otherObj.tag == "Key")
                {
                    // 鍵の位置をプレイヤーの位置に
                    otherObj.transform.position = arCamera.transform.position;
                }
                else if (otherObj.tag == "Wall")
                {
                    // 壁の位置をプレイヤーの位置に
                    Vector3 pos0 = arCamera.transform.position;
                    otherObj.transform.position = new Vector3(pos0.x, 0.0f, pos0.z);
                    // 鍵の向きをプレイヤーに向かせる
                    // otherObj.transform.LookAt(arCamera.transform);
                    otherObj.transform.eulerAngles = new Vector3(0.0f, arCamera.transform.eulerAngles.y, 0.0f);
                }
            }       
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // 壁からの音が止まる
        if (other.gameObject.tag == "Wall")
        {
            audioSource4.Stop();
        }
    }

    private void CaughtByTagger()
    {
        // scoreに応じて音を鳴らす
        if (score == 1)
        {
            audioSource1.PlayOneShot(loseOneSound);
        }
        else if (score == 2)
        {
            audioSource1.PlayOneShot(loseTwoSound);
        }
        else if (score == 3)
        {
            audioSource1.PlayOneShot(loseThreeSound);
        }
        // scoreを0に
        score = 0;
        // // 鍵を元の位置に戻して表示させる
        // if (keys.Count > 0)
        // {
        //     foreach (GameObject key in keys)
        //     {
        //         key.SetActive(true);
        //     }
        //     keys.Clear();
        // }
        // 鍵を元の位置で再生成する
        if (keysPos.Count > 0)
        {
            foreach (Vector3 keyPos in keysPos)
            {
                PhotonNetwork.Instantiate("Key", keyPos, Quaternion.identity);
            }
            keysPos.Clear();
        }
        // 他のプレイヤーに知らせる
#if !UNITY_EDITOR
        phoneController.TellCaught();
#endif
    }

    private void GetKey(GameObject key)
    {
        // scoreを増やす
        score++;
        // scoreに応じて音を鳴らす
        if (score == 1)
        {
            audioSource1.PlayOneShot(getOneSound);
        }
        else if (score == 2)
        {
            audioSource1.PlayOneShot(getTwoSound);
        }
        else if (score == 3)
        {
            audioSource1.PlayOneShot(getThreeSound);
            Invoke("ClearGame", 1.0f);
        }
        // ランダムに音を鳴らす
        int soundType = Random.Range(1, 5);
        switch (soundType)
        {
            case 1:
                audioSource2.PlayOneShot(keyGet1Sound);
                break;
            case 2:
                audioSource2.PlayOneShot(keyGet2Sound);
                break;
            case 3:
                audioSource2.PlayOneShot(keyGet3Sound);
                break;
            case 4:
                audioSource2.PlayOneShot(keyGet4Sound);
                break;
            default:
                break;
        }

        // // 鍵をリストに追加して非表示に
        // keys.Add(key);
        // key.SetActive(false);
        // 鍵の位置をリストに追加して削除
        keysPos.Add(key.transform.position);
        DestroyObj(key);
    }

    private void ClearGame()
    {
        audioSource1.PlayOneShot(escapedSound);
        score = 0;
        // 他のプレイヤーに知らせる
#if !UNITY_EDITOR
        phoneController.TellFailure();
#endif
        // GameObject[] players = GameObject.FindGameObjectsWithTag("Phone");
        // foreach (GameObject player in players)
        // {
        //     PhotonView playerPhotonView = player.GetComponent<PhotonView>();
        //     if (!playerPhotonView.IsMine)
        //     {
        //         GameObject playerCamera = player.transform.parent.gameObject;
        //         AudioSource playerAudioSource = playerCamera.GetComponent<AudioSource>();
        //         playerAudioSource.PlayOneShot(failedSound);
        //     }
        // }
    }

    private void DestroyObj(GameObject obj)
    {
        PhotonView photonViewComponent = obj.GetComponent<PhotonView>();
        if (photonViewComponent.IsMine)
        {
            PhotonNetwork.Destroy(obj);
        }
        else
        {
            photonViewComponent.TransferOwnership(PhotonNetwork.LocalPlayer);
            Debug.Log("TransferOwnership");
        }
    }

    void IPunOwnershipCallbacks.OnOwnershipRequest(PhotonView targetView, Player requestingPlayer) 
    {

    }

    void IPunOwnershipCallbacks.OnOwnershipTransfered(PhotonView targetView, Player previousOwner) 
    {
        PhotonNetwork.Destroy(targetView.gameObject);
        Debug.Log("Destroyed obj.");
    }

    void IPunOwnershipCallbacks.OnOwnershipTransferFailed(PhotonView targetView, Player previousOwner)
    {

    }

    IEnumerator CalcSpeed()
    {
        while (true)
        {
            yield return new WaitForSeconds(1.0f);
            Vector3 curPos = this.transform.position;
            float dist = (curPos - prePos).sqrMagnitude;
            // Debug.Log("dist: " + dist);
            if (dist > sqrSpeedTh)  // 速く移動しすぎた場合
            {
                // ランダムに音を鳴らす
                int soundType = Random.Range(1, 3);
                switch (soundType)
                {
                    case 1:
                        audioSource3.PlayOneShot(noise1);
                        break;
                    case 2:
                        audioSource3.PlayOneShot(noise2);
                        break;
                    default:
                        break;
                }
                // Taggerに知らせる
#if !UNITY_EDITOR
                phoneController.TellToTagger();
#endif
                Debug.Log("TellToTagger");
                // GameObject tagger = GameObject.FindGameObjectWithTag("Tagger");
                // TaggerController taggerController = tagger.GetComponent<TaggerController>();
                // taggerController.ChasePlayer(this.transform.position);
            }
            prePos = curPos;
        }
    }
}