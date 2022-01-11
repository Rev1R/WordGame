using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;   

public enum GameMode
{
    preGame, // перед началом игры
    loading, //список слов загружается и анализируется
    makeLevel, // создается отдельный WordLevel
    levelPrep, //создается уровень с визуальным представлением
    inLevel //уровень запущен
}

public class WordGame : MonoBehaviour
{
    static public WordGame S;  //одиночка

    [Header("Set in Inspector")]
    public GameObject prefabLetter;
    public Rect wordArea = new Rect(-24, 19, 48, 28);
    public float letterSize = 1.5f;
    public bool showAllWyrds = true;
    public float bigLetterSize = 4f;
    public Color bigColorDim = new Color(0.8f, 0.8f, 0.8f);
    public Color bigColorSelected = new Color(1f, 0.9f, 0.7f);
    public Vector3 bigLetterCenter = new Vector3(0, -16, 0);
    public Color[] wyrdPallete;


    [Header("Set Dynamically")]
    public GameMode mode = GameMode.preGame;
    public WordLevel currLevel;
    public List<Wyrd> wyrds;
    public List<Letter> bigLetters;
    public List<Letter> bigLetterActive;
    public string testWord;
    private string upperCase = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

    private Transform letterAnchor, bigLetterAnchor;

    void Awake()
    {
        S = this;   //записать ссылку на обьект одиночку
        letterAnchor = new GameObject("LetterAnchor").transform;
        bigLetterAnchor = new GameObject("BigLetterAnchor").transform;
    }
    void Start()
    {
        mode = GameMode.loading;
        //вызвать статистический метод INIT() класса WordList
        WordList.INIT();
    }
    //вызывается методом SendMEssage() из WordList
    public void WordListParseComplete()
    {
        mode = GameMode.makeLevel;
        //создать уровень и сохранить в currLevel текущий WordLevel
        currLevel = MakeWordLevel();
    }
    public WordLevel MakeWordLevel(int levelNum = -1)        //a
    {
        WordLevel level = new WordLevel();
        if(levelNum == -1)
        {
            //выбрать случайный уровень 
            level.longWordIndex = Random.Range(0, WordList.LONG_WORD_COUNT);
        }
        else
        {
            //код будет добавлен позже
        }
        level.levelNum = levelNum;
        level.word = WordList.GET_lONG_WORD(level.longWordIndex);
        level.charDict = WordLevel.MakeCharDict(level.word);

        StartCoroutine(FindSubWordsCoroutine(level));       //b
        return (level);                                     //c
    }

    //сопрограмма отыскивающая слова которые можно составить на этом уровне
    public IEnumerator FindSubWordsCoroutine(WordLevel level)
    {
        level.subWords = new List<string>();
        string str;

        List<string> words = WordList.GET_WORDS();           //d

        //выполнить обход всех слов в WordList
        for(int i=0; i<WordList.WORD_COUNT; i++)
        {
            str = words[i];
            //проверить можно ли его составить из символов в level.charDict
            if(WordLevel.CheckWordInLevel(str, level))
            {
                level.subWords.Add(str);
            }
            //приостановиться после анализа заданного числа слов в этом кадре
            if(i%WordList.NUM_TO_PARSE_BEFORE_YIELD == 0)
            {
                //приостановиться до следующего кадра 
                yield return null;
            }
        }
        level.subWords.Sort();                                //e
        level.subWords = SortWordsByLength(level.subWords).ToList();

        //сопрограмма завершила анализ, поэтому вызываем SubWordSearchComplete()
        SubWordSearchComplete();
    }

    //Использует LINQ для сортировки массива и возвращает его копию    //f
    public static IEnumerable<string> SortWordsByLength(IEnumerable<string> ws)
    {
        ws = ws.OrderBy(s => s.Length);
        return (ws);
    }

    public void SubWordSearchComplete()
    {
        mode = GameMode.levelPrep;
        Layout();    //вызвать layout() один раз после выполнения WordSearch
    }
    void Layout()
    {
        //Поместить на экран плитки с буквами каждого возможного слова текущего уровня
        wyrds = new List<Wyrd>();

        //поместить на экран плитки с будут использоваться методом 
        GameObject go;
        Letter lett;
        string word;
        Vector3 pos;
        float left = 0;
        float columnWidth = 3;
        char c;
        Color col;
        Wyrd wyrd;

        //определить,сколько рядов плиток уместиться на экране
        int numRows = Mathf.RoundToInt(wordArea.height / letterSize);

        //создать экземпляр Wyrd для каждого слова в level.subWords
        for(int i=0; i<currLevel.subWords.Count; i++)
        {
            wyrd = new Wyrd();
            word = currLevel.subWords[i];

            //если слово дленнее чем columnWidth развернуть его
            columnWidth = Mathf.Max(columnWidth, word.Length);

            //Создать экземпляр PrefabLetter для каждой буквы в слове
            for(int j=0; j<word.Length; j++)
            {
                c = word[j]; //получить j-й символ слова
                go = Instantiate<GameObject>(prefabLetter);
                go.transform.SetParent(letterAnchor);
                lett = go.GetComponent<Letter>();
                lett.c = c; //назначить букву плитке Letter

                //установить координаты плитки Letter 
                pos = new Vector3(wordArea.x + left + j * letterSize, wordArea.y, 0);

                //оператор % помогает выстроить плитки по вертикали
                pos.y -= (i % numRows) * letterSize;
                //переместить плитку lett немедленно за верхний край экрана
                lett.posImmediate = pos + Vector3.up * (20 + i % numRows);
                //затем начать ее перемещение в новую позицию pos
                lett.pos = pos;   //позднее вокруг этой строки будет добавлен дополнительный код
                //увеличить lett.timeStart для перемещения слов в разные времена
                lett.timeStart = Time.time + i * 0.05f;
                go.transform.localScale = Vector3.one * letterSize;
                wyrd.Add(lett);
            }
            if (showAllWyrds) wyrd.visible = true;

            //определить цвет слова, изходя из его длины
            wyrd.Color = wyrdPallete[word.Length - WordList.WORD_LENGTH_MIN];
            

            wyrds.Add(wyrd);

            //если достигнут последний ряд в столбце, начать новый столбец
            if(i%numRows== numRows - 1)
            {
                left+=(columnWidth + 0.5f) * letterSize;
            }
        }
        //Поместить на экран большие плитки с буквами 
        //Инициализировать список больших букв
        bigLetters = new List<Letter>();
        bigLetterActive = new List<Letter>();

        //Создать большую плитку для каждой буквы в целевом слове
        for(int i=0; i<currLevel.word.Length; i++)
        {
            //Напоминает процедуру создания маленьких плиток
            c = currLevel.word[i];
            go = Instantiate<GameObject>(prefabLetter);
            go.transform.SetParent(bigLetterAnchor);
            lett = go.GetComponent<Letter>();
            lett.c = c;
            go.transform.localScale = Vector3.one * bigLetterSize;

            //Первоначально поместить большие плитки ниже края экрана
            pos = new Vector3(0, -100, 0);
            lett.posImmediate = pos;
            lett.pos = pos;  //позднее вокруг этой строки будет добавлен дополнительный код
            //увелечить lett.timeStart, чтобы большие плитки с буквами появились последними
            lett.timeStart = Time.time + currLevel.subWords.Count * 0.05f;
            lett.easingCuve = Easing.Sin + "-0.18";   //Bouncy easing
            col = bigColorDim;
            lett.Color = col;
            lett.visible = true;   //всегда true для больших плиток
            lett.big = true;
            bigLetters.Add(lett);
        }
        //перемешать плитки
        bigLetters = ShuffleLetters(bigLetters);

        //вывести на экран
        ArrangeBigLetters();

        //установить режим mode-- "в игре"
        mode = GameMode.inLevel;
    }
    //этот метод перемешивает элементы в списке List<Letter> и возвращает результат
    List<Letter>ShuffleLetters(List<Letter> letts)
    {
        List<Letter> newL = new List<Letter>();
        int ndx;
        while(letts.Count > 0)
        {
            ndx = Random.Range(0, letts.Count);
            newL.Add(letts[ndx]);
            letts.RemoveAt(ndx);
        }
        return (newL);
    }

    //этот метод выводит большие плитки на экран
    void ArrangeBigLetters()
    {
        //найти середину для вывода ряда больших плиток с центрированием по горизонтали
        float halfWidth = ((float)bigLetters.Count) / 2f - 0.5f;
        Vector3 pos;
        for(int i=0; i<bigLetters.Count; i++)
        {
            pos = bigLetterCenter;
            pos.x += (i - halfWidth) * bigLetterSize;
            bigLetters[i].pos = pos;
        }
        //bigLettersActive
        halfWidth = ((float)bigLetterActive.Count)/2f - 0.5f;
        for(int i=0; i < bigLetterActive.Count; i++)
        {
            pos = bigLetterCenter;
            pos.x += (i - halfWidth) * bigLetterSize;
            pos.y += bigLetterSize * 1.25f;
            bigLetterActive[i].pos = pos;
        }
    }
    void Update()
    {
        //обьявить пару вспомогательных переменных
        Letter ltr;
        char c;

        switch (mode) 
        {
            case GameMode.inLevel:
                //выполнить обход всех символов, введенных игроком в этом кадре
                foreach (char cIt in Input.inputString)
                {
                    //преобразовать cIt в верхний регистр
                    c = System.Char.ToUpperInvariant(cIt);

                    //проверить есть ли такая буква верхнего регистра
                    if (upperCase.Contains(c))  //любая буква регистра
                    {
                        //найти доступную плитку с этой буквой в bigLetters
                        ltr = FindNextLetterByChar(c);
                        //если плитка найдена
                        if (ltr != null)
                        {
                            //добавить этот символ в testWord и переместить соответствующую плитку Letter в bigLettersActive
                            testWord += c.ToString();
                            //переместить из списка неактивных в список активных плиток
                            bigLetterActive.Add(ltr);
                            bigLetters.Remove(ltr);
                            ltr.Color = bigColorSelected;  //придать плитке активный вид
                            ArrangeBigLetters();  //отобразить плитки
                        }
                    }
                    if (c == '\b')  //BackSpace
                    {
                        //удалить последнюю плитку Letter из bigLettersActive
                        if (bigLetterActive.Count == 0) return;
                        if (testWord.Length > 1)
                        {
                            //удалить последнюю букву из testWord
                            testWord = testWord.Substring(0, testWord.Length - 1);
                        }
                        else
                        {
                            testWord = "";
                        }
                        ltr = bigLetterActive[bigLetterActive.Count - 1];
                        //переместить из списка активных в список неактивных плиток
                        bigLetterActive.Remove(ltr);
                        bigLetters.Add(ltr);
                        ltr.Color = bigColorDim;  //придать плитке неактивный вид
                        ArrangeBigLetters();   //отобразить плитки
                    }
                    if (c == '\n' || c == '\r')          //Enter
                    {
                        //проверить наличие сконструированного слова в Wordevel
                        CheckWord();
                    }
                    if (c ==' ')     //пробел
                    {
                        //перемешать плитки в bigLetters
                        bigLetters = ShuffleLetters(bigLetters);
                        ArrangeBigLetters();
                    }
                }
                break;
        }
    }
    //этот метод отыскивает плитку Letter с символом с в bigLetters
    //Если такой плитки нет, возвращает null
    Letter FindNextLetterByChar(char c)
    {
        //проверить каждую плитку Letter в bigLetters
        foreach(Letter ltr in bigLetters)
        {
            //Если содержит тот же символ что указан в с
            if(ltr.c == c)
            {
                //вернуть ее
                return (ltr);
            }
        }
        return (null);    //иначе вернуть null
    }
    public void CheckWord()
    {
        //проверяет присутствие слова testWord в списке level.subWords
        string subWord;
        bool foundTestWord = false;

        //создать список List<int> для хранения индексов других слов присутствующих в testWord
        List<int> containedWords = new List<int>();

        //обойти все слова в currLevel.subWords
        for(int i=0;i<currLevel.subWords.Count; i++)
        {
            //проверить было ли уже найдено Wyrd
            if (wyrds[i].found)                      //a
            {
                continue;
            }
            subWord = currLevel.subWords[i];
            //проверить, входит ли это слово subWord в testWord
            if (string.Equals(testWord, subWord))       //b
            {
                HighlightWyrd(i);
                ScoreManager.SCORE(wyrds[i], 1);  //подсчитать очки   //а
                foundTestWord = true;
            }
            else if(testWord.Contains(subWord))
            {
                containedWords.Add(i);
            }
        }
        if (foundTestWord)  //если проверяемое слово присутствует в списке - подсветить другие слова содержащиеся в testWord
        {
            int numContained = containedWords.Count;
            int ndx;
            //подсвечивать слова в обратном порядке
            for(int i=0; i < containedWords.Count; i++)
            {
                ndx = numContained - i - 1;
                HighlightWyrd(containedWords[ndx]);
                ScoreManager.SCORE(wyrds[containedWords[ndx]], i + 2);  //b
            }
        }
        //очистить список активных плиток в Letters независимо от того, является ли testWord допустимым
        ClearBigLettersActive();
    }
    //подсвечивает экземпляр Wyrd
    void HighlightWyrd(int ndx)
    {
        //активировать слово
        wyrds[ndx].found = true;   //установить признак что оно найдено
        //выделить цветом
        wyrds[ndx].Color = (wyrds[ndx].Color + Color.white) / 2f;
        wyrds[ndx].visible = true;  //сделать компонент 3D Text видимым
    }
    //удаляет все плитки Letters из bigLettersActive
    void ClearBigLettersActive()
    {
        testWord = "";  //очистить testWord
        foreach(Letter ltr in bigLetterActive)
        {
            bigLetters.Add(ltr);   //добавить каждую плитку в bigLetters
            ltr.Color = bigColorDim;  //придать ей неакивный вид
        }
        bigLetterActive.Clear();   //очистить список
        ArrangeBigLetters();       //повторно вывести плитки на экран
    }
}
