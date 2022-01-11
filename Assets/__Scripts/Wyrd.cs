using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wyrd    //не наследует MonoBehavior
{
    public string str;  //строковое представление слова 
    public List<Letter> letters = new List<Letter>();
    public bool found = false; //получит true, если игрок нашел это слово

    //свойство управляющее видимостью компонента 3D Text каждой плитки Letter
    public bool visible
    {
        get
        {
            if (letters.Count == 0) return (false);
            return (letters[0].visible);
        }
        set
        {
            foreach (Letter l in letters)
            {
                l.visible = value;
            }
        }
    }
    //свойство для назначения цвета каждой плитке Letter
    public Color Color
    {
        get
        {
            if (letters.Count == 0) return (Color.black);
            return (letters[0].Color);
        }
        set
        {
            foreach(Letter l in letters)
            {
                l.Color = value;
            }
        }
    }
    //добавляет плитку в список letters 
    public void Add(Letter l)
    {
        letters.Add(l);
        str += l.c.ToString();
    }
}
