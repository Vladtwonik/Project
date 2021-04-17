using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonRef : MonoBehaviour //для индикатора возле кнопок в меню
{
    public GameObject selectIndicator;
    public bool selected;
    
    void Start()
    {
        selectIndicator.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        selectIndicator.SetActive(selected);
    }
}
