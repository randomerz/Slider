using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShopManager : MonoBehaviour
{
    public GameObject oceanQuestPanel;
    public GameObject buyPanel;
    public GameObject talkPanel;
    public static bool canOpenMenus = true;
    public static bool closeUI;
    public static bool isGamePaused;
    public bool isOceanQuestPanelOpen;
    // Start is called before the first frame update
    //void Start()
    //{

    //}

    // Update is called once per frame
    void Update()
    {
      if (closeUI)
      {
          closeUI = false;
          ResumeGame();
      }
    }

    public void OpenQuestPanel()
    {
          oceanQuestPanel.SetActive(true);
          Time.timeScale = 0;
          isGamePaused = true;
    }

    public void ResumeGame()
    {
        if (isOceanQuestPanelOpen)
        {
            Player.SetCanMove(true);
            Time.timeScale = 1;
            isGamePaused = false;
            oceanQuestPanel.SetActive(false);
        }
    }


    public void OpenBuyPanel()
    {
        buyPanel.SetActive(true);
        Time.timeScale = 0;
        isGamePaused = true;
    }

    public void CloseBuyPanel()
    {
        buyPanel.SetActive(false);
    }

    public void OpenTalkPanel()
    {
        talkPanel.SetActive(true);
    }

    public void CloseTalkPanel()
    {
      talkPanel.SetActive(false);
    }

    //public void CloseQuestPanel() {
      //oceanQuestPanel.SetActive(false);
      //Time.timeScale = 1;
      //isGamePaused = false;
    //}
}
