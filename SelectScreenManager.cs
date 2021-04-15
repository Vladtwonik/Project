using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class SelectScreenManager : MonoBehaviour
{
    public int numberOfPlayers = 1;
    public List<PlayerInterfaces> plInterfaces = new List<PlayerInterfaces>();
    public PotraitInfo[] potraitPrefabs; //все входы в игру (?) - портреты бойцов (собраны в одном массиве)
    public int maxX; //сколько портретов на X и Y (?), НО ЭТО ОЧЕНЬ СЛОЖНО
    public int maxY;
    PotraitInfo[,] charGrid; //сетка, которую мы создаем для выбора входов
    //запятая в квадратных скобочках - двумерный массив

    public GameObject potraitCanvas; //рамка, в которой содержатся все портреты бойцов

    bool loadLevel; //если мы выбрали бойца, тогда 
    public bool bothPlayersSelected;

    CharacterManager charManager;

    #region Singleton
    public static SelectScreenManager instance;
    public static SelectScreenManager GetInstance()
    {
        return instance;
    }

    void Awake()
    {
        instance = this;
    }
    #endregion

    void Start() 
    {
        //начинаем с получения данных о персонаже, контроллера, в котором хранится информация о таковом
        charManager = CharacterManager.GetInstance();
        numberOfPlayers = charManager.numberOfUsers;

        //создаем сетку
        charGrid = new PotraitInfo[maxX, maxY];

        int x = 0;
        int y = 0;

        potraitPrefabs = potraitCanvas.GetComponentsInChildren<PotraitInfo>();

        //нужно войти во все наши портреты
        for (int i = 0 ; i < potraitPrefabs.Length ; i++)
        {
            potraitPrefabs[i].posX += x;
            potraitPrefabs[i].posY += y;

            charGrid[x, y] = potraitPrefabs[i];

            if (x < maxX - 1) 
            {
                x++;
            }
            else
            {
                x = 0;
                y++;
            }
        }
    }

    void Update()
    {
        if (!loadLevel)
        {
            for (int i = 0 ; i < plInterfaces.Count ; i++)
            {
                if (i < numberOfPlayers)
                {
                    if (Input.GetButtonUp("Fire2" + charManager.players[i].inputId))
                        {
                            plInterfaces[i].playerBase.hasCharacter = false;
                        }
                    
                    if (!charManager.players[i].hasCharacter)
                    {
                        plInterfaces[i].playerBase = charManager.players[i];

                        HandleSelectorPosition(plInterfaces[i]);
                        HandleSelectScreenInput(plInterfaces[i], charManager.players[i].inputId);
                        HandleCharacterPreview(plInterfaces[i]);
                    }
                }
                else
                {
                    charManager.players[i].hasCharacter = true;
                }
            }
        }

        if(bothPlayersSelected)
        {
            Debug.Log("loading");
            StartCoroutine("LoadLevel");
            loadLevel = true;
        }
        else
        {
            if(charManager.players[0].hasCharacter && charManager.players[1].hasCharacter)
            {
                bothPlayersSelected = true;
            }
        }
    }

    IEnumerator LoadLevel()
    {
        //проверяем, является ли искусственным интеллектом игрок
        for (int i = 0 ; i < charManager.players.Count ; i++)
        {
            if (charManager.players[i].playerType == PlayerBase.PlayerType.ai)
            {
                if(charManager.players[i].playerPrefab == null)
                {
                    int ranValue = Random.Range(0, potraitPrefabs.Length);//В ЭТОЙ СТРОЧКЕ У ТЕБЯ ДОЛЖЕН БЫТЬ НЕ РАНДОМ

                    charManager.players[i].playerPrefab =
                        charManager.returnCharacterWithID(potraitPrefabs[ranValue].characterId).prefab;

                    Debug.Log(potraitPrefabs[ranValue].characterId);
                }
            }
        }

        yield return new WaitForSeconds(2);
        SceneManager.LoadSceneAsync("level", LoadSceneMode.Single);

    }

    void HandleSelectScreenInput(PlayerInterfaces pl, string playerId) //передвижение по сетке
    {
        #region Grid Navigation

        /* Для навигации в сетке бойцов
         * Мы просто меняем активные x и y координаты для выбора активного входа 
         * Также можно стирать рамку вокруг выбранного бойца, если игрок нажимает на кнопку
         * Не переключается чаще чем в половину секунды
         */
        
        float vertical = Input.GetAxis("Vertical" + playerId); //берем ось, клавиши управления которой работают в соответствии с id игрока 

        if(vertical != 0)
        {
            if(!pl.hitInputOnce)
            {
                if(vertical > 0)
                {
                    pl.activeY = (pl.activeY > 0) ? pl.activeY - 1 : maxY - 1;
                }
                else
                {
                    pl.activeY = (pl.activeY < maxY - 1) ? pl.activeY + 1 : 0;
                }

                pl.hitInputOnce = true; 
            }
        }

        float horizontal = Input.GetAxis("Horizontal" + playerId);

        if(horizontal != 0)
        {
            if(!pl.hitInputOnce)
            {
                if(horizontal > 0)
                {
                    pl.activeX = (pl.activeX > 0) ? pl.activeX - 1 : maxX - 1;
                }
                else
                {
                    pl.activeX = (pl.activeX < maxX - 1) ? pl.activeX + 1 : 0;
                }

                pl.timerToReset = 0;
                pl.hitInputOnce = true; 
            }
        }

        if(vertical == 0 && horizontal == 0)
        {
            pl.hitInputOnce = false;
        }

        if(pl.hitInputOnce)
        {
            pl.timerToReset += Time.deltaTime;

            if(pl.timerToReset > 0.8f)
            {
                pl.hitInputOnce = false;
                pl.timerToReset = 0;
            }
        }
        
        #endregion

        //если игрок нажал "пробел", то он определился в выборе бойца
        if(Input.GetButtonUp("Fire1" + playerId))
        {
            //проиграем анимацию удара (можно любую другую), 
            //чтобы показать реакцию персонажа, потому что почему бы и нет
            pl.createdCharacter.GetComponentInChildren<Animator>().Play("Kick"); //ВЫБЕРИ СВОЮ АНИМАЦИЮ

            //пропустить персонажа в контроллер персонажей, чтобы он понял, какой префаб выбран игроком
            pl.playerBase.playerPrefab = charManager.returnCharacterWithID(pl.activePotrait.characterId).prefab;
            
            pl.playerBase.hasCharacter = true;
        }
    }
    void HandleSelectorPosition(PlayerInterfaces pl) //обновляет положение рамки выбора бойца
    {
        pl.selector.SetActive(true);//включить рамку выбора бойца

        pl.activePotrait = charGrid[pl.activeX, pl.activeY];//находим активный портрет бойца

        //помещаем рамку на выбранную позицию
        Vector2 selectorPosition = pl.activePotrait.transform.localPosition;
        selectorPosition = selectorPosition + new Vector2(potraitCanvas.transform.localPosition.x,
                                                          potraitCanvas.transform.localPosition.y);
        pl.selector.transform.localPosition = selectorPosition;
    }
    void HandleCharacterPreview(PlayerInterfaces pl)//превью и активированный боец - разные вещи, смотри этот метод
    {
        //если у нас есть отображаемый портрет бойца, который не совпадает с активным (выбираемым нами)
        //это значит мы изменили персонажей
        if(pl.previewPortrait != pl.activePotrait)
        {
            if(pl.createdCharacter != null)//удаляем выбор персонажа, если игрок осуществил таковой
            {
                Destroy(pl.createdCharacter);
            }

            //и мы создаем нового
            GameObject go = Instantiate(
                CharacterManager.GetInstance().returnCharacterWithID(pl.activePotrait.characterId).prefab,
                pl.charVisPos.position,
                Quaternion.identity) as GameObject;
            
            pl.createdCharacter = go;

            pl.previewPortrait = pl.activePotrait;

            if(!string.Equals(pl.playerBase.playerId, charManager.players[0].playerId))
            {
                pl.createdCharacter.GetComponent<StateManager>().lookRight = false;
            }
        }
    }
}

[System.Serializable]
public class PlayerInterfaces
{
    public PotraitInfo activePotrait;//определенный активный портрет бойца для первого игрока (для играющего?)
    public PotraitInfo previewPortrait; 
    public GameObject selector;//индикатор выбора для первого игрока (рамка)
    public Transform charVisPos;//отображение позиции для первого игрока
    public GameObject createdCharacter;//задаем бойца для первого игрока (?)
    public int activeX;//активные X и Y для входа для первого игрока
    public int activeY;

    //стирает рамку выбора предыдущего бойца
    public bool hitInputOnce;
    public float timerToReset;
    public PlayerBase playerBase; 
}
