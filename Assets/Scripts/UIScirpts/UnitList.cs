using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UnitList : MonoBehaviour
{
    public GameObject myPrefab;
    public GameManager gameManager;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void CreateUnitList()
    {
        if (!transform.Find("Cool GameObject made from Code"))
        {

            GameObject EmptyObj = new GameObject("Cool GameObject made from Code");
            EmptyObj.transform.parent = this.gameObject.transform;
            EmptyObj.transform.localScale = new Vector3(1, 1, 1);
            int i = 100;
            foreach (UnitController unitData in gameManager.activePlayer.allyUnits)
            {
                GameObject newEntry = Instantiate(myPrefab, transform.position + new Vector3(80, i, 0), Quaternion.identity, EmptyObj.transform);

                GameObject unitName = newEntry.transform.Find("name").gameObject;
                TMP_Text nameText = unitName.GetComponent<TMP_Text>();
                nameText.text = unitData.unitType.ToString();

                GameObject unitCurrectntHp = newEntry.transform.Find("hp").gameObject;
                TMP_Text hpText = unitCurrectntHp.GetComponent<TMP_Text>();
                hpText.text = unitData.currentHealth.ToString();

                GameObject unitCurrectntAttack = newEntry.transform.Find("attack").gameObject;
                TMP_Text attackText = unitCurrectntAttack.GetComponent<TMP_Text>();
                attackText.text = unitData.attack.ToString();

                GameObject  button = newEntry.transform.Find("button").gameObject;
                Button buttonEvent = button.GetComponent<Button>();
                buttonEvent.onClick.AddListener(delegate { goToPosition(unitData); });
                i += 40;
            }
        }
        else
        {
            GameObject abc = transform.Find("Cool GameObject made from Code").gameObject;
            Destroy(abc);
        }
    }
    public void goToPosition(UnitController unitData)
    {
        Vector3 tilePosition=unitData.mapManager.MapEntity.WorldPosition(unitData.unitMove.hexPosition);

        Camera.main.transform.position = new Vector3(tilePosition.x, tilePosition.y, Camera.main.transform.position.z);

    }
}
