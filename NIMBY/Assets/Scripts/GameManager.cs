using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class GameManager : MonoBehaviourPunCallbacks
{
    [SerializeField] Zone[] zones = new Zone[4];
    public bool isStarted;
    // Start is called before the first frame update
    void Awake()
    {

    }
    private void Start()
    {
        zones = GameObject.Find("Areas").transform.GetComponentsInChildren<Zone>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.G) && !isStarted)
            photonView.RPC("GameStart", RpcTarget.All);
    }


    [PunRPC]
    void GameStart()
    {
        Player[] p = FindObjectsOfType<Player>();
        for(int i=0; i<p.Length; i++)
        {
            for(int j=0; j<p.Length - i-1; j++)
            {
                if(p[j].photonView.ViewID > p[j+1].photonView.ViewID)
                {
                    var tmp = p[j + 1];
                    p[j + 1] = p[j];
                    p[j] = tmp;
                }
            }
        }

        for(int i=0; i<PhotonNetwork.PlayerList.Length; i++)
        {
            zones[i].owner = p[i];
            zones[i].spr.color = p[i].GetComponent<SpriteRenderer>().color;

            if(p[i].photonView.IsMine)
                p[i].transform.position = zones[i].transform.position;
            
        }
        isStarted = true;
        Debug.Log("game start");


    }


}
