using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Trash : MonoBehaviourPunCallbacks
{
    public int size;
    public int trashId;

    public void DestroySelf()
    {
        photonView.RPC("Des", RpcTarget.All);
    }
    [PunRPC]
    private void Des()
    {
        if(photonView.IsMine)
            PhotonNetwork.Destroy(gameObject);
    }
}
