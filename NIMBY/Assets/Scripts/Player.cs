using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

public class Player : MonoBehaviourPunCallbacks, IPunObservable
{
    float speed;
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
        speed = 4;
        gaugeNum = 0;
        UpdateTrash();
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
                    int randNum = Random.Range(1, 16);
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

        #region ???? ???? ????
        if (trashHit2 != null)
        {
            if (Input.GetKeyDown(KeyCode.LeftShift) && photonView.IsMine && gameManager.isStarted)
            {
                int id = trashHit2.GetComponent<Trash>().trashId;
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
                    codeText.text = "??";
                    if (Input.GetKeyDown(KeyCode.UpArrow))
                        i++;
                    else if (Input.anyKeyDown)
                        i = i == 0 ? 0 : i -1;
                    break;
                case 2:
                    codeText.text = "??";
                    if (Input.GetKeyDown(KeyCode.DownArrow))
                        i++;
                    else if (Input.anyKeyDown)
                        i = i == 0 ? 0 : i - 1;
                    break;
                case 3:
                    codeText.text = "??";
                    if (Input.GetKeyDown(KeyCode.RightArrow))
                        i++;
                    else if (Input.anyKeyDown)
                        i = i == 0 ? 0 : i - 1;
                    break;
                case 4:
                    codeText.text = "??";
                    if (Input.GetKeyDown(KeyCode.LeftArrow))
                        i++;
                    else if (Input.anyKeyDown)
                        i = i == 0 ? 0 : i - 1;
                    break;
            }
            count.text = "???? ???? : " + (cleaningCode.Length - i).ToString();
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
                transform.Translate(new Vector2(h, v).normalized * speed * Time.deltaTime);

            }

        }
    }

    Collider2D trashHit;
    Collider2D trashHit2;
    Collider2D[] enemyHit;
    private void FixedUpdate() //????????
    {
        if (!gameManager.isStarted)
            return;

        enemyHit = Physics2D.OverlapCircleAll(transform.position, 1.5f, LayerMask.GetMask("Player"));
        trashHit = Physics2D.OverlapCircle(transform.position, 1.5f, LayerMask.GetMask("Trash"));
        trashHit2 = Physics2D.OverlapCircle(transform.position, 0.5f, LayerMask.GetMask("Trash"));
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
    #region ?????? ????????
    Coroutine c = null;
    [PunRPC]
    void MakeTrash(int type) //?????? ????????
    {
        if(inOtherZone && trashHit == null)
        {
            switch(type)
            {
                case 1: Instantiate(trashes[0], transform.position, Quaternion.identity);break;
                case 2: Instantiate(trashes[1], transform.position, Quaternion.identity);break;
                case 3: Instantiate(trashes[2], transform.position, Quaternion.identity);break;
                case 4: Instantiate(trashes[3], transform.position, Quaternion.identity);break;
                case 5: Instantiate(trashes[4], transform.position, Quaternion.identity);break;
            }
            trashCount[type - 1]--;
            UpdateTrash();


            if (c != null) StopCoroutine(c);
            c = StartCoroutine(Criminal());
            Debug.Log("?????? ????");
        }
    }
    #region ?????? ???????? ????
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
    IEnumerator Criminal() //?????????? ??????????(????????)
    {
        isCrimed = true;
        speed = 3;
        yield return new WaitForSeconds(10f);
        isCrimed = false;
        speed = 4;
    }
    #endregion
    #region ????
    [PunRPC]
    void Report() //????
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
    #region ????????
    public void Arrested()
    {
        //????(????)???? ????
        transform.position = new Vector2(0, 0);
        
        //10???? ???? ?? ????
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
        //???? ?? ????
        arrestedUI.SetActive(true);
        for(int i=10; i>0; i--)
        {
            arrestTimer.text ="?????????? "+ i.ToString()+"??";
            yield return new WaitForSeconds(1f);
        }
        //????, ???? ????
        isArrested = false;
        arrestedUI.SetActive(false);
        speed = 4f;
    }
    #endregion

    #region ????
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
            cleaningCode[i] = Random.Range(1, 5);

        audioSource.clip = cleaningAudio;
        audioSource.Play();
        codeBox.SetActive(true);
        isCleaning = true;
        i = 0;
    }
    

    Coroutine slowC = null;
    [PunRPC]
    void Clean() //????
    {
        if(trashHit2.GetComponent<Trash>().trashId == 3)
        {
            if(slowC != null)
            {
                StopCoroutine(slowC);
            }
            slowC = StartCoroutine(SlowA());
        }
        else if (trashHit2.GetComponent<Trash>().trashId == 4)
        {
            if (slowC != null)
            {
                StopCoroutine(slowC);
            }
            slowC = StartCoroutine(SlowB());
        }
        Destroy(trashHit2.gameObject);
    }

    IEnumerator SlowA()
    {
        speed = 1.5f;
        yield return new WaitForSeconds(3f);
        speed = 4f;
    }
    IEnumerator SlowB()
    {
        speed = 1.5f;
        yield return new WaitForSeconds(5f);
        speed = 4f;
    }
    #endregion

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("Zone") && gameManager.isStarted)
        {
            Zone z = collision.GetComponent<Zone>();
            //?????? ???? ????
            Debug.Log(z.owner.photonView.Owner.NickName + "?? ????");
            if(photonView.IsMine)
                zoneT.gameObject.SetActive(true);
            zoneT.text = z.owner.photonView.Owner.NickName + "?? ????";
            if (z.owner.photonView.ViewID != photonView.ViewID)
            {
                inOtherZone = true;
            }
        }
    }
    //private void OnTriggerStay2D(Collider2D collision)
    //{
    //    if(collision.CompareTag("Trash"))
    //    {
    //        if (Input.GetKeyDown(KeyCode.LeftShift) && photonView.IsMine && gameManager.isStarted)
    //        {
    //            int id = collision.GetComponent<Trash>().trashId;
    //            Debug.Log(id.ToString());
    //            CleanEvent(id);
    //        }
    //    }
    //}

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Zone") && gameManager.isStarted)
        {
            Zone z = collision.GetComponent<Zone>();
            //???????? ???????? ????????
            if (z.owner.photonView.ViewID != photonView.ViewID)
            {
                inOtherZone = false;
            }
            if (photonView.IsMine)
                zoneT.gameObject.SetActive(false);
        }
    }

    #region ?????? ?????????? ???? ????
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
