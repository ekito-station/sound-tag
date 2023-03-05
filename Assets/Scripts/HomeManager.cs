using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;
using Photon.Realtime;

public class HomeManager : MonoBehaviourPunCallbacks
{
    public Camera arCamera;
    public Slider distSlider;
    public TextMeshProUGUI distText;
    public Slider speedSlider;
    public TextMeshProUGUI speedText;
    public GameObject it;
    public GameObject player;
    public GameObject itPanel;
    public GameObject itButton;
    public GameObject playerPanel;
    public GameObject playerButton;
    [System.NonSerialized] public float dist;
    [System.NonSerialized] public float speed;
    private bool isIt = true;

    // Start is called before the first frame update
    void Start()
    {
        PhotonNetwork.ConnectUsingSettings();

        OnDistSliderValueChanged();
        OnSpeedSliderValueChanged();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public override void OnConnectedToMaster()
    {
        PhotonNetwork.JoinOrCreateRoom("Room", new RoomOptions(), TypedLobby.Default);
    }

    public void OnResetButtonClicked()
    {
        SceneManager.LoadScene("Home");
    }

    public void OnItButtonClicked()
    {
        isIt = true;
        itPanel.SetActive(true);
        itButton.SetActive(false);
        playerPanel.SetActive(false);
        playerButton.SetActive(true);
    }

    public void OnPlayerButtonClicked()
    {
        isIt = false;
        itPanel.SetActive(false);
        itButton.SetActive(true);
        playerPanel.SetActive(true);
        playerButton.SetActive(false);
    }

    public void OnDistSliderValueChanged()
    {
        dist = distSlider.value;
        distText.text = dist.ToString() + "m";
    }

    public void OnSpeedSliderValueChanged()
    {
        speed = speedSlider.value;
        speedText.text = speed.ToString();
    }

    public void PutSound()
    {
        Transform camTrans = arCamera.transform;
        Vector3 soundPos = camTrans.position + dist * camTrans.forward;
        if (isIt)
        {
            // Instantiate(it, soundPos, Quaternion.identity);
            PhotonNetwork.Instantiate("It", soundPos, Quaternion.identity);
        }
        else
        {
            // Instantiate(player, soundPos, Quaternion.identity);
            PhotonNetwork.Instantiate("Player", soundPos, Quaternion.identity);
        }
    }
}
