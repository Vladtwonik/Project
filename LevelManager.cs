using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour {
    
    WaitForSeconds oneSec;//будем много использовать
    public Transform[] spawnPositions;// позиции спавна персонажей


    CameraManager camM;
    CharacterManager charM;
    LevelUI levelUI;//будем хранить все UI элементы здесь для удобного использования

    public int maxTurns = 2;
    int currentTurn = 1;//определенный ход, который мы совершаем в игре, начиная с первого

    //переменные для обратного отсчета
    public bool countdown;
    public int maxTurnTimer = 30;
    int currentTimer;
    float internalTimer;

	void Start () {
        //получим объекты из других скриптов
        charM = CharacterManager.GetInstance();
        levelUI = LevelUI.GetInstance();
        camM = CameraManager.GetInstance();

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

        if (charM.players[0].playerStates.transform.position.x < 
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
        if (countdown)//если активирован обратный отсчет
        {
            HandleTurnTimer();//контроллирующий таймер
        }
    }

    void HandleTurnTimer()
    {
        levelUI.LevelTimer.text = currentTimer.ToString();

        internalTimer += Time.deltaTime;  //каждую секунду

        if (internalTimer > 1)
        {
            currentTimer--;//вычитает из определенного времени секунду
            internalTimer = 0;
        }

        if (currentTimer <= 0) //когда время на таймере истекло
        {
            EndTurnFunction(true); //заканчиваем ход
            countdown = false;
        }
    }

    IEnumerator StartGame()
    {
        //когда в первый раз запускаем бой

        //сперва нужно создать игроков
        yield return CreatePlayers();

        //затем проверить, какой это ход 
        yield return InitTurn();
    }
	
    IEnumerator InitTurn()
    {
        //чтобы инициализировать ход боя

        //выключает текст на экране
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

    IEnumerator CreatePlayers()
    {
        //рассматриваем лист со всеми присутствующими игроками
        for (int i = 0; i < charM.players.Count; i++)
        {
            //и экземпляры их префабов
            GameObject go = Instantiate(charM.players[i].playerPrefab
            , spawnPositions[i].position, Quaternion.identity)
            as GameObject;

            //и помечаем необходимые образцы
            charM.players[i].playerStates = go.GetComponent<StateManager>();

            charM.players[i].playerStates.healthSlider = levelUI.healthSliders[i];

            camM.players.Add(go.transform);
        }

        yield return null;
    }

    IEnumerator InitPlayers()
    {
        //единственное, что нам необхождимо сделать - перезапустить счетчик их жизней
        for (int i = 0; i < charM.players.Count; i++)
        {
            charM.players[i].playerStates.health = 100;
            charM.players[i].playerStates.handleAnim.anim.Play("Locomotion");
            charM.players[i].playerStates.transform.position = spawnPositions[i].position;
        }

        yield return null;
    }

	IEnumerator EnableControl()
    {
        //начнем с текста уведомлений

        levelUI.AnnouncerTextLine1.gameObject.SetActive(true);
        levelUI.AnnouncerTextLine1.text = "Turn " + currentTurn;
        levelUI.AnnouncerTextLine1.color = Color.white;
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

            //если это компьютер-игрок
             if(charM.players[i].playerType == PlayerBase.PlayerType.ai)
             {
                 AICharacter ai = charM.players[i].playerStates.gameObject.GetComponent<AICharacter>();
                 ai.enabled = true;
                 
                 //assign the enemy states to be the one from the opposite player
                 ai.enStates = charM.returnOppositePlayer(charM.players[i]).playerStates;
             }
        }

        //спустя секунду выключаем текст уведомлений
        yield return oneSec;
        levelUI.AnnouncerTextLine1.gameObject.SetActive(false);
        countdown = true;
    } 

    void DisableControl()
    {
        //to disable the controls, you need to disable the component that makes a character controllable
        for (int i = 0; i < charM.players.Count; i++)
        {
            //but first, reset the variables in their state manager 
            charM.players[i].playerStates.ResetStateInputs();

            //for user players, that's the input handler
            if(charM.players[i].playerType == PlayerBase.PlayerType.user)
            {
                charM.players[i].playerStates.GetComponent<InputHandler>().enabled = false;
            }

            if(charM.players[i].playerType == PlayerBase.PlayerType.ai)
            {
                charM.players[i].playerStates.GetComponent<AICharacter>().enabled = false;
            }
        }
    }

    public void EndTurnFunction(bool timeOut = false)
    {
        /* We call this function everytime we want to end the turn
         * but we need to know if we do so by a timeout or not
         */
        countdown = false;
        //reset the timer text
        levelUI.LevelTimer.text = maxTurnTimer.ToString() ;

        //if it's a timeout
        if (timeOut)
        {
            //add this text first
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

        //disable the controlls
        DisableControl();

        //and start the coroutine for end turn
        StartCoroutine("EndTurn");
    }

    IEnumerator EndTurn()
    {
        yield return oneSec;
        yield return oneSec;
        yield return oneSec;

        //кто победитель?
        PlayerBase vPlayer = FindWinningPlayer();

        if(vPlayer == null) //если функция вернула null, значит ничья
        {
            levelUI.AnnouncerTextLine1.text = "Draw";
            levelUI.AnnouncerTextLine1.color = Color.blue;
        }
        else
        {
            //однако если победитель-игрок есть, то...
            levelUI.AnnouncerTextLine1.text = vPlayer.playerId + " Wins!";
            levelUI.AnnouncerTextLine1.color = Color.red;
        }

        yield return oneSec;
        yield return oneSec;
        yield return oneSec;

        //проверяем получил ли игрок урон вообще
        if (vPlayer != null)
        {
            //если нет, то это безукоризненная победа
            if (vPlayer.playerStates.health == 100)
            {
                levelUI.AnnouncerTextLine2.gameObject.SetActive(true);
                levelUI.AnnouncerTextLine2.text = "Flawless Victory!";
            }
        }

        yield return oneSec;
        yield return oneSec;
        yield return oneSec;

        currentTurn++;//считаем раунд

        bool matchOver = isMatchOver();

        if (!matchOver)
        {
            StartCoroutine("InitTurn"); 
        }
        else
        {
            for (int i = 0; i < charM.players.Count; i++)
            {
                charM.players[i].score = 0;
                charM.players[i].hasCharacter = false;
            }

            if (charM.solo)
            {
                if(vPlayer == charM.players[0])
                    MySceneManager.GetInstance().LoadNextOnProgression();
                else
                    MySceneManager.GetInstance().RequestLevelLoad(SceneType.main, "game_over");
            }
            else
            {
                MySceneManager.GetInstance().RequestLevelLoad(SceneType.main, "select");
            }
        }
    }
  
    bool isMatchOver()
    {
        bool retVal = false;

        for (int i = 0; i < charM.players.Count; i++)
        {
            if(charM.players[i].score >= maxTurns)
            {
                retVal = true;
                break;
            }
        }

        return retVal;
    }

    PlayerBase FindWinningPlayer()
    {
        //чтобы найти победителя
        PlayerBase retVal = null;

        StateManager targetPlayer = null;

        //проверяем равенство здоровья персонажей
        if (charM.players[0].playerStates.health != charM.players[1].playerStates.health)
        {
            // если не соблюдается равенство, то проверяем, у кого здоровье меньше, другой игрок будет являться победителем 
            if (charM.players[0].playerStates.health < charM.players[1].playerStates.health)
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

    public static LevelManager instance;
    public static LevelManager GetInstance()
    {
        return instance;
    }

    void Awake()
    {
        instance = this;
    }
   
}

