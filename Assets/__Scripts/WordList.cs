using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WordList : MonoBehaviour
{
    private static WordList S;            //a

    [Header("Set in Inspector")]
    public TextAsset wordListText;
    public int numToParseBeforYield = 10000;
    public int wordLengthMin = 3;
    public int wordLengthMax = 7;

    [Header("Set Dynamically")]
    public int currLine = 0;
    public int totalLines;
    public int longWordCount;
    public int wordCount;

    //скрытые поля 
    private string[] lines;                    //b
    private List<string> longWords;
    private List<string> words;

    void Awake()
    {
        S = this;  // подготовка обьекта одиночки WordLIst
    }
    public void Init()
    {
        lines = wordListText.text.Split('\n');               //c
        totalLines = lines.Length;

        StartCoroutine(ParseLines());                         //d
    }

    static public void INIT()
    {
        S.Init();
    }
    //все сопрограммы возвращают значения типа IEnumerator
    public IEnumerator ParseLines()           //e
    {
        string word;
        //инициализировать список для хранения длиннейших слов из числа допустимых
        longWords = new List<string>();
        words = new List<string>();               //f

        for (currLine = 0; currLine < totalLines; currLine++)            //g
        {
            word = lines[currLine];

            //если длинна слов равна wordLengthMax...
            if (word.Length == wordLengthMax)
            {
                longWords.Add(word);    // сохранить его в longWords
            }
            //если длина слова между wordLengthMin и wordLengthMax ..
            if (word.Length >= wordLengthMin && word.Length <= wordLengthMax)
            {
                words.Add(word);   //добавить его в список допустимых слов
            }
            //определить не пора ли сделать перерыв
            if (currLine % numToParseBeforYield == 0)           //h
            {
                //подсчитать слова в каждом списке, чтобы показать как протекает процесс анализа
                longWordCount = longWords.Count;
                wordCount = words.Count;

              

                //приостановить выполнение сопрограммы до след кадра
                yield return null;           //i

                //инструкция yield приостановит выполнение этого метода, даст возможность выполниться другому коду
                //и возобновит выполнение сопрограммы с этой точки, начав следующую итерацию цикла for
            }
        }
        longWordCount = longWords.Count;
        wordCount = words.Count;

        //послать игровому обьекту gameObject сообщение об окончании анализа
        gameObject.SendMessage("WordListParseComplete");              //b
    }
    //эти методы позволяют другим классам обращаться к скрытым полям List<string>
    static public List<string> GET_WORDS()         //j
    {
        return (S.words);
    }
    static public string GET_WORD(int ndx)
    {
        return (S.words[ndx]);
    }
    static public List<string> GET_LONG_WORDS()
    {
        return (S.longWords);
    }
    static public string GET_lONG_WORD(int ndx)
    {
        return (S.longWords[ndx]);
    }
    static public int WORD_COUNT
    {
        get{ return S.wordCount; }
    }
    static public int LONG_WORD_COUNT 
    {
        get { return S.longWordCount; }
    }
    static public int NUM_TO_PARSE_BEFORE_YIELD
    {
        get { return S.numToParseBeforYield; }
    }
    static public int WORD_LENGTH_MIN
    {
        get { return S.wordLengthMin; }
    }
    static public int WORD_lENGHT_MAX
    {
        get { return S.wordLengthMax; }
    }
}
