using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Letter : MonoBehaviour
{
    [Header("Set in Inspector")]
    public float timeDuration = 0.5f;
    public string easingCuve = Easing.InOut;  //функция сглаживания из Utils.cs

    [Header("Set Dynamiccaly")]
    public TextMesh tMesh;   //TextMesh отображает символ
    public Renderer tRend;   //Компонент Renderer обьекта 3D Text. Он будет определять видимость символа
    public bool big = false;  //большие и малые плитки действуют по разному

    // поля для линиейной интерполяции 
    public List<Vector3> pts = null;
    public float timeStart = -1;

    private char _c;   //символ отображаемый в этой плитке
    private Renderer rend;

    void Awake()
    {
        tMesh = GetComponentInChildren<TextMesh>();
        tRend = tMesh.GetComponent<Renderer>();
        rend = GetComponent<Renderer>();
        visible = false;
    }

    //свойство для чтения/записи буквы в поле _c, отображаемой объектом 3D Text
    public char c
    {
        get { return (_c); }
        set
        {
            _c = value;
            tMesh.text = _c.ToString();
        }
    }

    //свойство для чтения/записи буквы в поле _с в виде строки
    public string str 
    {
        get { return (_c.ToString()); }
        set { _c = value[0]; }
    }
    //разрешает или запрещает отображение 3D Text, что делает букву 
    //видимой или невидимой соотвественно
    public bool visible
    {
        get { return (tRend.enabled); }
        set { tRend.enabled = value; }
    }
    //свойство для чтения/записи плитки
    public Color Color
    {
        get { return rend.material.color; }
        set { rend.material.color = value; }
    }
    //свойство для чтения/записи координат плитки
    //настраиваем кривую Безье для плавного перемещения в новые координаты
    public Vector3 pos
    {
        set 
        {
            //transform.position = value; 
            //найти среднюю точку на случайном расстоянии от фактической средней точки 
            //между текущей и новой (value) позициями

            Vector3 mid = (transform.position + value) / 2f;

            //случайное расстояние не превышает 1/4 расстояния до фактической средней точки
            float mag = (transform.position - value).magnitude;
            mid += Random.insideUnitSphere * mag * 0.25f;

            //создать List<Vector3> точек определяющих кривую Безье 
            pts = new List<Vector3>() { transform.position, mid, value };

            //если timeStart содержит значение по умолчанию -1, установить текущее время
            if (timeStart == -1) timeStart = Time.time;
        }
    }

    //немедленно перемещает в новую позицию
    public Vector3 posImmediate        //a
    {
        set
        {
            transform.position = value;
        }
    }

    //код, реализующий анимационный эффект 
    void Update()
    {
        if (timeStart == -1) return;

        //стандартная линейная интерполяция
        float u = (Time.time - timeStart) / timeDuration;
        u = Mathf.Clamp01(u);
        float u1 = Easing.Ease(u, easingCuve);
        Vector3 v = Utils.Bezier(u1, pts);
        transform.position = v;

        //если интерполяция закончена, записать -1 в timeStart
        if (u == 1) timeStart = -1;
    }
}
