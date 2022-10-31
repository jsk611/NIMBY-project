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
    bool isJoinedRoom;
    void Start()
    {
        Screen.SetResolution(960, 540, false);
        PhotonNetwork.ConnectUsingSettings();
        PhotonNetwork.JoinLobby();
        DontDestroyOnLoad(gameObject);
        DontDestroyOnLoad(mainCam);
        DontDestroyOnLoad(canvas);
        SceneManager.LoadScene("Main");
    }

    public override void OnConnected()
    {
        log.text = "와! 서버 연결!";
    }
    public override void OnJoinedRoom()
    {
        mainCam.gameObject.SetActive(false);
        canvas.SetActive(false);
        PhotonNetwork.Instantiate("Player", new Vector2(0, 0), Quaternion.identity);
        isJoinedRoom = true;
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
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (!isJoinedRoom) Shutdown();
            isJoinedRoom = false;
            PhotonNetwork.LeaveRoom();
            SceneManager.LoadScene("First");
            Destroy(gameObject);
            Destroy(mainCam);
            Destroy(canvas);
        }    
    }

    public void Shutdown()
    {
        Application.Quit();
    }

    public void Restart()
    {
        SceneManager.LoadScene("Main");
        PhotonNetwork.Instantiate("Player", new Vector2(0, 0), Quaternion.identity);
    }

}
