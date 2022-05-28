using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class GameManager : MonoBehaviourPunCallbacks
{
    int playerCount = 0;
    List<int> players;
    [SerializeField] Zone[] zones;
    // Start is called before the first frame update
    void Awake()
    {
        players = new List<int>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer)
    {
        playerCount++;
        //players.Add(int.Parse(newPlayer.UserId));

        if(playerCount == 2)
        {
            GameSetUp();
        }
    }
    public override void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer)
    {
        playerCount--;
        //players.Remove(int.Parse(otherPlayer.UserId));
    }

    void GameSetUp()
    {
        Player[] p = FindObjectsOfType<Player>();
        for(int i=0; i<2; i++)
        {
            zones[i].ownerId = p[i].photonView.ViewID;
            zones[i].spr.color = Color.red;
        }
    }
}
