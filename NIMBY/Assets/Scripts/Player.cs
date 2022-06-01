using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

public class Player : MonoBehaviourPunCallbacks, IPunObservable
{
    public float speed;
    public GameObject canvas;
    public Text nickName;
    public Camera maincam;
    [SerializeField] GameManager gameManager;
    [SerializeField] GameObject trash;
    [SerializeField] Text zoneT;

    [SerializeField] GameObject reportArea;
    bool inOtherZone;
    public bool isCrimed;

    bool isArrested;
    

    [SerializeField] Text chatInput;
    [SerializeField] Text chatText;
    [SerializeField] GameObject chatBox;
    
    // Start is called before the first frame update
    void Start()
    {
        if(!photonView.IsMine)
        {
            canvas.SetActive(false);
            maincam.enabled = false;
            reportArea.SetActive(false);
        }
        nickName.text = photonView.Owner.NickName;


    }
    private void Update()
    {
        if(gameManager.isStarted)
        {
            if(photonView.IsMine)
            {
                if(Input.GetKeyDown(KeyCode.Space))
                {
                    photonView.RPC("MakeTrash", RpcTarget.All);
                }
            }
        }

        if(photonView.IsMine)
        {
            if (Input.GetKeyDown(KeyCode.Return) && chatInput != null)
            {
                photonView.RPC("Chat", RpcTarget.All);
            }
        }
    }
    // Update is called once per frame
    void LateUpdate()
    {
        if(photonView.IsMine)
        {
            if(!isArrested)
            {
                float h = Input.GetAxisRaw("Horizontal");
                float v = Input.GetAxisRaw("Vertical");
                transform.Translate(new Vector2(h, v).normalized * speed * Time.deltaTime);

            }

        }
    }

    Collider2D trashHit;
    Collider2D[] enemyHit;
    private void FixedUpdate() //신고범위
    {
        if (!gameManager.isStarted)
            return;

        enemyHit = Physics2D.OverlapCircleAll(transform.position, 1.5f, LayerMask.GetMask("Player"));
        trashHit = Physics2D.OverlapCircle(transform.position, 1.5f, LayerMask.GetMask("Trash"));
        Debug.DrawRay(transform.position, Vector2.right * 1.5f, Color.red);

        if (enemyHit != null)
        {
            if (Input.GetKeyDown(KeyCode.Q) && photonView.IsMine)
                photonView.RPC("Report", RpcTarget.All);

        }
        //if(trashHit != null)
        //{
        //    if(Input.GetKeyDown(KeyCode.LeftShift) && photonView.IsMine)
        //    {
        //        photonView.RPC("Clean", RpcTarget.All);
        //    }
        //}
    }
    #region 쓰레기 무단투기
    Coroutine c = null;
    [PunRPC]
    void MakeTrash() //쓰레기 무단투기
    {
        if(inOtherZone && trashHit == null)
        {
            Instantiate(trash, transform.position, Quaternion.identity);
            if (c != null) StopCoroutine(c);
            c = StartCoroutine(Criminal());
        }
    }

    IEnumerator Criminal() //범죄행위를 저질렀을때(무단투기)
    {
        isCrimed = true;
        speed = 4;
        yield return new WaitForSeconds(10f);
        isCrimed = false;
        speed = 5;
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
                iP.Arrested();
        }
    }
    #endregion
    #region 체포당함
    public void Arrested()
    {
        //자신의 존으로 이동
        transform.position = gameManager.myZone.transform.position;
        //10초간 정지 및 실명
        if (photonView.IsMine)
            StartCoroutine(StopAndBlind());
    }

    IEnumerator StopAndBlind() 
    {
        StopCoroutine(Criminal());
        isCrimed = false;
        isArrested = true;
        //정지 및 실명
        yield return new WaitForSeconds(10f);
        //정지, 실명 해제
        isArrested = false;
    }
    #endregion

    #region 청소
    [PunRPC]
    void Clean() //청소
    {
        Destroy(trashHit.gameObject);
    }
    #endregion

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("Zone") && gameManager.isStarted)
        {
            Zone z = collision.GetComponent<Zone>();
            //들어온 구역 안내
            Debug.Log(z.owner.photonView.Owner.NickName + "의 구역");
            zoneT.text = z.owner.photonView.Owner.NickName + "의 구역";
        }
    }
    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("Zone") && gameManager.isStarted)
        {
            Zone z = collision.GetComponent<Zone>();
            //현재 상대방의 구역에 있는지 확인
            if (z.owner.photonView.ViewID != photonView.ViewID)
            {
                inOtherZone = true;
            }
        }
        if(collision.CompareTag("Trash"))
        {
            if (Input.GetKeyDown(KeyCode.LeftShift) && photonView.IsMine)
            {
                photonView.RPC("Clean", RpcTarget.All);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Zone") && gameManager.isStarted)
        {
            Zone z = collision.GetComponent<Zone>();
            //상대방의 구역에서 나왔을때
            if (z.owner.photonView.ViewID != photonView.ViewID)
            {
                inOtherZone = false;
            }
            
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
