using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Trash : MonoBehaviour
{
    public int size;
    public int trashId;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Trashbin"))
        {
            
        }
    }

    [PunRPC]
    void DestroySelf()
    {
        PhotonNetwork.Destroy(gameObject);
        Debug.Log("쓰레기 처리됨");
    }
}
