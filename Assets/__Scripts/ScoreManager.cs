using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScoreManager : MonoBehaviour
{
    static private ScoreManager S;   //скрытый обьект одиночка

    [Header("Set in Inspector")]
    public List<float> scoreFontSizes = new List<float> { 36, 64, 64, 1 };
    public Vector3 scoreMidPoint = new Vector3(1, 1, 0);
    public float scoreTraveltime = 3f;
    public float scoreComboDelay = 0.5f;

    private RectTransform rectTrans;

    void Awake()
    {
        S = this;
        rectTrans = GetComponent<RectTransform>();
    }

    //этот метод можно вызвать как ScoreManager.SCORE() из любого места
    static public void SCORE(Wyrd wyrd, int combo)
    {
        S.Score(wyrd, combo);
    }

    //добавить очки за это слово
    //int combo - номер этого слова в комбинации
    void Score(Wyrd wyrd, int combo)
    {
        //Создать список List<vector2> с точками, определяющими кривую Безье для FloatingScore
        List<Vector2> pts = new List<Vector2>();

        //получить позицию плитки с первой буквой в wyrd
        Vector3 pt = wyrd.letters[0].transform.position;           //a
        pt = Camera.main.WorldToViewportPoint(pt);
        pts.Add(pt);   //сделаь pt первой точкой кривой Безье
        //добавить вторую точку
        pts.Add(scoreMidPoint);
        //сделать Scoreboard последней точкой кривой Безье
        pts.Add(rectTrans.anchorMax);
        //определить значение для FloatingScore 
        int value = wyrd.letters.Count * combo;
        FloatingScore fs = Scoreboard.S.CreateFloatingScore(value, pts);

        fs.timeDuration = scoreTraveltime;
        fs.timeStart = Time.time + combo * scoreComboDelay;
        fs.fontSizes = scoreFontSizes;

        //удвоить эффект InOut из Easing
        fs.easingCurve = Easing.InOut + Easing.InOut;

        //вывести в FloatingScore еукт вида "3 х 2" 
        string txt = wyrd.letters.Count.ToString();
        if(combo > 1)
        {
            txt += " x " + combo;
        }
        fs.GetComponent<Text>().text = txt;
    }
}
