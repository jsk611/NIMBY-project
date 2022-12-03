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
        logs = new string[]{ "���ο��� ���� �޼����Դϴ�.", 
            "�� ������ ������ �Ÿ����� ���� ��ġ�� \n365�� �Ŀ� �ǽ��� �����Դϴ�." ,
            "������ �Ÿ����� �� ������ ���� �� \n���� ������ ���� ��ġ�� �����̰�,\n������ ���� ������ ���� ��ġ�� �����Դϴ�.",
            "������ �����ϰ� �����ϴ� ���� �˷��帮�ڽ��ϴ�.",
            "�����⿡ ������ Shift Ű�� ������\n�����⸦ û���� �� �ֽ��ϴ�.",
            "���� �ٸ� ���� �ֹε��� �����⸦ ���������Ѵٸ�\nQ�� ���� �Ű��Ͻʽÿ�.",
            "�׷� 300�� �Ŀ� ���ô�."};

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
