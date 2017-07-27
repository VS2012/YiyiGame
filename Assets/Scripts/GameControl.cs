using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class GameControl : MonoBehaviour
{
    public static int mapSize = 7;
    public static int textureNum = 6;
    public static int textureSize = 100;
    public static int mapOffset = textureSize * mapSize / 2 - textureSize / 2;
    private static float anniTime = 0.4f;
    public static bool annimating = true;

    private static int[,] map = new int[mapSize, mapSize];
    private Struct2D selectFirst = new Struct2D();
    private Struct2D selectSecond = new Struct2D();
    private Struct2D pressedFirst = new Struct2D();
    private Struct2D pressedSecond = new Struct2D();
    private int clickCount;

    //private static List<int[,]> tobeErased = new List<int[,]>();
    private static List<Struct2D> tobeErased = new List<Struct2D>();
    private Dictionary<Struct2D, GameObject> objectMap = new Dictionary<Struct2D, GameObject>();

    public GameObject myTexture;
    //public Material[] materials = new Material[textureNum];
    public Texture[] textures = new Texture[textureNum];
    public static GameControl instance;

    private void Awake()
    {
        instance = this;
    }

    void Start()
    {
        StartCoroutine(StartGame());
    }

    void Update()
    {

    }

    private IEnumerator StartGame()
    {
        for (var i = 0; i < mapSize; i++)
        {
            for (var j = 0; j < mapSize; j++)
            {
                map[i, j] = -1;
            }
        }
        yield return StartCoroutine(FillMap());
    }

    private void Go()
    {
        checkErase(0, 0, mapSize - 1, mapSize - 1);
        if (tobeErased.Count > 0)
        {
            DoErase();
            FallDown();
            StartCoroutine(FillMap());
        }
    }

    //填充地图
    private IEnumerator FillMap()
    {
        Debug.Log("FillMap");
        annimating = true;
        var depth = -1;
        var random = new System.Random();
        for (var i = 0; i < mapSize; i++)
        {
            for (var j = 0; j < mapSize; j++)
            {
                if (map[i, j] == -1)
                {
                    var rand = random.Next() % textureNum;
                    map[i, j] = rand;
                    var newTexture = (GameObject)Instantiate(myTexture);
                    newTexture.gameObject.transform.parent = transform;
                    newTexture.GetComponent<ButtonControl>().SetPos(i, j);
                    newTexture.GetComponent<UITexture>().material.mainTexture =  textures[rand];
                    objectMap.Add(new Struct2D(i, j), newTexture);
                    TweenScale.Begin(newTexture, anniTime, new Vector3(textureSize, textureSize, 0));
                    TweenPosition.Begin(newTexture, anniTime, new Vector3(i * textureSize - mapOffset, j * textureSize - mapOffset, depth));
                    depth--;
                }
            }
        }
        yield return new WaitForSeconds(anniTime);
        annimating = false;
        checkErase(0, 0, mapSize - 1, mapSize - 1);
    }

    private static void printMap()
    {
        for (var i = 0; i < mapSize; i++)
        {
            for (var j = 0; j < mapSize; j++)
                Console.Write(map[i, j] + " ");
            Console.Write('\n');
        }
    }

    public void OnTexturePress(bool pressed, int x, int y)
    {
        if(pressed)
        {
            pressedFirst.x = x;
            pressedFirst.y = y;
        }
        else
        {
            pressedSecond.x = x;
            pressedSecond.y = y;
            if (pressedFirst == pressedSecond)
                return;
            selectFirst = pressedFirst;
            selectSecond = pressedSecond;
            StartCoroutine(CheckGame());
        }
    }

    public void OnTextureClick(int x, int y)
    {
        if (clickCount == 0)
        {
            clickCount++;
            selectFirst.x = x;
            selectFirst.y = y;
        }
        else if (clickCount == 1)
        {
            selectSecond.x = x;
            selectSecond.y = y;
            clickCount = 0;

            if (selectFirst == selectSecond)
                return;
            StartCoroutine(CheckGame());
        }
    }


    private IEnumerator CheckGame()
    {
        yield return StartCoroutine(Swap());
        checkErase(0, 0, mapSize - 1, mapSize - 1, false);

        if (tobeErased.Count > 0)
        {
            yield return StartCoroutine(DoErase());
        }
        else
        {
            yield return StartCoroutine(Swap());
        }
    }

    private IEnumerator Swap()
    {
        annimating = true;
        //交换数据
        var tmp = map[selectFirst.x, selectFirst.y];
        map[selectFirst.x, selectFirst.y] = map[selectSecond.x, selectSecond.y];
        map[selectSecond.x, selectSecond.y] = tmp;

        //交换位置

        var firstObj = objectMap[selectFirst];
        var secondObj = objectMap[selectSecond];
        objectMap.Remove(selectFirst);
        objectMap.Remove(selectSecond);

        var pos1 = new Vector3(selectSecond.x * textureSize - mapOffset, selectSecond.y * textureSize - mapOffset, 0);
        var pos2 = new Vector3(selectFirst.x * textureSize - mapOffset, selectFirst.y * textureSize - mapOffset, 0);
        var time = (int)(Vector3.Distance(pos1, pos2) / textureSize) * 0.15f;
        if (time > 0.5)
            time = 0.5f;
        TweenPosition.Begin(firstObj, time, pos1);
        TweenPosition.Begin(secondObj, time, pos2);
        yield return new WaitForSeconds(time);
        TweenScale.Begin(firstObj, 0.1f, Vector3.one * textureSize);
        TweenScale.Begin(secondObj, 0.1f, Vector3.one * textureSize);
        yield return new WaitForSeconds(0.1f);
        annimating = false;

        firstObj.GetComponent<ButtonControl>().SetPos(selectSecond.x, selectSecond.y);
        secondObj.GetComponent<ButtonControl>().SetPos(selectFirst.x, selectFirst.y);

        objectMap.Add(selectSecond, firstObj);
        objectMap.Add(selectFirst, secondObj);
    }

    //检查消除
    private void checkErase(int startX, int startY, int endX, int endY, bool autoErase = true)
    {
        //竖向检查
        for (var x = startX; x <= endX; x++)
        {
            for (var y = startY; y <= endY - 2; y++)
            {
                if (map[x, y] == map[x, y + 1] && map[x, y + 1] == map[x, y + 2]
                    && map[x, y] != -1)
                {
                    tobeErased.Add(new Struct2D(x, y));
                    tobeErased.Add(new Struct2D(x, y + 1));
                    tobeErased.Add(new Struct2D(x, y + 2));
                }
            }
        }

        //横向检查
        for (var y = startY; y <= endY; y++)
        {
            for (var x = startX; x <= endX - 2; x++)
            {
                if (map[x, y] == map[x + 1, y] && map[x + 1, y] == map[x + 2, y]
                    && map[x, y] != -1)
                {
                    tobeErased.Add(new Struct2D(x, y));
                    tobeErased.Add(new Struct2D(x + 1, y));
                    tobeErased.Add(new Struct2D(x + 2, y));
                }
            }
        }

        if (tobeErased.Count > 0 && autoErase)
            StartCoroutine(DoErase());
    }

    private IEnumerator DoErase()
    {
        Debug.Log("DoErase " + tobeErased.Count);

        annimating = true;
        GameObject obj;
        foreach (var v in tobeErased)
        {
            obj = null;
            objectMap.TryGetValue(v, out obj);
            if(obj != null)
            {
                TweenScale.Begin(obj, anniTime, Vector3.zero);
                TweenRotation.Begin(obj, anniTime, new Quaternion(0, 0, 360, 0));
            }
        }
        yield return new WaitForSeconds(anniTime);
        annimating = false;
        
        foreach ( var v in tobeErased)
        {
            obj = null;
            objectMap.TryGetValue(v, out obj);
            if(obj != null) // 往 tobeErased 中添加元素时没有进行去重判断
            {
                Debug.Log("Erase " + v.x + ", " + v.y);
                objectMap.Remove(v);
                Destroy(obj);
                map[v.x, v.y] = -1;
            }
        }
        tobeErased.Clear();

        yield return new WaitForFixedUpdate();
        StartCoroutine(FallDown());
    }

    private IEnumerator FallDown()
    {
        Debug.Log("FallDown");
        annimating = true;
        for (var i = 0; i < mapSize; i++)
        {
            for (var j = 0; j < mapSize; j++)
            {
                if (map[i, j] == -1) // j 代表 -1 的下标
                {
                    for (var y = j; y < mapSize; y++)
                    {
                        if (map[i, y] != -1) // y 代表不是 -1 的下标
                        {
                            map[i, j] = map[i, y];
                            map[i, y] = -1;
                            var obj = objectMap[new Struct2D(i, y)];
                            obj.GetComponent<ButtonControl>().SetPos(i, j);
                            objectMap.Remove(new Struct2D(i, y));
                            objectMap.Add(new Struct2D(i, j), obj);
                            TweenPosition.Begin(obj, anniTime, new Vector3(i * textureSize - mapOffset, j * textureSize - mapOffset, 0));
                            break;
                        }
                    }
                }
            }
        }
        yield return new WaitForSeconds(anniTime);
        yield return new WaitForFixedUpdate();
        annimating = false;
        StartCoroutine(FillMap());
    }
}

public struct Struct2D
{
    public int x;
    public int y;

    public Struct2D(int i, int j)
    {
        x = i;
        y = j;
    }
    
    public bool Equals(Struct2D obj)
    {
        return x == obj.x && y == obj.y;
    }

    public static bool operator == (Struct2D first, Struct2D second)
    {
        return first.x == second.x && first.y == second.y;
    }

    public static bool operator !=(Struct2D first, Struct2D second)
    {
        return first.x != second.x || first.y != second.y;
    }
}

