using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]    //экземпляры WordLevel можно просматривать/изменять в инспекторе
public class WordLevel    // WordLevel не наследует MonoBehavior 
{
    public int levelNum;
    public int longWordIndex;
    public string word;

    //словарь со всеми буквами в слове
    public Dictionary<char,int>  charDict;
    //все слова которые можно составить из букв в charDict
    public List<string> subWords;

    //статистическая функция, подсчитывает колличество вхождений символов в строку и возвращает
    //словарь Dictionary<char, int> с этой информацией
    static public Dictionary<char, int> MakeCharDict(string w)
    {
        Dictionary<char, int> dict = new Dictionary<char, int>();
        char c;
        for (int i=0; i<w.Length; i++)
        {
            c = w[i];
            if (dict.ContainsKey(c))
            {
                dict[c]++;
            }
            else
            {
                dict.Add(c, 1);
            }
        }
        return (dict);
    }

    //статистический метод, проверяет возмоность составить слово из 
    //символов в level.charDict
    public static bool CheckWordInLevel(string str, WordLevel level)
    {
        Dictionary<char, int> counts = new Dictionary<char, int>(); 
        for(int i=0; i<str.Length; i++)
        {
            char c = str[i];
            //если charDict содержит символ с
            if (level.charDict.ContainsKey(c))
            {//если counts еще не содержит ключа с символом с 
                if (!counts.ContainsKey(c))
                {//добавить нвоый ключ со значением 1 
                    counts.Add(c, 1);
                }
                else
                {//в противном случае прибавить 1 к текущему значению
                    counts[c]++;
                }
                //если число вхождений символа с в строку str превысило
                // доступное количество в level.charDict
                if(counts[c] > level.charDict[c])
                {
                    //..ернуть false
                    return (false);
                }
            }
            else
            {
                //символ с отсутствует в level.word, вернуть false
                return (false);
            }
        }
        return (true);
    }

}
