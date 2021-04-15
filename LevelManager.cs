using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    WaitForSeconds oneSec; //будем много использовать
    public Transform[] spawnPositions;
    CharacterManager charM;
    LevelUI levelUI;//будем хранить все UI элементы здесь для удобного использования
    int currentTurn = 1;//определенный ход, который мы совершаем в игре, начиная с первого
    public int maxTurns = 2;
    //переменные для обратного отсчета
    public bool countdown;//countdown - обратный отсчет
    public int maxTurnTimer = 30;
    int currentTimer;
    float internalTimer;

    void Start()
    {
        //получим единичные объекты из других скриптов
        charM = CharacterManager.GetInstance();
        levelUI = levelUI.GetInstance();

        //задаем количество секунд переменной oneSec
        oneSec = new WaitForSeconds(1);

        levelUI.AnnouncerTextLine1.gameObject.SetActive(false);
        levelUI.AnnouncerTextLine2.gameObject.SetActive(false);

        StartCoroutine("StartGame");
    }
    void FixedUpdate()
    {
        //Быстрый способ ручной ориентировки игрока на сцене
        //сопоставим направления игроков по x координате

        if(charM.players[0].playerStates.transform.position.x <
             charM.players[1].playerStates.transform.position.x)   
        {
            charM.players[0].playerStates.lookRight = true;
            charM.players[1].playerStates.lookRight = false;
        }
        else
        {
            charM.players[0].playerStates.lookRight = false;
            charM.players[1].playerStates.lookRight = true;
        }
    }
    void Update()
    {
        if(countdown)//если активирован обратный отсчет
        {
            HandleTurnTimer();//контроллирующий таймер
        }
    }
    void HandleTurnTimer()
    {
        levelUI.LevelTimer.text = currentTimer.ToString();

        internalTimer += Time.deltaTime; //каждую секунду

        if(internalTimer > 1)
        {
            currentTimer--;//вычитает изопределенного времени секунду
            internalTimer = 0;
        }

        if (currentTimer <= 0)//когда время на таймере истекло
        {
            EndTurnFunction(true);//заканчиваем ход
            countdown = false;
        }
    }
    IEnumerator StartGame()
    {
        //когда в первый раз запускаем бой

        //сперва нужно создать игроков
        yield return CreatePlayers();

        //затем проверить, какой это ход (?)
        yield return InitTurn();
    }
    IEnumerator InitTurn()
    {
        //чтобы инициализировать ход боя (?)

        //выключает текст уведомлений
        levelUI.AnnouncerTextLine1.gameObject.SetActive(false);
        levelUI.AnnouncerTextLine2.gameObject.SetActive(false);

        //перезапускаем таймер
        currentTimer = maxTurnTimer;
        countdown = false;

        //запускаем игроков на поле боя
        yield return InitPlayers();

        //запускаем корутину, которая активирует доступ к управлению персонажем
        yield return EnableControl();
    }
    IEnumerator InitPlayers()
    {
        //единственное, что нам необхождимо сделать - перезапустить счетчик их жизней
        for(int i = 0; i < charM.players.Count; i++)
        {
            charM.players[i].playerStates.health = 100;
            charM.players[i].playerStates.transform.GetComponent<Animator>().Play("Locomotion");
            charM.players[i].playerStates.transform.position = spawnPositions[i].position;
        }

        yield return null;
    }
    IEnumerator EnableControl()
    {
        //начнем с текста уведомлений
        levelUI.AnnouncerTextLine1.gameObject.SetActive(true);
        levelUI.AnnouncerTextLine1.text = "Turn" + currentTurn;
        levelUI.AnnouncerTextLine1.color = Color.white; //ЗДЕСЬ НУЖНО РАЗОБРАТЬСЯ С ЦВЕТОМ!!
        yield return oneSec;
        yield return oneSec;

        //меняем текст и цвет каждую секунду
        levelUI.AnnouncerTextLine1.text = "3";
        levelUI.AnnouncerTextLine1.color = Color.green;
        yield return oneSec;
        levelUI.AnnouncerTextLine1.text = "2";
        levelUI.AnnouncerTextLine1.color = Color.yellow;
        yield return oneSec;
        levelUI.AnnouncerTextLine1.text = "1";
        levelUI.AnnouncerTextLine1.color = Color.red;
        yield return oneSec;
        levelUI.AnnouncerTextLine1.color = Color.red;
        levelUI.AnnouncerTextLine1.text = "FIGHT!";
        
        //каждый игрок включает все, что ему необходимо для открытия контроллера персонажем
        for (int i = 0; i < charM.players.Count; i++)
        {
            //для игроков, включивших ручное управление 
            if(charM.players[i].playerType == PlayerBase.PlayerType.user)
            {
                InputHandler ih = charM.players[i].playerStates.gameObject.GetComponent<InputHandler>();
                ih.playerInput = charM.players[i].inputId;
                ih.enabled = true;
            }
        }

        //спустя секунду выключаем текст уведомлений
        yield return oneSec;
        levelUI.AnnouncerTextLine1.gameObject.SetActive(false);
        countdown = true;
    }
    IEnumerator CreatePlayers()
    {
        //рассматриваем лист со всеми присутствующими игроками
        for(int i = 0; i < charM.players.Count; i++)
        {
            //и экземпляры их префабов
            GameObject go = Instantiate(charM.players[i].playerPrefab
            , spawnPositions[i].position, Quaternion.identity);

            //и помечаем необходимые образцы
            charM.players[i].playerStates = go.GetComponent<StateManager>();
            
            charM.players[i].playerStates.healthSlider = levelUI.healthSliders[i];
        }

        yield return null;
    }
    public void EndTurnFunction(bool timeOut = false)
    {
        /* Вызываем эту функцию всякий раз, когда необходимо закончить ход
        * но необходимо знать, делаем это ли потому, что время истекло или по другой причине
        */
        countdown = false;
        //перезапускаем вермя текстового отображения таймера
        levelUI.LevelTimer.text = maxTurnTimer.ToString();

        //если время вышло, то...
        if(timeOut)
        {
            //сперва выводим этот текст
            levelUI.AnnouncerTextLine1.gameObject.SetActive(true);
            levelUI.AnnouncerTextLine1.text = "Time Out!";
            levelUI.AnnouncerTextLine1.color = Color.cyan;
        }
        else
        {
            levelUI.AnnouncerTextLine1.gameObject.SetActive(true);
            levelUI.AnnouncerTextLine1.text = "K.O.";
            levelUI.AnnouncerTextLine1.color = Color.red;
        }

        //выключаем управление
        DisableControl();

        //запускаем кортину для конца хода
        StartCoroutine("EndTurn");
    }
    void DisableControl()
    {
        //выключаем компоненты, позволяющие игроку управлять персонажем
        for(int i = 0; i < charM.players.Count; i++)
        {
            //сначала выключим кое-что в StateManager
            charM.players[i].playerStates.ResetStateInputs();
            
            //для живого игрока
            if(charM.players[i].playerType == PlayerBase.PlayerType.user)
            {
                charM.players[i].playerStates.GetComponent<InputHandler>().enabled = false;
            }
        }
    }
    IEnumerator EndTurn()
    {
        yield return oneSec;
        yield return oneSec;
        yield return oneSec;

        //кто победитель?
        PlayerBase vPlayer = FindWinningPlayer();

        if(vPlayer == null)//если функция вернула null, значит ничья
        {
            levelUI.AnnouncerTextLine1.text = "Draw";
            levelUI.AnnouncerTextLine1.color = Color.blue;
        }
        else
        {
            levelUI.AnnouncerTextLine1.text = vPlayer.playerId + "Wins!";
            levelUI.AnnouncerTextLine1.color = Color.red;
        }

        yield return oneSec;
        yield return oneSec;
        yield return oneSec;

        //проверяем получил ли игрок урон вообще
        if (vPlayer != null)
        {
            //если нет
            if(vPlayer.playerStates.health == 100)
            {
                levelUI.AnnouncerTextLine2.gameObject.SetActive(true);
                levelUI.AnnouncerTextLine2.text = "Flawless victory!";
            }
        }

        yield return oneSec;
        yield return oneSec;
        yield return oneSec;

        currentTurn++;//считаем раунд

        bool matchOver = isMatchOver();

        if(!matchOver)
        {
            StartCoroutine("InitTurn2");
        }
        else
        {
            for(int i = 0; i < charM.players.Count; i++)
            {
                charM.players[i].hasCharacter = false;
            }

            SceneManager.LoadSceneAsync("select");
        }
    }
    PlayerBase FindWinningPlayer()
    {
        //чтобы найти победителя
        PlayerBase retVal = null;

        StateManager targetPlayer = null;

        //проверяем равенство здоровья персонажей
        if(charM.players[0].playerStates.health != charM.players[1].playerStates.health)
        {
            if(charM.players[0].playerStates.health < charM.players[1].playerStates.health)
            {
                charM.players[1].score++;
                targetPlayer = charM.players[1].playerStates;
                levelUI.AddWinIndicator(1);
            }
            else
            {
                charM.players[0].score++;
                targetPlayer = charM.players[0].playerStates;
                levelUI.AddWinIndicator(0);
            }

            retVal = charM.returnPlayerFromStates(targetPlayer);
        }

        return retVal;
    }
    bool isMatchOver()
    {
        bool retVal = false;

        for(int i = 0 ; i < charM.players.Count; i++)
        {
            if(charM.players[i].score >= maxTurns)
            {
                retVal = true;
                break;
            }
        }

        return retVal;
    }
    public static LevelManager instance;
    public static LevelManager GetInstance()
    {
        return instance;
    }
    void Awake()
    {
        instance = this;
    }
    IEnumerator InitTurn2()
    {
        levelUI.AnnouncerTextLine1.gameObject.SetActive(false);
        levelUI.AnnouncerTextLine2.gameObject.SetActive(false);

        currentTimer = maxTurnTimer;
        countdown = false;

        yield return InitPlayers();

        yield return EnableControl();
    }
}
