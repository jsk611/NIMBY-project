using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;

public class PhotonInit : MonoBehaviourPunCallbacks
{
    public Text log, roomName, nickName;
    [SerializeField] Camera mainCam;
    [SerializeField] GameObject canvas;

    void Start()
    {
        Screen.SetResolution(980, 540, false);
        PhotonNetwork.ConnectUsingSettings();
        PhotonNetwork.JoinLobby();
        DontDestroyOnLoad(gameObject);
    }

    public override void OnConnected()
    {
        log.text = "��! ���� ����!";
    }
    public override void OnJoinedRoom()
    {
        mainCam.gameObject.SetActive(false);
        canvas.SetActive(false);
        PhotonNetwork.Instantiate("Player", new Vector2(0, 0), Quaternion.identity);
    }
    public override void OnJoinedLobby()
    {
        log.text = "��! �κ� ����!";

    }
    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        log.text = message;
    }
    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        log.text = message;
    }
    public void MakeRoom()
    {
        PhotonNetwork.NickName = nickName.text;
        PhotonNetwork.CreateRoom(roomName.text, new RoomOptions { MaxPlayers = 4 }, null);
        
    }
    public void JoinRandomRoom()
    {
        PhotonNetwork.NickName = nickName.text;
        PhotonNetwork.JoinRandomRoom();
    }
    public void SearchRoom()
    {
        PhotonNetwork.NickName = nickName.text;
        PhotonNetwork.JoinRoom(roomName.text);
    }
    void Update()
    {

    }

}
