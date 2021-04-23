using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class SelectScreenManager : MonoBehaviour
{
    public int numberOfPlayers = 1;
    public List<PlayerInterfaces> plInterfaces = new List<PlayerInterfaces>();
    int maxRow;
    int maxCollumn;
    List<PotraitInfo> potraitList = new List<PotraitInfo>();

    public GameObject potraitCanvas; // рамка, содержащая в себе все портреты возможных бойцов
   
    bool loadLevel; //если мы выбрали бойца, тогда загружаем уровень
    public bool bothPlayersSelected;

    CharacterManager charManager;

    GameObject potraitPrefab;

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

        potraitPrefab = Resources.Load("potraitPrefab") as GameObject;
        CreatePotraits();

        charManager.solo = (numberOfPlayers == 1);

    }

    void CreatePotraits()
    {
        GridLayoutGroup group = potraitCanvas.GetComponent<GridLayoutGroup>();

        maxRow = group.constraintCount;
        int x = 0;
        int y = 0;

        for (int i = 0; i < charManager.characterList.Count; i++)
        {
            CharacterBase c = charManager.characterList[i];

            GameObject go = Instantiate(potraitPrefab) as GameObject;
            go.transform.SetParent(potraitCanvas.transform);

            PotraitInfo p = go.GetComponent<PotraitInfo>();
            p.img.sprite = c.icon;
            p.characterId = c.charId;
            p.posX = x;
            p.posY = y;
            potraitList.Add(p);

            if(x < maxRow-1)
            {
                x++;
            }
            else
            {
                x=0;
                y++;
            }

            maxCollumn = y;
        }
    }

    void Update()
    {
        if (!loadLevel)
        {
            for (int i = 0; i < plInterfaces.Count; i++)
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
            StartCoroutine("LoadLevel"); //и начинаем корутину загрузки уровня
            loadLevel = true;
            bothPlayersSelected = false;
        }
        else
        {
            if(charManager.players[0].hasCharacter 
                && charManager.players[1].hasCharacter)
            {
                bothPlayersSelected = true;
            }
           
        }
    }
  
    void HandleSelectScreenInput(PlayerInterfaces pl, string playerId)
    {
        #region Grid Navigation

        /*To navigate in the grid
         * we simply change the active x and y to select what entry is active
         * we also smooth out the input so if the user keeps pressing the button
         * it won't switch more than once over half a second
         */

        float vertical = Input.GetAxis("Vertical" + playerId);

        if (vertical != 0)
        {
            if (!pl.hitInputOnce)
            {
                if (vertical > 0)
                {
                    pl.activeY = (pl.activeY > 0) ? pl.activeY - 1 : maxCollumn;
                }
                else
                {
                    pl.activeY = (pl.activeY < maxCollumn) ? pl.activeY + 1 : 0;
                }

                pl.hitInputOnce = true;
            }
        }

        float horizontal = Input.GetAxis("Horizontal" + playerId);

        if (horizontal != 0)
        {
            if (!pl.hitInputOnce)
            {
                if (horizontal > 0)
                {
                    pl.activeX = (pl.activeX > 0) ? pl.activeX - 1 : maxRow-1;
                }
                else
                {
                    pl.activeX = (pl.activeX < maxRow-1) ? pl.activeX + 1 : 0;
                }

                pl.timerToReset = 0;
                pl.hitInputOnce = true;
            }
        }
       
        if(vertical == 0 && horizontal == 0)
        {
            pl.hitInputOnce = false;
        }

        if (pl.hitInputOnce)
        {
            pl.timerToReset += Time.deltaTime;

            if (pl.timerToReset > 0.8f)
            {
                pl.hitInputOnce = false;
                pl.timerToReset = 0;
            }
        }

        #endregion

        //если игрок нажал "пробел", то он определился в выборе бойца
        if (Input.GetButtonUp("Fire1" + playerId))
        {
            //проиграем анимацию удара (можно любую другую), 
            //чтобы показать реакцию персонажа, потому что почему бы и нет
            pl.createdCharacter.GetComponentInChildren<Animator>().Play("Kick");

            //пропустить персонажа в контроллер персонажей, чтобы он понял, какой префаб выбран игроком
            pl.playerBase.playerPrefab =
               charManager.returnCharacterWithID(pl.activePotrait.characterId).prefab;

            pl.playerBase.hasCharacter = true;
        }
    }

    IEnumerator LoadLevel()
    {
        //проверяем, является ли игрок компьютером
        for (int i = 0; i < charManager.players.Count; i++)
        {
            if(charManager.players[i].playerType == PlayerBase.PlayerType.ai)
            {
                if(charManager.players[i].playerPrefab == null)
                {
                    int ranValue = Random.Range(0, potraitList.Count);

                    charManager.players[i].playerPrefab = 
                        charManager.returnCharacterWithID(potraitList[ranValue].characterId).prefab;

                    Debug.Log(potraitList[ranValue].characterId);
                }
            }
        }

        yield return new WaitForSeconds(2);

        if (charManager.solo)
        {
            MySceneManager.GetInstance().CreateProgression();
            MySceneManager.GetInstance().LoadNextOnProgression();
        }
        else
        {
            MySceneManager.GetInstance().RequestLevelLoad(SceneType.prog, "level_1");
        }

    }

    void HandleSelectorPosition(PlayerInterfaces pl) //обновляет положение рамки выбора бойца
    {
        pl.selector.SetActive(true); //включить рамку выбора бойца

        PotraitInfo pi = ReturnPotrait(pl.activeX, pl.activeY);

        if (pi != null)
        {
            pl.activePotrait = pi;//находим активный портрет бойца

            //помещаем рамку на выбранную позицию
            Vector2 selectorPosition = pl.activePotrait.transform.localPosition;

            selectorPosition = selectorPosition + new Vector2(potraitCanvas.transform.localPosition.x
                , potraitCanvas.transform.localPosition.y);

            pl.selector.transform.localPosition = selectorPosition;
        }
    }

    void HandleCharacterPreview(PlayerInterfaces pl)
    {
        //если у нас есть отображаемый портрет бойца, который не совпадает с активным (выбираемым нами)
        //это значит мы изменили персонажей
        if (pl.previewPotrait != pl.activePotrait)
        {
            if (pl.createdCharacter != null)//удаляем выбор персонажа, если игрок осуществил таковой
            {
                Destroy(pl.createdCharacter);
            }

            //и мы создаем нового
            GameObject go = Instantiate(
                CharacterManager.GetInstance().returnCharacterWithID(pl.activePotrait.characterId).prefab,
                pl.charVisPos.position,
                Quaternion.identity) as GameObject;

            pl.createdCharacter = go;

            pl.previewPotrait = pl.activePotrait;

            if(!string.Equals(pl.playerBase.playerId, charManager.players[0].playerId))
            {
                pl.createdCharacter.GetComponent<StateManager>().lookRight = false;
            }
        }
    }


    PotraitInfo ReturnPotrait(int x, int y)
    {
        PotraitInfo r = null;
        for (int i = 0; i < potraitList.Count; i++)
        {
            if(potraitList[i].posX == x && potraitList[i].posY == y)
            {
                r = potraitList[i];
            }
        }

        return r;
    }

    [System.Serializable]
    public class PlayerInterfaces
    {
        public PotraitInfo activePotrait; //определенный активный портрет бойца для player1
        public PotraitInfo previewPotrait; 
        public GameObject selector; //индикатор выбора для player 1 (рамка)
        public Transform charVisPos; //отображение позиции для player 1 игрока
        public GameObject createdCharacter; //задаем бойца для player 1

        public int activeX;//активные X и Y для входа для player 1
        public int activeY;

        //стирает рамку выбора предыдущего бойца
        public bool hitInputOnce;
        public float timerToReset;

        public PlayerBase playerBase;

    }

}
