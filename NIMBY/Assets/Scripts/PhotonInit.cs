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
    

    void Start()
    {
        Screen.SetResolution(980, 540, false);
        PhotonNetwork.ConnectUsingSettings();
        PhotonNetwork.JoinLobby();
        DontDestroyOnLoad(gameObject);
    }

    public override void OnConnected()
    {
        log.text = "와! 서버 연결!";
    }
    public override void OnJoinedRoom()
    {
        PhotonNetwork.Instantiate("Player", new Vector2(0, 0), Quaternion.identity);

    }
    public override void OnJoinedLobby()
    {
        log.text = "와! 로비 연결!";

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
        SceneManager.LoadScene("Main");
    }
    public void JoinRandomRoom()
    {
        PhotonNetwork.NickName = nickName.text;
        PhotonNetwork.JoinRandomRoom();
    }
    public void SearchRoom()
    {
        SceneManager.LoadScene("Main");
        PhotonNetwork.NickName = nickName.text;
        PhotonNetwork.JoinRoom(roomName.text);
    }
    void Update()
    {

    }

}
