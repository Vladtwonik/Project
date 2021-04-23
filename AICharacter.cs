using UnityEngine;
using System.Collections;

public class AICharacter : MonoBehaviour
{
    #region Variables
    //Хранимые компоненты
    StateManager states;
    public StateManager enStates; //StateManager для противника

    public float changeStateTolerance = 3; //расстояние, на котором должен находится игрок-компьютер от живого игрока

    public float normalRate = 1;  //Скорость, с которой компьютер-игрок решит, что он в нормальном состоянии
    float nrmTimer;

    public float closeRate = 0.5f;//Скорость, с которой компьютер-игрок решит, что он близко от живого игрока
    float clTimer;

    public float blockingRate = 1.5f; //Время удержания блока
    float blTimer;

    public float aiStateLife = 1; //Время, спустя которое изменяется состояние компьютера-игрока
    float aiTimer;

    bool initiateAI; //Когда есть состояние, последовательность которого можно запустить
    bool closeCombat;//если мы в ближнем бою

    bool gotRandom; //Помогает с тем, чтобы мы не обновляли нашу рандомную переменную каждый кадр
    float storeRandom; //хранит рандомную переменную


    //блок от ударов 
    bool checkForBlocking;
    bool blocking;

    //количество атак
    bool randomizeAttacks;
    int numberOfAttacks;
    int curNumAttacks;

    //прыжок
    public float JumpRate = 1;
    float jRate;
    bool jump;
    float jtimer;
    #endregion 

    public AttackPatterns[] attackPatterns;

    //состояния компьютера-игрока
    public enum AIState
    { 
        closeState,
        normalState,
        resetAI
    }

    public AIState aiState;

	void Start () {

        states = GetComponent<StateManager>();

        AISnapshots.GetInstance().RequestAISnapshot(this);
	}
		
	void Update () 
    {
        //вызов функций
        CheckDistance();
        States();
        AIAgent();
	}

    //содержит все состояния
    void States()
    {
        //этот switch решает запуск в зависимости от состояния
        switch (aiState)
        { 
            case AIState.closeState:
                CloseState();
                break;
            case AIState.normalState:
                NormalState();
                break;
            case AIState.resetAI:
                ResetAI();
                break;
        }

        Jumping();
    }

    //Управление вещами, которые должен делать компьютер-игрок
    void AIAgent()
    { 
        //Если ему нужно что-то сделать, значит, цикл компьютера-игрока полностью завершился
        if(initiateAI)
        {
            //Запуск процесса сброса состояния компьютера-игрока, что происходит НЕ МГНОВЕННО
            aiState = AIState.resetAI;
            //Создаем множитель вероятности
            float multiplier = 0;

            //Получаем рандомную переменную, если ее нет
            if (!gotRandom)
            {
                storeRandom = ReturnRandom();
                gotRandom = true;
            }

            //Если в состоянии closeCombat
            if (!closeCombat)
            {
                //Имеет больше вероятности к передвижению (+30%)
                multiplier += 30;
            }
            else 
            {
                //..больше шансов атаковать (+30 процентов)
                multiplier -= 30;
            }
      
            //Сравнивает рандомную переменную с добавленными модификаторами
            if (storeRandom + multiplier < 50)
            {
                Attack(); //...и либо атакует
            }
            else
            {
                Movement();//либо движется
            }
        }
    }

    //здесь прописана логика атаки компьютера-игрока
    void Attack()
    {
        //Получаем рандомную переменную
        if (!gotRandom)
        {
            storeRandom = ReturnRandom();
            gotRandom = true;
        }

        //See how many attacks he will do...
        if (!randomizeAttacks)
        {
            //..получение рандомного числа в промежутке от 1 до 4, потому что нужно ударить противника хотя бы раз
            numberOfAttacks = (int)Random.Range(1, 4);
            randomizeAttacks = true;
        }

        //если мы не атаковали больше максимального числа раз
        if (curNumAttacks < numberOfAttacks)
        {
            //Затем наугад решаем, какую атаку мы хотим провести, максимальное число случайного диапазона - это длина нашего массива атак
            int attackNumber = Random.Range(0, attackPatterns.Length);

            StartCoroutine(OpenAttack(attackPatterns[attackNumber],0));
        
            //..и увеличиваем количество совершенных атак
            curNumAttacks++;
        }
    }

    void Movement()
    {
        //Получаем случайное значение
        if (!gotRandom)
        {
            storeRandom = ReturnRandom();
            gotRandom = true;
        }

        //Вероятность приближения к врагу - 90%
        if (storeRandom < 90)
        {
            if (enStates.transform.position.x < transform.position.x)
                states.horizontal = -1;
            else
                states.horizontal = 1;
        }
        else//..или отдаления от такового
        {
            if (enStates.transform.position.x < transform.position.x)
                states.horizontal = 1;
            else
                states.horizontal = -1;
        }

        //нереализованная идея: создать модификатор на основе здоровья
    }

    //сброс ВСЕХ значений переменных
    void ResetAI()
    {
        aiTimer += Time.deltaTime;

        if(aiTimer > aiStateLife)
        {
            initiateAI = false;
            states.horizontal = 0;
            states.vertical = 0;
            aiTimer = 0;

            gotRandom = false;

            //А также есть шанс переключить состояние AI с normal на close, чтобы получить больше случайности 
            storeRandom = ReturnRandom();
            if (storeRandom < 50)
                aiState = AIState.normalState;
            else
                aiState = AIState.closeState;

            curNumAttacks = 1;
            randomizeAttacks = false;
        }
    }
    //В зависимости от проверенного значения расстояния до игрока игрок-компьютер меняет состояние
    void CheckDistance()
    {
        //измеряем дистанцию
        float distance = Vector3.Distance(transform.position, enStates.transform.position);

        //сравниваем с допустимым значением дистанции
        if (distance < changeStateTolerance)
        {
            //Если мы не находимся в процессе сброса состояния, тогда это нужно изменить
            if (aiState != AIState.resetAI)
                aiState = AIState.closeState;

            //Если мы близко к живому игроку, тогда необходимо состояние closeCombat
            closeCombat = true;
        }
        else
        {
            //Если мы не находимся в процессе сброса состояния, тогда это нужно изменить
            if (aiState != AIState.resetAI)
                aiState = AIState.normalState;

            //Если бы мы были близко к противнику, а затем начали бы удаляться ... 
            if (closeCombat)
            { 
                //берем рандомное число
                if(!gotRandom)
                {
                    storeRandom = ReturnRandom();
                    gotRandom = true;
                }

                //... и тогда есть 60% шансов, что наш кломпьтер-игрок последует за врагом 
                if (storeRandom < 60)
                {
                    Movement();
                }
            }

            //Тогда мы, наверное, уже не в ближнем бою
            closeCombat = false;
        }
    }
    //логика блокировки
    void Blocking()
    { 
        //если персонаж компьютера-игрока собирается принять удар 
        if (states.gettingHit)
        {
            //..получаем рандомное число
            if (!gotRandom)
            {
                storeRandom = ReturnRandom();
                gotRandom = true;
            }

            //...50%ый шанс, что удар будет заблокирован
            if (storeRandom < 50)
            {
                blocking = true;
                states.gettingHit = false;
            }
        }

        //Если совершаем блокироваку, то стоит начать считать, сколько раз был совершен блок, чтобы это не повторялось без конца
        if(blocking)
        {
            blTimer += Time.deltaTime;

            if (blTimer > blockingRate)
            {
                blTimer = 0;
            }
        }
    }   
    //normal state, определяющее вероятность действий цикла 
    void NormalState()
    {
        nrmTimer += Time.deltaTime;

        if (nrmTimer > normalRate)
        {
            initiateAI = true;
            nrmTimer = 0;
        }
    }
    //close state, определяющее вероятность действий цикла 
    void CloseState()
    {
        clTimer += Time.deltaTime;

        if (clTimer > closeRate)
        {
            clTimer = 0;
            initiateAI = true;
        }
    }
    //логика прыжка
    void Jumping()
    {
        //если игрок прыгает или компьютер-игрок запускает прыжок сам
        if(!enStates.onGround)
        {
            float ranValue = ReturnRandom();

            if(ranValue < 50)
            {
                jump = true;
            }
        }
        
        if (jump)
        {
            //задаем вертикальный ввод
            states.vertical = 1;
            jRate = ReturnRandom();
            jump = false; //чтобы предотвратить повторение прыжков
        }
        else
        {
            //сбрасываем вертикальный ввод
            states.vertical = 0;
        }

        // таймер прыжка определяет, сколько секунд он будет запускать проверку, будет ли прыгать компьютер-игрок или нет. 
        jtimer += Time.deltaTime;

        if(jtimer > JumpRate*10)
        {
            //50%я вероятность совершения прыжка
            if (jRate < 50)
            {
                jump = true;
            }
            else
            {
                jump = false;
            }

            jtimer = 0;
        }

    }
    //функция, которая выдает рандомное число от 1 до 100 включительно
    float ReturnRandom()
    {
        float retVal = Random.Range(0, 101);
        return retVal;
    }

    IEnumerator OpenAttack(AttackPatterns a, int i)
    {
        int index = i;
        float delay = a.attacks[index].delay;
        states.attack1 = a.attacks[index].attack1;
        states.attack2 = a.attacks[index].attack2;
        yield return new WaitForSeconds(delay);

        states.attack1 = false;
        states.attack2 = false;

        if(index < a.attacks.Length - 1 )
        {
            index++;
            StartCoroutine(OpenAttack(a, index));
        }
       
    }

    [System.Serializable]
    public class AttackPatterns
    {
        public AttacksBase[] attacks;
    }

    [System.Serializable]
    public class AttacksBase
    {
        public bool attack1;
        public bool attack2;
        public float delay;
    }
}

/* Имейте в виду, что всякий раз, когда выполняется процентная проверка, это можно рандомизировать.
 * Плюсы моего кода:
 * Можно управлять процентами в зависимости от настроек сложности.
 * Есть возможность рандомизировать скорость атаки, добавляя и удаляя секунды.
 * Можно сделать разные переменные для каждого случайного значения.
 * В принципе, можно рандомизировать все, и у нас будет более непредсказуемый AI.
 */
