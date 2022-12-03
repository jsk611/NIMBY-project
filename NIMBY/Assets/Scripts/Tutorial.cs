using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tutorial : MonoBehaviour
{
    [SerializeField] GameObject[] tutorials;
    [SerializeField] GameObject tutorialUI;
    int index = 0;
    bool isTutorial;
    private void Update()
    {

        if (Input.anyKeyDown && isTutorial)
        {
            tutorials[index++].SetActive(false);
            if (index >= tutorials.Length)
            {
                tutorialUI.SetActive(false);
                isTutorial = false;
                index = 0;
            }
            else
                tutorials[index].SetActive(true);
        }
    }

    public void TutorialOn()
    {
        tutorialUI.SetActive(true);
        isTutorial = true;
        index = 0;
    }
}
