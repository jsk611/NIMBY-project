using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Intro : MonoBehaviour
{
    Text log;
    string[] logs;
    int index;
    [SerializeField] GameObject parent;

    private void Start()
    {
        index = 0;
        log = GetComponent<Text>();
        log.text = "";
        logs = new string[]{ "정부에서 보낸 메세지입니다.", 
            "이 지역에 쓰레기 매립지와 공원 설치를 \n365일 후에 실시할 예정입니다." ,
            "쓰레기 매립지는 이 지역의 구역 중 \n가장 더러운 곳에 설치할 예정이고,\n공원은 가장 깨끗한 곳에 설치할 예정입니다.",
            "구역을 깨끗하게 유지하는 팁을 알려드리겠습니다.",
            "쓰레기에 접근해 Shift 키를 누르면\n쓰레기를 청소할 수 있습니다.",
            "만약 다른 구역 주민들이 쓰레기를 무단투기한다면\nQ를 눌러 신고하십시오.",
            "그럼 300일 후에 봅시다."};

        StartCoroutine(LogAnimation());
    }

    IEnumerator LogAnimation()
    {
        foreach(var str in logs)
        {
            log.text = "";
            foreach(var chr in str)
            {
                log.text += chr;
                yield return new WaitForSeconds(0.1f);
            }
            yield return new WaitForSeconds(2f);
        }
        yield return new WaitForSeconds(1f);
        parent.SetActive(false);
        
    }
}
