using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class IntroSceneManager : MonoBehaviour
{
    public GameObject startText;
    float timer;
    bool loadingLevel;
    bool init;

    public int activeElement;
    public GameObject menuObj;
    public ButtonRef[] menuOptions;

    void Start()
    {
        menuObj.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if(!init)
        {
            //Начальная надпись "Нажмите "пробел", чтобы начать" будет появляться и исчезать через определенный промежуток времени
            timer += Time.deltaTime;
            if (timer > 0.6f)
            {
                timer = 0;
                startText.SetActive(startText.activeInHierarchy);
            }
            
            //действия после нажатия клавиши "пробел"
            if (Input.GetKeyUp(KeyCode.Space) || Input.GetButtonUp("Jump"))
            {
                init = true;
                startText.SetActive(false); //закрываем стартовый текст
                menuObj.SetActive(true); //открываем выбор режима игры 
            }
        }
        else 
        {
            if(!loadingLevel) //если мы еще не выбрали режим
            {
                //с помощью рамки/указателя, определяем, какой режим хотим выбрать (индикатор)
                menuOptions[activeElement].selected = true;

                //если игрок только определяется и пытается выбрать режим, нажимая стрелочки
                if(Input.GetKeyUp(KeyCode.UpArrow))
                {
                    menuOptions[activeElement].selected = false;

                    if (activeElement > 0)
                    {
                        activeElement--;
                    }
                    else 
                    {
                        activeElement = menuOptions.Length - 1;
                    }
                }

                if(Input.GetKeyUp(KeyCode.DownArrow))
                {
                    menuOptions[activeElement].selected = false;

                    if (activeElement > menuOptions.Length - 1)
                    {
                        activeElement++;
                    }
                    else 
                    {
                        activeElement = 0;
                    }
                }
           //повторное нажатие клавиши "пробел" - выбор режима

                if (Input.GetKeyUp(KeyCode.Space) || Input.GetButtonUp("Jump"))
                {
                    //загружаем уровень с учетом выбранного режима
                    Debug.Log("load");
                    loadingLevel = true;
                    StartCoroutine("LoadLevel"); // обращаемся к выполнению корутины
                    menuOptions[activeElement].transform.localScale *= 1.2f;
                }
            }
        }
    }
    void HandleSelectedOption()
    {
        switch(activeElement)
        {
            case 0:
                CharacterManager.GetInstance().numberOfUsers = 1; //Get instance дословно переводится как "Получить экземпляр"
            break;
            
            case 1:
                CharacterManager.GetInstance().numberOfUsers = 2;
                CharacterManager.GetInstance().players[1].playerType = PlayerBase.PlayerType.user;
            break;
        }
    }

    IEnumerator LoadLevel()
    {
        HandleSelectedOption();
        yield return new WaitForSeconds(0.6f);
        startText.SetActive(false);
        yield return new WaitForSeconds(1.5f);
        SceneManager.LoadSceneAsync("select", LoadSceneMode.Single);
    }
}