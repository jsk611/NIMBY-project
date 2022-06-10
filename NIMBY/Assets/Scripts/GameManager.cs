using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;

public class GameManager : MonoBehaviourPunCallbacks
{
    [SerializeField] Zone[] zones = new Zone[4];
    Zone[] activatedZones;
    public bool isStarted;
    public Zone myZone;
    Zone[] ranking;
    [SerializeField] Text[] rankTexts;
    [SerializeField] Text winnerText;
    [SerializeField] GameObject[] trashes;
    GameObject startTrashes;
    float timer;
    [SerializeField] Text timerText;
    [SerializeField] GameObject exitRoomInfo;
    [SerializeField] GameObject intro;

    AudioSource audioSource;
    [SerializeField] AudioClip[] BGMs;
    // Start is called before the first frame update
    void Awake()
    {
    }
    private void Start()
    {
        if(photonView.IsMine)
        {
            audioSource = GetComponent<AudioSource>();
            startTrashes = GameObject.Find("StartTrashes");
            startTrashes.SetActive(false);
            audioSource.clip = BGMs[0];
            audioSource.Play();
        }
        zones = GameObject.Find("Areas").transform.GetComponentsInChildren<Zone>();
        timer = 300f;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.G) && !isStarted)
            photonView.RPC("GameStart", RpcTarget.All);

        if(isStarted)
        {
            if (timer <= 0)
                GameEnd();
            else
                timer -= Time.deltaTime;
            timerText.text = "D - " + ((int)timer).ToString();

            CheckRanking();
            for(int i=0; i<4; i++)
            {
                if(i> ranking.Length-1)
                {
                    rankTexts[i].text = (i+1).ToString() + " : -";
                    continue;
                }
                rankTexts[i].text = (i+1).ToString() + " : " + ranking[i].owner.photonView.Owner.NickName + " : " + ranking[i].Check();
            }

            exitRoomInfo.SetActive(false);

            audioSource.pitch = timer > 200f ? 1 : 1 + (200 - timer) / 200;
        }
        else
        {
            exitRoomInfo.SetActive(true);
        }
    }


    [PunRPC]
    void GameStart()
    {
        PhotonNetwork.CurrentRoom.IsOpen = false;
        Player[] p = FindObjectsOfType<Player>();
        activatedZones = new Zone[p.Length];
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
            switch(i)
            {
                case 0:p[i].GetComponent<SpriteRenderer>().color = new Color(1, 0.34f, 0.34f);break;
                case 1:p[i].GetComponent<SpriteRenderer>().color = new Color(1, 0.77f, 0.34f);break;
                case 2:p[i].GetComponent<SpriteRenderer>().color = new Color(0.12f, 0.78f, 0f);break;
                case 3:p[i].GetComponent<SpriteRenderer>().color = new Color(0.19f, 0.57f, 1f);break;
            }

            zones[i].owner = p[i];
            zones[i].spr.color = p[i].GetComponent<SpriteRenderer>().color;

            if(p[i].photonView.IsMine)
            {
                myZone = zones[i];
                p[i].transform.position = zones[i].transform.position;
            }
            activatedZones[i] = zones[i];
        }
        isStarted = true;
        Debug.Log("game start");
        if(photonView.IsMine)
        {
            startTrashes.SetActive(true);
            intro.SetActive(true);
            audioSource.Stop();
            audioSource.clip = BGMs[1];
            audioSource.Play();
        }
    }

    void CheckRanking()
    {
        ranking = activatedZones;

        for (int i = 0; i < activatedZones.Length; i++)
        {
            for (int j = 0; j < activatedZones.Length - i - 1; j++)
            {
                if (ranking[j].Check() > ranking[j + 1].Check())
                {
                    var tmp = ranking[j + 1];
                    ranking[j + 1] = ranking[j];
                    ranking[j] = tmp;
                }
            }
        }
    }
    void GameEnd()
    {
        isStarted = false;
        winnerText.gameObject.SetActive(true);
        winnerText.text = ranking[0].owner.photonView.Owner.NickName + " ½Â¸®!!";

        Vector2 loserPos = ranking[ranking.Length - 1].transform.position;
        StartCoroutine(Punishment(loserPos));
        audioSource.clip = BGMs[2];
        audioSource.pitch = 1;
        audioSource.Play();
    }
    IEnumerator Punishment(Vector2 loserPos)
    {
        for(int i=0; i<100; i++)
        {
            Vector2 randPos = new Vector2(loserPos.x + Random.Range(-7f, 7f), loserPos.y + Random.Range(-4f, 4f));
            int randTrash = Random.Range(0, 5);
            Instantiate(trashes[randTrash], randPos, Quaternion.identity);
            yield return new WaitForSeconds(0.2f);
        }
    }

    
}
