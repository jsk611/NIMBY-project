using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

public class Player : MonoBehaviourPunCallbacks, IPunObservable
{
    float speed;
    float Speed
    {
        get { return speed; }
        set { speed = value * (100 + speedUpPct) / 100; }
    }

    public GameObject canvas;
    public Text nickName;
    public Camera maincam;
    [SerializeField] GameManager gameManager;
    [SerializeField] GameObject[] trashes;
    [SerializeField] Text zoneT;

    float gaugeNum;
    float maxGauge;
    [SerializeField] Image gauge;
    int[] trashCount = new int[5];
    public int[] TrashCount { get { return trashCount; } }
    int trashSummonSpeed = 0;
    public int TrashSummonSpeed { get { return trashSummonSpeed; } }
    [SerializeField] Text[] countText;

    int i = 0;
    int n;
    int[] cleaningCode;
    bool isCleaning;
    [SerializeField] GameObject codeBox;
    [SerializeField] Text codeText;
    [SerializeField] Text count;

    [SerializeField] GameObject reportArea;
    bool inOtherZone;
    public bool isCrimed;
    [SerializeField] GameObject arrestedUI;
    [SerializeField] Text arrestTimer;

    bool isArrested;

    [SerializeField] InputField inputField;
    [SerializeField] Text chatInput;
    [SerializeField] Text chatText;
    [SerializeField] GameObject chatBox;

    [SerializeField] AudioClip cleaningAudio;
    [SerializeField] AudioClip ReportingAudio;
    [SerializeField] AudioClip ArrestedAudio;
    AudioSource audioSource;

    int money;
    [SerializeField] Text moneyText;

    [SerializeField] GameObject ShopUI;
    bool isWorking;
    float speedUpPct;
    [SerializeField] Button DefenceUpBtn;
    [SerializeField] Button EcoUpBtn;


    // Start is called before the first frame update
    void Start()
    {
        if(!photonView.IsMine)
        {
            canvas.SetActive(false);
            maincam.enabled = false;
            reportArea.SetActive(false);
        }
        else
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.pitch = 2;
            audioSource.clip = cleaningAudio;
        }

        nickName.text = photonView.Owner.NickName;
        Speed = 4;
        gaugeNum = 0;
        UpdateTrash();
        StartCoroutine(Earning());
    }
    private void Update()
    {
        if(photonView.IsMine)
        {

            if(gameManager.isStarted)
            {
                gaugeNum += 5f * Time.deltaTime;
                if(gaugeNum >= 100f)
                {
                    int randNum = UnityEngine.Random.Range(1, 16);
                    if (randNum <= 5)
                        trashCount[0]++;
                    else if (randNum <= 9)
                        trashCount[1]++;
                    else if (randNum <= 12)
                        trashCount[2]++;
                    else if (randNum <= 14)
                        trashCount[3]++;
                    else
                        trashCount[4]++;

                    gaugeNum -= 100f;
                    UpdateTrash();
                }
                gauge.fillAmount = gaugeNum/100f;
            }

            if(Input.GetKeyDown(KeyCode.C))
            {
                inputField.Select();
            }
            if (Input.GetKeyDown(KeyCode.Return) && chatInput.text != "")
            {
                photonView.RPC("Chat", RpcTarget.All);
                inputField.text = "";
            }
        }

        #region 청소 코드 입력
        if (cleanHit != null)
        {
            if (Input.GetKeyDown(KeyCode.LeftShift) && photonView.IsMine && gameManager.isStarted)
            {
                int id = cleanHit.GetComponent<Trash>().trashId;
                Debug.Log(id.ToString());
                CleanEvent(id);
            }
        }

        if (isCleaning)
        {
            codeText.text = cleaningCode[i].ToString();
            switch (cleaningCode[i])
            {
                case 1:
                    codeText.text = "↑";
                    if (Input.GetKeyDown(KeyCode.UpArrow))
                        i++;
                    else if (Input.anyKeyDown)
                        i = i == 0 ? 0 : i -1;
                    break;
                case 2:
                    codeText.text = "↓";
                    if (Input.GetKeyDown(KeyCode.DownArrow))
                        i++;
                    else if (Input.anyKeyDown)
                        i = i == 0 ? 0 : i - 1;
                    break;
                case 3:
                    codeText.text = "→";
                    if (Input.GetKeyDown(KeyCode.RightArrow))
                        i++;
                    else if (Input.anyKeyDown)
                        i = i == 0 ? 0 : i - 1;
                    break;
                case 4:
                    codeText.text = "←";
                    if (Input.GetKeyDown(KeyCode.LeftArrow))
                        i++;
                    else if (Input.anyKeyDown)
                        i = i == 0 ? 0 : i - 1;
                    break;
            }
            count.text = "남은 횟수 : " + (cleaningCode.Length - i).ToString();
            if(i>=n || !gameManager.isStarted)
            {
                isCleaning = false;
                photonView.RPC("Clean", RpcTarget.All);
                gaugeNum += 30;
                codeBox.SetActive(false);
                if (photonView.IsMine) audioSource.Stop();
            }
        }
        #endregion


        moneyText.text = money.ToString();

    }
    // Update is called once per frame
    void LateUpdate()
    {
        if(photonView.IsMine)
        {
            if(!(isArrested || isCleaning))
            {
                float h = Input.GetAxisRaw("Horizontal");
                float v = Input.GetAxisRaw("Vertical");
                transform.Translate(new Vector2(h, v).normalized * Speed * Time.deltaTime);

            }

        }
    }

    Collider2D trashHit;
    Collider2D cleanHit;
    Collider2D[] enemyHit;
    private void FixedUpdate() //신고범위
    {
        if (!gameManager.isStarted)
            return;

        enemyHit = Physics2D.OverlapCircleAll(transform.position, 1.5f, LayerMask.GetMask("Player"));
        trashHit = Physics2D.OverlapCircle(transform.position, 0.5f, LayerMask.GetMask("Trash"));
        cleanHit = Physics2D.OverlapCircle(transform.position, 0.5f, LayerMask.GetMask("Trash"));
        Debug.DrawRay(transform.position, Vector2.right * 1.5f, Color.red);

        if (enemyHit != null)
        {
            if (Input.GetKeyDown(KeyCode.Q) && photonView.IsMine)
                photonView.RPC("Report", RpcTarget.All);

        }

        

    }
    
    void UpdateTrash()
    {
        for (int i = 0; i < 5; i++)
            countText[i].text = trashCount[i].ToString();
    }
    #region 쓰레기 무단투기
    Coroutine c = null;
    [PunRPC]
    void MakeTrash(int type) //쓰레기 무단투기
    {
        if(photonView.IsMine && trashHit == null)
        {
            switch (type)
            {
                case 1: PhotonNetwork.Instantiate("RecycleTrash", transform.position, Quaternion.identity); break;
                case 2: PhotonNetwork.Instantiate("NormalTrash", transform.position, Quaternion.identity); break;
                case 3: PhotonNetwork.Instantiate("FoodWaste", transform.position, Quaternion.identity); break;
                case 4: PhotonNetwork.Instantiate("Poop", transform.position, Quaternion.identity); break;
                case 5: PhotonNetwork.Instantiate("FurnitureWaste", transform.position, Quaternion.identity); break;
            }
            trashCount[type - 1]--;
            UpdateTrash();


            if (c != null) StopCoroutine(c);
            c = StartCoroutine(Criminal());
            Debug.Log("쓰레기 버림");
        }
    }
    #region 쓰레기 생성버튼 함수
    public void RecycleTrash()
    {
        if (trashCount[0] <= 0)
            return;

        if(photonView.IsMine && gameManager.isStarted)
            photonView.RPC("MakeTrash", RpcTarget.All, 1);
    }
    public void NormalTrash()
    {
        if (trashCount[1] <= 0)
            return;

        if (photonView.IsMine && gameManager.isStarted)
            photonView.RPC("MakeTrash", RpcTarget.All, 2);
    }
    public void FoodWaste()
    {
        if (trashCount[2] <= 0)
            return;

        if (photonView.IsMine && gameManager.isStarted)
            photonView.RPC("MakeTrash", RpcTarget.All, 3);
    }
    public void Poop()
    {
        if (trashCount[3] <= 0)
            return;

        if (photonView.IsMine && gameManager.isStarted)
            photonView.RPC("MakeTrash", RpcTarget.All, 4);
    }
    public void FurnitureWaste()
    {
        if (trashCount[4] <= 0)
            return;

        if (photonView.IsMine && gameManager.isStarted)
            photonView.RPC("MakeTrash", RpcTarget.All, 5);
    }
    #endregion
    IEnumerator Criminal() //범죄행위를 저질렀을때(무단투기)
    {
        isCrimed = true;
        Speed = 3;
        yield return new WaitForSeconds(10f);
        isCrimed = false;
        Speed = 4;
    }
    #endregion
    #region 신고
    [PunRPC]
    void Report() //신고
    {
        foreach (var i in enemyHit)
        {
            if (i.gameObject == gameObject)
                continue;
            Player iP = i.GetComponent<Player>();

            if (iP.isCrimed)
            {
                iP.Arrested();
                audioSource.clip = ReportingAudio;
                audioSource.Play();
            }
        }
    }
    #endregion
    #region 체포당함
    public void Arrested()
    {
        //감옥(중앙)으로 이동
        transform.position = new Vector2(0, 0);
        
        //10초간 정지 및 실명
        if (photonView.IsMine)
        {
            audioSource.clip = ArrestedAudio;
            audioSource.Play();
            StartCoroutine(StopAndBlind());
        }
    }

    IEnumerator StopAndBlind() 
    {
        StopCoroutine(c);
        isCrimed = false;
        isArrested = true;
        //정지 및 실명
        arrestedUI.SetActive(true);
        for(int i=10; i>0; i--)
        {
            arrestTimer.text ="풀리기까지 "+ i.ToString()+"초";
            yield return new WaitForSeconds(1f);
        }
        //정지, 실명 해제
        isArrested = false;
        arrestedUI.SetActive(false);
        Speed = 4f;
    }
    #endregion

    #region 청소
    void CleanEvent(int type)
    {
        switch(type)
        {
            case 1:n = 5;break;
            case 2:n = 7;break;
            case 3:n = 7;break;
            case 4:n = 10;break;
            case 5:n = 14;break;

        }
        cleaningCode = new int[n];
        for (int i = 0; i < n; i++)
            cleaningCode[i] = UnityEngine.Random.Range(1, 5);

        audioSource.clip = cleaningAudio;
        audioSource.Play();
        codeBox.SetActive(true);
        isCleaning = true;
        i = 0;
    }
    

    Coroutine slowC = null;
    [PunRPC]
    void Clean() //청소
    {
        if(cleanHit.GetComponent<Trash>().trashId == 3)
        {
            if(slowC != null)
            {
                StopCoroutine(slowC);
            }
            slowC = StartCoroutine(SlowA());
        }
        else if (cleanHit.GetComponent<Trash>().trashId == 4)
        {
            if (slowC != null)
            {
                StopCoroutine(slowC);
            }
            slowC = StartCoroutine(SlowB());
        }
        Destroy(cleanHit.gameObject);
    }

    IEnumerator SlowA()
    {
        Speed = 1.5f;
        yield return new WaitForSeconds(3f);
        Speed = 4f;
    }
    IEnumerator SlowB()
    {
        Speed = 1.5f;
        yield return new WaitForSeconds(5f);
        Speed = 4f;
    }
    #endregion

    #region 돈벌기
    IEnumerator Earning()
    {
        while(photonView.IsMine)
        {
            if(isWorking)
            {
                money++;
                yield return new WaitForSeconds(0.5f);
            }
            else
            {
                yield return new WaitForEndOfFrame();
                Debug.Log("직장밖");
            }
        }
    }
    #endregion

    #region 업그레이드
    #region 스피드증가
    public void SpeedUp()
    {
        if(money >= 50 && photonView.IsMine)
        {
            money -= 50;
            speedUpPct += 10;
            Speed = 4;
        }
    }
    #endregion
    #region 방어 정책

    #endregion
    #region 환경 정책
    public void Upcycling()
    {
        if(money >= 50 && photonView.IsMine)
        {
            money -= 50;
            trashSummonSpeed += 5;
            Text t = EcoUpBtn.GetComponentInChildren<Text>();
            t.text = "나무 심기(75원)";
            EcoUpBtn.onClick.RemoveAllListeners();
            EcoUpBtn.onClick.AddListener(PlantingTree);
        }
    }
    void PlantingTree()
    {
        if(money >= 75 && photonView.IsMine)
        {
            money -= 75;
            //쓰레기생성범위좁히기

            Text t = EcoUpBtn.GetComponentInChildren<Text>();
            t.text = "재활용 캠페인(100원)";
            EcoUpBtn.onClick.RemoveAllListeners();
            EcoUpBtn.onClick.AddListener(Recycling);
        }
    }
    void Recycling()
    {
        if (money >= 100 && photonView.IsMine)
        {
            money -= 100;
            //쓰레기 자연생성 안되게하기 && 진엔딩조건

            Text t = EcoUpBtn.GetComponentInChildren<Text>();
            t.text = "최대 레벨 도달";
            EcoUpBtn.onClick.RemoveAllListeners();
        }
    }
    #endregion
    #endregion
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("Zone") && gameManager.isStarted)
        {
            Zone z = collision.GetComponent<Zone>();
            //들어온 구역 안내
            try
            {
                Debug.Log(z.owner.photonView.Owner.NickName + "의 구역");
                zoneT.text = z.owner.photonView.Owner.NickName + "의 구역";
                if (z.owner.photonView.ViewID != photonView.ViewID)
                {
                    inOtherZone = true;
                }
                if(photonView.IsMine)
                    zoneT.gameObject.SetActive(true);
            }
            catch (NullReferenceException ie)
            {
                Debug.LogWarning(ie);
            }
        }
        if(collision.CompareTag("WorkArea") && gameManager.isStarted)
        {
            isWorking = true;
        }
        if(collision.CompareTag("Shop") && gameManager.isStarted)
        {
            if (photonView.IsMine)
                ShopUI.SetActive(true);
        }
    }


    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Zone") && gameManager.isStarted)
        {
            Zone z = collision.GetComponent<Zone>();
            //상대방의 구역에서 나왔을때
            try
            {
                if (z.owner.photonView.ViewID != photonView.ViewID)
                {
                    inOtherZone = false;
                }

            }
            catch (NullReferenceException ie)
            {
                Debug.LogWarning(ie);
            }
            if (photonView.IsMine)
                zoneT.gameObject.SetActive(false);
        }
        if (collision.CompareTag("WorkArea") && gameManager.isStarted)
        {
            isWorking = false;
        }
        if (collision.CompareTag("Shop"))
        {
            if (photonView.IsMine)
                ShopUI.SetActive(false);
        }
    }

    #region 간단한 의사소통을 위한 채팅
    Coroutine chatC = null;
    [PunRPC] 
    void Chat()
    {
        if(chatC != null)
        {
            StopCoroutine(chatC);
        }
        chatC = StartCoroutine(ChatCoroutine());
        chatBox.SetActive(true);
        chatText.text = chatInput.text;
        //chatInput = null;
    }
    IEnumerator ChatCoroutine()
    {
        yield return new WaitForSeconds(5f);
        chatBox.SetActive(false);
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(chatText.text);
        }

        else
        {
            chatText.text = (string)stream.ReceiveNext();   

        }
    }
    #endregion
}
