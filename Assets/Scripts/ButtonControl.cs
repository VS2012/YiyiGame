using UnityEngine;
using System.Collections;

public class ButtonControl : MonoBehaviour
{
    public int indexX;
    public int indexY;
    private bool clicked = false;

    // Use this for initialization
    void Start()
    {
        StartCoroutine(MoveBack());
    }

    // Update is called once per frame
    void Update()
    {

    }

    private IEnumerator MoveBack()
    {
        yield return new WaitForSeconds(0.5f);
        TweenPosition.Begin(gameObject, 0.01f, new Vector3(indexX * GameControl.textureSize - GameControl.mapOffset, indexY * GameControl.textureSize - GameControl.mapOffset, 0));
    }

    public void SetPos(int x, int y)
    {
        indexX = x;
        indexY = y;
        //transform.position = new Vector3(indexX * GameControl.textureSize - GameControl.mapOffset, indexY * GameControl.textureSize - GameControl.mapOffset, 0);
    }

    void OnPress(bool pressed)
    {
        Debug.Log("OnPress " + pressed + " " + indexX + " " + indexY);
    }

    private void OnClick()
    {
        if (GameControl.annimating)
            return;
        Debug.Log("clicked " + indexX + " " + indexY);
        GameControl.instance.OnTextureClick(indexX, indexY);
        StopCoroutine(ClickAnnim());
        StartCoroutine(ClickAnnim());
        //GetComponent<TweenScale>().Play(true);

    }

    private IEnumerator ClickAnnim()
    {
        if (!clicked)
        {
            TweenScale.Begin(gameObject, 0.05f, new Vector3(0.8f, 0.8f, 0.8f) * GameControl.textureSize);
            yield return new WaitForSeconds(0.05f);
            TweenScale.Begin(gameObject, 0.05f, new Vector3(0.9f, 0.9f, 0.9f) * GameControl.textureSize);
            yield return new WaitForSeconds(0.05f);
            clicked = true;
        }
        else
        {
            TweenScale.Begin(gameObject, 0.05f, Vector3.one * GameControl.textureSize);
            yield return new WaitForSeconds(0.05f);
            clicked = false;
        }
    }

    private void OnDestroy()
    {
        Debug.Log("OnDestory " + indexX + " " + indexY);
        //TweenScale.Begin(gameObject, 0.5f, Vector3.zero);
    }
}
