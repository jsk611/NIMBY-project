using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

public class Player : MonoBehaviourPunCallbacks
{
    public float speed;
    public GameObject canvas;
    public Text nickName;
    public Camera maincam;
    [SerializeField] GameObject trash;
    // Start is called before the first frame update
    void Start()
    {
        if(!photonView.IsMine)
        {
            canvas.SetActive(false);
            maincam.enabled = false;
        }
        nickName.text = photonView.Owner.NickName;
    }
    private void Update()
    {
        if(photonView.IsMine)
        {
            if(Input.GetKeyDown(KeyCode.Space))
            {
                photonView.RPC("MakeTrash", RpcTarget.All);
            }
        }
    }
    // Update is called once per frame
    void LateUpdate()
    {
        if(photonView.IsMine)
        {
            float h = Input.GetAxisRaw("Horizontal");
            float v = Input.GetAxisRaw("Vertical");
            transform.Translate(new Vector2(h, v).normalized * speed * Time.deltaTime);

        }
    }

    [PunRPC]
    void MakeTrash()
    {
        Instantiate(trash, transform.position, Quaternion.identity);
    }
}
