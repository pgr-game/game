using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class UIMobileModifier : MonoBehaviour
{
    public GameObject SettingsButton;
    public GameObject EvolutionTreeButton;
    void Start()
    {
        if (!Application.isMobilePlatform)
        {
            SettingsButton.SetActive(false);
            EvolutionTreeButton.transform.position =  new Vector3(EvolutionTreeButton.transform.position.x, EvolutionTreeButton.transform.position.y + 100, EvolutionTreeButton.transform.position.z);
        }
    }
}
