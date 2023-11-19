using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerControl : MonoBehaviour
{
    public Player player;
    private GameObject selected;
    private GameObject newSelected;
    private Ray ray;
    RaycastHit hit;  

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("Player control instantiated!");
    }

    // Update is called once per frame
    void Update()
    {
        newSelected = Select();
        if(newSelected) {
            RedBjorn.ProtoTiles.Example.UnitMove currentUnitMove = newSelected.GetComponent<RedBjorn.ProtoTiles.Example.UnitMove>();
            if(currentUnitMove && newSelected == selected) {
                //unselect
                Debug.Log("Deactivating unit");
                selected = null;
                newSelected = null;
                currentUnitMove.Deactivate();
            }
            else if(currentUnitMove && !selected) {
                //select if nothing else is selected
                Debug.Log("Activating unit");
                selected = newSelected;
                currentUnitMove.Activate();
                HandleSelected();
            }
        }
    }

    GameObject Select()
    {
        if (Input.GetMouseButtonDown(0)) {  
            ray = Camera.main.ScreenPointToRay(Input.mousePosition);  
            if (Physics.Raycast(ray, out hit)) {  
                Debug.Log("Selected: " + hit.transform.name);
                return hit.transform.gameObject;
            }  
        }  
        return null;
    }

    void HandleSelected()
    {
        Debug.Log("Handling selected");
    }
}
