using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UnitIconController : MonoBehaviour
{
    public string name;
    public int count = 0;

    public Image image;
    public TMP_Text text;
    public void IncrementCount()
    {
        count++;
        text.text = count.ToString();
    }

    public void DecrementCount()
    {
        count--;
        text.text = count.ToString();
    }

    public void SetImage(Sprite sprite)
    {
        image.sprite = sprite;
    }
}
