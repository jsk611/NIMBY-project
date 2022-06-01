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
    private void FixedUpdate() //�Ű����
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
    #region ������ ��������
    Coroutine c = null;
    [PunRPC]
    void MakeTrash() //������ ��������
    {
        if(inOtherZone && trashHit == null)
        {
            Instantiate(trash, transform.position, Quaternion.identity);
            if (c != null) StopCoroutine(c);
            c = StartCoroutine(Criminal());
        }
    }

    IEnumerator Criminal() //���������� ����������(��������)
    {
        isCrimed = true;
        speed = 4;
        yield return new WaitForSeconds(10f);
        isCrimed = false;
        speed = 5;
    }
    #endregion
    #region �Ű�
    [PunRPC]
    void Report() //�Ű�
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
    #region ü������
    public void Arrested()
    {
        //�ڽ��� ������ �̵�
        transform.position = gameManager.myZone.transform.position;
        //10�ʰ� ���� �� �Ǹ�
        if (photonView.IsMine)
            StartCoroutine(StopAndBlind());
    }

    IEnumerator StopAndBlind() 
    {
        StopCoroutine(Criminal());
        isCrimed = false;
        isArrested = true;
        //���� �� �Ǹ�
        yield return new WaitForSeconds(10f);
        //����, �Ǹ� ����
        isArrested = false;
    }
    #endregion

    #region û��
    [PunRPC]
    void Clean() //û��
    {
        Destroy(trashHit.gameObject);
    }
    #endregion

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("Zone") && gameManager.isStarted)
        {
            Zone z = collision.GetComponent<Zone>();
            //���� ���� �ȳ�
            Debug.Log(z.owner.photonView.Owner.NickName + "�� ����");
            zoneT.text = z.owner.photonView.Owner.NickName + "�� ����";
        }
    }
    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("Zone") && gameManager.isStarted)
        {
            Zone z = collision.GetComponent<Zone>();
            //���� ������ ������ �ִ��� Ȯ��
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
            //������ �������� ��������
            if (z.owner.photonView.ViewID != photonView.ViewID)
            {
                inOtherZone = false;
            }
            
        }
    }

    #region ������ �ǻ������ ���� ä��
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
