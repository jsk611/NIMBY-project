using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
public class Trashbin : MonoBehaviourPunCallbacks
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Trash"))
        {
            Debug.Log("������ ó����");
            PhotonNetwork.Destroy(collision.gameObject);
        }
    }
}
