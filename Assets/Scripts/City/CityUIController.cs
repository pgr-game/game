using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CityUIController : MonoBehaviour
{
    public TMP_Text nameObject;
    public TMP_Text levelObject;
    public Image PlayerColorImage;
    public Image HPImage;
    public TMP_Text HPText;
    public TMP_Text NoGarrisonText;
    public Image unitInProductionImage;
    public TMP_Text unitInProductionTurnsText;
    public HorizontalLayoutGroup garrisonUnitIconsContainer;

    public List<UnitIconController> garrisonUnitIcons;
    public GameObject unitIconPrefab;
    // Start is called before the first frame update
    public void Init()
    {
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

    public void SetUnitInProduction(Sprite sprite) {
        SetUnitInProductionImage(sprite);
    }

    public void AddGarrisonedUnitIcon(Sprite icon, UnitTypes unitType)
    {
        UnitIconController unitIcon = garrisonUnitIcons.Find(u => u.name == unitType.ToString());
        if (unitIcon)
        {
            unitIcon.IncrementCount();
        }
        else
        {
            GameObject newIconObject = Instantiate(unitIconPrefab, garrisonUnitIconsContainer.transform.position + new Vector3(0, 0, 0), Quaternion.identity, garrisonUnitIconsContainer.transform);
            UnitIconController newIcon = newIconObject.GetComponent<UnitIconController>();
            newIcon.IncrementCount();
            newIcon.SetImage(icon);
            newIcon.name = unitType.ToString();
            garrisonUnitIcons.Add(newIcon);
        }
    }

    public void RemoveGarrisonedUnitIcon(UnitTypes unitType)
    {
        UnitIconController unitIcon = garrisonUnitIcons.Find(u => u.name == unitType.ToString());
        if (unitIcon && unitIcon.count > 1)
        {
            unitIcon.DecrementCount();
        }
        else
        {
            Destroy(unitIcon.gameObject);
            garrisonUnitIcons.Remove(unitIcon);
        }
    }
}
