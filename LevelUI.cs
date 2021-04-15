using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class LevelUI : MonoBehaviour
{
     public Text AnnouncerTextLine1;
     public Text AnnouncerTextLine2;
     public Text LevelTimer;

     public Slider[] healthSliders;

     public GameObject[] winIndicatorGrids;
     public GameObject winIndicator;

     public LevelUI instance;
    public LevelUI GetInstance()
    {
        return instance;
    }

    void Awake()
    {
        instance = this;
    }

    public void AddWinIndicator(int player)
    {
        GameObject go = Instantiate(winIndicator, transform.position, Quaternion.identity) as GameObject;
        go.transform.SetParent(winIndicatorGrids[player].transform);
    }
}
