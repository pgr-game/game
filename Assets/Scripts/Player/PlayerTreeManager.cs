using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerTreeManager : MonoBehaviour
{
    // Start is called before the first frame update
    private bool panelActive = false;
    public void togleEvolutionTree()
    {
        panelActive = !panelActive;
        this.gameObject.SetActive(panelActive);
    }
}
