using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UnitList : MonoBehaviour
{
    public GameObject myPrefab;
    public GameObject unitList;
    public GameManager gameManager;
    // Start is called before the first frame update

    public void ButtonPress()
    {
	    if (gameManager.isMultiplayer && !gameManager.activePlayer.IsOwner)
	    {
		    return;
	    }
        if (gameManager.activePlayer.isInMenu)
        {
            return;
        }
        GameObject content = unitList.transform.Find("Scroll View/Viewport/Content").gameObject;
        unitList.SetActive(!unitList.activeSelf);
        if (unitList.activeSelf)
        {
            PauseMenu.isPaused = true;
            foreach (Transform child in content.transform)
            {
                Destroy(child.gameObject);
            }
            int i = 0;
            foreach (UnitListData unitData in gameManager.activePlayer.playerUnitsManager.GetUnitListData())
            {
                GameObject newEntry = Instantiate(myPrefab, transform.position + new Vector3(100, 160 + i, 0), Quaternion.identity, content.transform);
                GameObject unitName = newEntry.transform.Find("name").gameObject;
                TMP_Text nameText = unitName.GetComponent<TMP_Text>();
                nameText.text = unitData.unitType;

                GameObject unitCurrectntHp = newEntry.transform.Find("hp").gameObject;
                TMP_Text hpText = unitCurrectntHp.GetComponent<TMP_Text>();
                hpText.text = unitData.health;

                GameObject unitCurrectntAttack = newEntry.transform.Find("attack").gameObject;
                TMP_Text attackText = unitCurrectntAttack.GetComponent<TMP_Text>();
                attackText.text = unitData.attack;

                GameObject button = newEntry.transform.Find("button").gameObject;
                Button buttonEvent = button.GetComponent<Button>();
                buttonEvent.onClick.AddListener(delegate { goToPosition(unitData.unit); });
                i += 50;
            }
        }
        else
        {
            PauseMenu.isPaused = false;
        }
    }
    public void CreateUnitList()
    {
        if (!transform.Find("Cool GameObject made from Code"))
        {

            GameObject EmptyObj = new GameObject("Cool GameObject made from Code");
            EmptyObj.transform.parent = this.gameObject.transform;
            EmptyObj.transform.localScale = new Vector3(1, 1, 1);
            int i = 100;
            foreach (UnitListData unitData in gameManager.activePlayer.playerUnitsManager.GetUnitListData())
            {
                GameObject newEntry = Instantiate(myPrefab, transform.position + new Vector3(100, 160 + i, 0), Quaternion.identity, EmptyObj.transform);

                GameObject unitName = newEntry.transform.Find("name").gameObject;
                TMP_Text nameText = unitName.GetComponent<TMP_Text>();
                nameText.text = unitData.unitType;

                GameObject unitCurrectntHp = newEntry.transform.Find("hp").gameObject;
                TMP_Text hpText = unitCurrectntHp.GetComponent<TMP_Text>();
                hpText.text = unitData.health;

                GameObject unitCurrectntAttack = newEntry.transform.Find("attack").gameObject;
                TMP_Text attackText = unitCurrectntAttack.GetComponent<TMP_Text>();
                attackText.text = unitData.attack;

                GameObject  button = newEntry.transform.Find("button").gameObject;
                Button buttonEvent = button.GetComponent<Button>();
                buttonEvent.onClick.AddListener(delegate { goToPosition(unitData.unit); });
                i += 90;
            }
        }
        else
        {
            GameObject abc = transform.Find("Cool GameObject made from Code").gameObject;
            Destroy(abc);
        }
    }
    public void goToPosition(UnitController unit)
    {
        Vector3 tilePosition= gameManager.mapManager.MapEntity.WorldPosition(unit.unitMove.hexPosition);
        Camera.main.transform.position = new Vector3(tilePosition.x, tilePosition.y, Camera.main.transform.position.z);
        PauseMenu.isPaused = false;
        gameManager.activePlayer.SelectUnitFromList(unit);
        unitList.SetActive(false);
    }
}
