using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CityUIController : MonoBehaviour
{
    private TMP_Text nameObject;
    private TMP_Text levelObject;
    private Image PlayerColorImage;
    private Image HPImage;
    private TMP_Text HPText;
    // Start is called before the first frame update
    public void Init()
    {
        this.nameObject = transform.Find("Name").GetComponent<TMP_Text>();
        this.levelObject = transform.Find("Level").GetComponent<TMP_Text>();
        this.PlayerColorImage = transform.Find("Backdrop/Image").GetComponent<Image>();
        this.HPImage = transform.Find("HP/Filler").GetComponent<Image>();
        this.HPText = transform.Find("HP/Text").GetComponent<TMP_Text>();
    }

    public void SetName(string name) {
        nameObject.text = name;
    }

    public void SetLevel(string level) {
        levelObject.text = level;
    }

    public void SetHP(int HP, int MaxHP) {
        HPImage.fillAmount = (float)HP/(float)MaxHP;
        HPText.text = HP+"/"+MaxHP+" HP";
    }

    public void SetColor(Color32 color) {
        PlayerColorImage.color = color;
    }
}
