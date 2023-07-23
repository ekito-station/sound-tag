using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using Shapes2D;
using UnityEngine;

public class MessengerController : MonoBehaviourPunCallbacks
{
    public GameObject speakerPrefab;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void GameStart()
    {
        photonView.RPC(nameof(GameStartRPC), RpcTarget.Others);
    }

    [PunRPC]
    public void GameStartRPC()
    {
        RoomManager roomManager = GameObject.FindGameObjectWithTag("RoomManager").GetComponent<RoomManager>();
        roomManager.isStarted = true;
        
        GameObject[] phones = GameObject.FindGameObjectsWithTag("Phone");
        if (phones.Length > 0)
        {
            foreach (GameObject phone in phones)
            {
                PhotonView phonePhotonView = phone.GetComponent<PhotonView>();
                if (!phonePhotonView.IsMine)
                {
                    GameObject speaker =  Instantiate(speakerPrefab, phone.transform.position, Quaternion.identity);
                    speaker.transform.SetParent(phone.transform);                  
                }
            }
        }
        // GameObject[] phones = GameObject.FindGameObjectsWithTag("Phone");
        // if (phones.Length > 0)
        // {
        //     foreach (GameObject phone in phones)
        //     {
        //         GameObject speaker =  Instantiate(speakerPrefab, phone.transform.position, Quaternion.identity);
        //         speaker.transform.SetParent(phone.transform);
        //     }
        // }
    }
}
