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
    private TMP_Text NoGarrisonText;
    private Image unitInProductionImage;
    private TMP_Text unitInProductionTurnsText;
    // Start is called before the first frame update
    public void Init()
    {
        this.nameObject = transform.Find("Name").GetComponent<TMP_Text>();
        this.levelObject = transform.Find("Level").GetComponent<TMP_Text>();
        this.PlayerColorImage = transform.Find("Backdrop/Image").GetComponent<Image>();
        this.HPImage = transform.Find("HP/Filler").GetComponent<Image>();
        this.HPText = transform.Find("HP/HealthText").GetComponent<TMP_Text>();
        this.NoGarrisonText = transform.Find("HP/NoGarrisonText").GetComponent<TMP_Text>();
        this.unitInProductionImage = transform.Find("UnitImage").GetComponent<Image>();
        this.unitInProductionTurnsText = transform.Find("TurnsLeft").GetComponent<TMP_Text>();
    }

    public void SetName(string name) {
        nameObject.text = name;
    }

    public void SetLevel(string level) {
        levelObject.text = level;
    }

    public void SetHP(int HP, int MaxHP) {
        if(MaxHP > 0)
        {
            HPText.text = HP + "/" + MaxHP + " HP";
            NoGarrisonText.text = "";
            HPImage.fillAmount = (float)HP / (float)MaxHP;
        }
        else
        {
            HPText.text = "";
            NoGarrisonText.text = "NO GARRISON!";
            HPImage.fillAmount = (float)0 / (float)1;
        }
    }

    public void SetColor(Color32 color) {
        PlayerColorImage.color = color;
    }

    public void SetUnitInProductionImage(Sprite sprite) {
        unitInProductionImage.sprite = sprite;
    }

    public void SetTurnsLeft(int turnsLeft) {
        unitInProductionTurnsText.text = turnsLeft.ToString();
    }

    public void SetUnitInProduction(GameObject unitPrefab) {
        Sprite sprite = unitPrefab.transform.Find("Sprite").GetComponent<SpriteRenderer>().sprite;
        SetUnitInProductionImage(sprite);
    }
}
