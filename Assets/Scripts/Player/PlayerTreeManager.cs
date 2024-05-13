using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.PackageManager.UI;
using UnityEngine;
using UnityEngine.UI;

public class PlayerTreeManager : MonoBehaviour
{
    // Start is called before the first frame update
    Dictionary<int, List<string>> powerEvolution = new Dictionary<int, List<string>>();
    Dictionary<int, List<string>> strategyEvolution = new Dictionary<int, List<string>>();
    public GameObject rootOfTreeCanvas;
    private List<string> powerNodeNames = new List<string>();
    private List<int> powerNodeLinks = new List<int>();
    private List<string> strategyNodeNames = new List<string>();
    private List<int> strategyNodeLinks = new List<int>();

    private bool panelActive = false;
    private static int powerEvolutionCount = 4;
    private static int startegyEvolutionCount = 3;
    // Start is called before the first frame update
    void Start()
    {
        powerNodeNames.Add("POWER");
        powerNodeNames.Add("UNIT LEVELUP");
        powerNodeNames.Add("CHARIOT");
        powerNodeNames.Add("ELEPHANT");
        powerNodeNames.Add("CATAPULT");
        // creating list with ids which node is linked to 
        powerNodeLinks.Add(0); //from Node 1
        powerNodeLinks.Add(1); //from  Node 2
        powerNodeLinks.Add(1); //from  Node 3
        powerNodeLinks.Add(2); // from  Node 4



        strategyNodeNames.Add("STRATEGY");
        strategyNodeNames.Add("PLACING FORTS");
        strategyNodeNames.Add("DEFENCE BONUS");
        strategyNodeNames.Add("HEALING");
        // creating list with ids which node is linked to 
        strategyNodeLinks.Add(0); //from  Node 1
        strategyNodeLinks.Add(1); //from  Node 2
        strategyNodeLinks.Add(1); //from  Node 3

        //creating the dictionary <Node ID,Node info>
        for (int i = 0; i <= powerEvolutionCount; i++)
        {
            List<string> list = new List<string>();
            list.Add(powerNodeLinks.ToString());//previos node id
            list.Add(powerNodeNames[i]); // node name
            if (i == 0)
                list.Add("true");
            else
                list.Add("false"); // Node state default 0 - not researched
            powerEvolution.Add(i, list);
        }

        for (int i = 0; i <= startegyEvolutionCount; i++)
        {
            List<string> list = new List<string>();
            list.Add(powerNodeLinks.ToString());
            list.Add(powerNodeNames[i]);
            if (i == 0)
                list.Add("true");
            else
                list.Add("false"); // Node state default 0 - not researched
            strategyEvolution.Add(i, list);
        }
    }

    private void updateColorsOfTree(GameObject branchRoot, Dictionary<int, List<string>> nodeDict, string nodeBaseName, int numberOfNodes)
    {
        for (int i = 0; i <= numberOfNodes; i++)
        {
            GameObject tmp = (branchRoot.transform.Find(nodeBaseName + i).gameObject);
            //Debug.Log(nodeDict[i][2]);
            if (bool.Parse(nodeDict[i][2])) // if object is reaserched
            {
                GameObject backGround = tmp.transform.Find("Background").gameObject;
                backGround.GetComponent<Image>().color = new Color(1, 0.92f, 0.016f, 1);
                backGround.transform.Find("Frame").gameObject.GetComponent<Image>().color = new Color(1, 0.92f, 0.016f, 1);
            }
            else
            {
                GameObject backGround = tmp.transform.Find("Background").gameObject;
                backGround.GetComponent<Image>().color = new Color(0.5f, 0.5f, 0.5f, 1);
                backGround.transform.Find("Frame").gameObject.GetComponent<Image>().color = new Color(0.5f, 0.5f, 0.5f, 1);

            }
        }
    }
    public void togleEvolutionTree()
    {
        panelActive = !panelActive;
        this.gameObject.SetActive(panelActive);
        if (strategyEvolution.Count == 0 || powerEvolution.Count == 0)
        { this.Start(); }
        if (panelActive)
        {
            GameObject strategy = rootOfTreeCanvas.transform.Find("Strategy").gameObject;
            updateColorsOfTree(strategy, strategyEvolution, "StrategyEvolution", startegyEvolutionCount);

            GameObject power = rootOfTreeCanvas.transform.Find("Power").gameObject;
            updateColorsOfTree(power, powerEvolution, "PowerEvolution", powerEvolutionCount);

        }
    }

    public void evolvePower(int evolutionID)
    {

    }

    public void evolveStrategy(int evolutionID)
    {

    }
}
