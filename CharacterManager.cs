using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class CharacterManager : MonoBehaviour //класс, позволяющий получить тип и объект персонажа, со всеми его составляющими
{
    public int numberOfUsers;
    public List<PlayerBase> players = new List<PlayerBase>(); //список (массив абстрактного типа) со списком всех наших игроков и типами игроков

    //в списке содержится все, что нам необходимо знать о каждом отдельном персонаже
    //на данный момент, это id персонажа (?) и ему соотвествующий (corresponding) префаб
    public List<CharacterBase> characterList = new List<CharacterBase>();

    //эту функцию мы используем для определения выбираемеого персонажа по его id
    public CharacterBase returnCharacterWithID(string id) //проверяет вводимый id, ищет нужный
    {
        CharacterBase retVal = null;

        for (int i = 0; i < characterList.Count; i++)
        {
            if(string.Equals(characterList[i].charId,id))
            {
                retVal = characterList[i];
                break;
            }
        }

        return retVal;
    }
    //возвращает характеристики персонажа
    public PlayerBase returnPlayerFromStates(StateManager states)
    {
        PlayerBase retVal = null;

        for (int i = 0; i < players.Count; i++)
        {
            if(players[i].playerStates == states)
            {
                retVal = players[i];
                break;
            }
        }

        return retVal;
    }

    public static CharacterManager instance;
    public static CharacterManager GetInstance()
    {
        return instance;
    }

    void Awake()
    {
        instance = this;
        DontDestroyOnLoad(this.gameObject);
    }
}


[System.Serializable]
public class CharacterBase
{
    public string charId;
    public GameObject prefab;
}

[System.Serializable]
public class PlayerBase
{
    public string playerId;
    public string inputId;
    public PlayerType playerType;
    public bool hasCharacter;
    public GameObject playerPrefab;
    public StateManager playerStates;
    public int score;

    public enum PlayerType
    {
        user, //реальный человек (игрок)
        ai, //компьютер
    }
}
