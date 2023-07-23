using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using Shapes2D;
using UnityEngine.XR.ARFoundation;
using TMPro;

public class RoomManager : MonoBehaviourPunCallbacks
{
    // 位置合わせ用
    [SerializeField] private ARSessionOrigin sessionOrigin;
    [SerializeField] private ARTrackedImageManager imageManager;
    private GameObject worldOrigin;    // ワールドの原点として振る舞うオブジェクト
    private Coroutine originCoroutine;
    // public GameObject checkTracking;
    public float trackInterval = 1.0f;
    private bool isTracked = false;

    public PlayerController playerController;

    public Camera arCamera;
    public float dist;

    public GameObject adminCanvas;
    public TextMeshProUGUI pageText;
    public TextMeshProUGUI modeText;
    public GameObject startButton;
    public GameObject stopButton;

    [System.NonSerialized] public int pageNum;    // 0: Play, 1: Edit
    [System.NonSerialized] public int modeNum;    // 0: Create, 1: Move, 2: Delete

    private float fingerPosX0;
    private float fingerPosX1;
    public float posThX;
    private float fingerPosY0;
    private float fingerPosY1;
    public float posThY;

    private int tapCount;
    private bool isDoubleTap;
    public float doubleTapTh = 0.5f;

    private bool drawingWall;
    private Vector3 prePos;

    public AudioSource camAudioSource;
    public AudioClip playVoice;
    public AudioClip editVoice;
    public AudioClip createVoice;
    public AudioClip moveVoice;
    public AudioClip deleteVoice;
    public AudioClip introVoice;
    public AudioClip pointSound;

    public GameObject pointPrefab;

    private GameObject tagger;
    private TaggerController taggerController;

    [System.NonSerialized] public bool isStarted;

    private MessengerController messegnerCtrl;

    // 位置合わせ
    public override void OnEnable()
    {
        base.OnEnable();
        worldOrigin = new GameObject("Origin");
        Debug.Log("Created origin.");
        imageManager.trackedImagesChanged += OnTrackedImagesChanged;
    }

    public override void OnDisable()
    {
        base.OnDisable();
        imageManager.trackedImagesChanged -= OnTrackedImagesChanged;
    }

    private IEnumerator OriginDecide(ARTrackedImage trackedImage, float trackInterval)
    {
        yield return null;
        Debug.Log("OriginDecide");
        Debug.Log("isTracked " + isTracked);

        if (!isTracked)
        {
            isTracked = true;
            Invoke("Retracking", trackInterval);

            var trackedImageTransform = trackedImage.transform;
            worldOrigin.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
            // 原点をマーカーの位置に移す
            sessionOrigin.MakeContentAppearAt(worldOrigin.transform, trackedImageTransform.position, trackedImageTransform.localRotation);
            Debug.Log("Adjusted the origin.");
        }
        originCoroutine = null;
        // checkTracking.SetActive(false);
    }

    private void Retracking()
    {
        Debug.Log("Retracking");
        isTracked = false;
    }

    // // ワールド座標を任意の点から見たローカル座標に変換
    // public Vector3 WorldToOriginLocal(Vector3 world)    // worldはワールド座標
    // {
    //     return worldOrigin.transform.InverseTransformDirection(world);
    // }

    // TrackedImagesChanged時の処理
    private void OnTrackedImagesChanged(ARTrackedImagesChangedEventArgs eventArgs)  // eventArgsは検出イベントに関する引数
    {
        foreach (var trackedImage in eventArgs.added)
        {
            // checkTracking.SetActive(true);
            StartCoroutine(OriginDecide(trackedImage, 0));
            // デバイスを振動させる
            if (SystemInfo.supportsVibration)
            {
                Handheld.Vibrate();
            }
        }

        foreach (var trackedImage in eventArgs.updated)
        {
            if (originCoroutine == null)
            {
                // checkTracking.SetActive(true);
                originCoroutine = StartCoroutine(OriginDecide(trackedImage, trackInterval));
                // デバイスを振動させる
                if (SystemInfo.supportsVibration)
                {
                    Handheld.Vibrate();
                }
            }
        }
    }
    
    // Start is called before the first frame update
    void Start()
    {
        PhotonNetwork.ConnectUsingSettings();

#if UNITY_EDITOR
        adminCanvas.SetActive(true);
#else
        adminCanvas.SetActive(false);
#endif
        // adminCanvas.SetActive(true);
    }

    public override void OnConnectedToMaster()
    {
        PhotonNetwork.JoinOrCreateRoom("Room", new RoomOptions(), TypedLobby.Default);
    }

    public override void OnJoinedRoom()
    {
#if !UNITY_EDITOR
        playerController.InstantiatePhone();
#else
        GameObject messegner = PhotonNetwork.Instantiate("Messenger", Vector3.zero, Quaternion.identity);
        messegnerCtrl = messegner.GetComponent<MessengerController>();
#endif
    }    

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            fingerPosY0 = Input.mousePosition.y;
            fingerPosX0 = Input.mousePosition.x;
        }
        if (Input.GetMouseButtonUp(0))
        {
            fingerPosY1 = Input.mousePosition.y;
            float diffY = fingerPosY1 - fingerPosY0;
            if (Mathf.Abs(diffY) > posThY)  // Swipe vertically
            {
                if (diffY > 0) pageNum--;
                else pageNum++;

                if (pageNum == -1) pageNum = 1;
                else if (pageNum == 2) pageNum = 0;

                ChangePage();
                return;
            }

            fingerPosX1 = Input.mousePosition.x;
            float diffX = fingerPosX1 - fingerPosX0;
            if (Mathf.Abs(diffX) > posThX)  // Swipe horizontally
            {
                if (pageNum == 1)   // Edit Page
                {
                    if (diffX > 0) modeNum--;
                    else modeNum++;

                    if (modeNum == -1) modeNum = 2;
                    else if (modeNum == 3) modeNum = 0;

                    ChangeMode();
                    return;
                }
            }
            else    // Tap or Double Tap
            {
                tapCount++;
                Invoke("DoubleTap", doubleTapTh);
            }
        }
    }

    public void ChangePage()
    {
        drawingWall = false;
        switch (pageNum)
        {
            case 0:
                pageText.text = "Play";
                modeText.text = "";
                camAudioSource.PlayOneShot(playVoice);
                break;
            case 1:
                pageText.text = "Edit";
                camAudioSource.PlayOneShot(editVoice);

                modeNum = 0;
                Invoke("ChangeMode", 1.0f);
                break;
        }
    }

    public void ChangeMode()
    {
        drawingWall = false;
        if (pageNum == 1)
        {
            switch (modeNum)
            {
                case 0:
                    modeText.text = "Create";
                    camAudioSource.PlayOneShot(createVoice);
                    break;
                case 1:
                    modeText.text = "Move";
                    camAudioSource.PlayOneShot(moveVoice);
                    break;
                case 2:
                    modeText.text = "Delete";
                    camAudioSource.PlayOneShot(deleteVoice);
                    break;
            }
        }
    }

    public Vector3 CalcPos()
    {
        Transform camTrans = arCamera.transform;
        Vector3 pos = camTrans.position + dist * camTrans.forward;
        return pos;
    }

    private void DoubleTap()
    {
        Vector3 curPos = CalcPos();
        if (tapCount < 2)   // Single Tap
        {
            if (!isDoubleTap)
            {
                if (pageNum == 1 && modeNum == 0)   // Edit - Create
                {
                    // 壁を描く
                    if (!drawingWall)
                    {
                        StartDrawingWall(curPos);
                    }
                    else
                    {
                        DrawWall(prePos, curPos);
                    }
                }
#if !UNITY_EDITOR
                else if (pageNum == 0)  // Play
                {
                    if (!isStarted)
                    {
                        camAudioSource.clip = introVoice;
                        camAudioSource.Play();
                    }
                    else
                    {
                        // 番人を避けるための壁を描く
                        if (!drawingWall)
                        {
                            StartDrawingWall(curPos);
                        }
                        else
                        {
                            DrawWall(prePos, curPos);
                            drawingWall = false;
                        }
                    }
                }
#endif
            }
            isDoubleTap = false;
        }
        else    // Double Tap
        {
            isDoubleTap = true;
            if (pageNum == 1 && modeNum == 0)
            {
                if (drawingWall)
                {
                    // 壁を描くのを終える
                    DrawWall(prePos, curPos);
                    drawingWall = false;
                }
                else
                {
                    // 鍵を置く
                    PlaceKey();
                }
            }
        }
        tapCount = 0;
    }

    public void StartDrawingWall(Vector3 _curPos)
    {
        prePos = _curPos;
        drawingWall = true;
        // Pointを置く
        Vector3 prePos1 = new Vector3(prePos.x, 0.0f, prePos.z);
        Instantiate(pointPrefab, prePos1, Quaternion.identity);
        camAudioSource.PlayOneShot(pointSound);
    }

    public void DrawWall(Vector3 _prePos, Vector3 _curPos)
    {
        Vector3 prePos1 = new Vector3(_prePos.x, 0.0f, _prePos.z);
        Vector3 curPos1 = new Vector3(_curPos.x, 0.0f, _curPos.z);

        // Pointを置く
        Instantiate(pointPrefab, curPos1, Quaternion.identity);
        camAudioSource.PlayOneShot(pointSound);

        // 壁の中点を取得
        Vector3 wallVec = curPos1 - prePos1;    // 壁の方向を取得
        float dist = wallVec.magnitude;         // 壁の幅を取得
        Vector3 wallX = new Vector3(dist, 0.0f, 0.0f);
        Vector3 halfWallVec = wallVec * 0.5f;
        Vector3 centerCoord = prePos1 + halfWallVec;
        // 壁を描く
        GameObject wall = PhotonNetwork.Instantiate("Wall", centerCoord, Quaternion.identity);
        wall.transform.rotation = Quaternion.FromToRotation(wallX, wallVec);
        // Debug.Log("Wall rotation: " + wall.transform.rotation);
        // Debug.Log("Wall eulerAngles: " + wall.transform.eulerAngles);
        Vector3 wallScale = wall.transform.localScale;
        wall.transform.localScale = new Vector3(dist, wallScale.y, wallScale.z);
        // prePosを更新
        prePos = _curPos;
    }

    public void PlaceKey()
    {
        // Vector3 keyPos = camTrans.position + dist * camTrans.forward;
        Vector3 keyPos0 = CalcPos();
        Vector3 keyPos = new Vector3(keyPos0.x, 0.0f, keyPos0.z);
        PhotonNetwork.Instantiate("Key", keyPos, Quaternion.identity);
        Debug.Log("Instantiated Key.");     
    }

    public void GameStart()
    {
#if UNITY_EDITOR
        // isStarted = true;
        stopButton.SetActive(true);
        startButton.SetActive(false);
        // 他人のPhoneに子オブジェクトSpeakerをつける
        messegnerCtrl.GameStart();
        // 番人が動き始める
        tagger = PhotonNetwork.Instantiate("Tagger", new Vector3(-0.3f, 0.0f, -2.7f), Quaternion.identity);
        taggerController = tagger.GetComponent<TaggerController>();
        taggerController.StartTagger();
#endif
    }

    public void GameStop()
    {
        // isStarted = false;
        startButton.SetActive(true);
        stopButton.SetActive(false);
        // 番人を止める
        PhotonNetwork.Destroy(tagger);
    }

    public void SaveObj()
    {
        // 鍵の位置を保存
        List<Vector3> keysPos = new List<Vector3>();
        GameObject[] keys = GameObject.FindGameObjectsWithTag("Key");
        if (keys.Length > 0)
        {
            foreach (GameObject key in keys)
            {
                keysPos.Add(key.transform.position);
            }
            foreach (Vector3 keyPos in keysPos)
            {
                Debug.Log("Key Position: " + keyPos);
            }
            Debug.Log("----------");
        }
        // 壁の位置を保存
        List<Vector3> wallsPos = new List<Vector3>();
        GameObject[] walls = GameObject.FindGameObjectsWithTag("Wall");
        if (walls.Length > 0)
        {
            foreach (GameObject wall in walls)
            {
                GameObject startPoint = wall.transform.GetChild(0).gameObject;
                wallsPos.Add(startPoint.transform.position);
                GameObject endPoint = wall.transform.GetChild(1).gameObject;
                wallsPos.Add(endPoint.transform.position);
            }
            foreach (Vector3 wallPos in wallsPos)
            {
                Debug.Log("Wall Position: " + wallPos);
            }
            Debug.Log("----------");
        }
    }
}
