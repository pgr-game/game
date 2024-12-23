using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerTreeManager : MonoBehaviour
{
    // Start is called before the first frame update
    //Dictionary<int, List<string>> powerEvolution = new Dictionary<int, List<string>>();
    //Dictionary<int, List<string>> strategyEvolution = new Dictionary<int, List<string>>();
    public GameObject rootOfTreeCanvas;
    public GameManager gameManager;
    public GameObject ProgressCricle;
    public GameObject ProgressButtonText;
    public GameObject ProgressBurronIcon;
    private Dictionary<string,int> unitIDS= new Dictionary<string,int>();
    private List<string> powerNodeNames = new List<string>();
    private List<int> powerNodeLinks = new List<int>();
    private List<int> powerTurnsToUnlock = new List<int>();

    private List<string> strategyNodeNames = new List<string>();
    private List<int> strategyNodeLinks = new List<int>();
    private List<int> startegyTurnsToUnlock = new List<int>();


    private bool panelActive = false;
    private static int powerEvolutionCount = 4;
    private static int startegyEvolutionCount = 4;

    public void Init(GameManager gameManager)
    {
        this.gameManager = gameManager;

        if (powerNodeNames.Count != 0 || powerNodeLinks.Count != 0
    || strategyNodeNames.Count != 0 || strategyNodeLinks.Count != 0)
        {
            return;
        }
            //ist of names 
            powerNodeNames.Add("POWER");
        powerNodeNames.Add("UNIT LEVELUP");
        powerNodeNames.Add("CHARIOT");
        powerNodeNames.Add("ELEPHANT");
        powerNodeNames.Add("CATAPULT");

        unitIDS.Add("CHARIOT", 2);
        unitIDS.Add("ELEPHANT", 3);
        unitIDS.Add("CATAPULT", 4);
        // creating list with ids which node is linked to 
        powerNodeLinks.Add(0); //root node not used
        powerNodeLinks.Add(0); //from Node 1
        powerNodeLinks.Add(1); //from  Node 2
        powerNodeLinks.Add(1); //from  Node 3
        powerNodeLinks.Add(2); // from  Node 4
        //list of turns needed to reasrch each node
        powerTurnsToUnlock.Add(3);
        powerTurnsToUnlock.Add(3);
        powerTurnsToUnlock.Add(4);
        powerTurnsToUnlock.Add(5);
        powerTurnsToUnlock.Add(5);



        strategyNodeNames.Add("STRATEGY");
        strategyNodeNames.Add("PLACING FORTS");
        strategyNodeNames.Add("DEFENCE BONUS");
        strategyNodeNames.Add("HEALING");
        strategyNodeNames.Add("SUPPLY LINES");
        // creating list with ids which node is linked to 
        strategyNodeLinks.Add(0); //root node; not used
        strategyNodeLinks.Add(0); //from  Node 1
        strategyNodeLinks.Add(1); //from  Node 2
        strategyNodeLinks.Add(1); //from  Node 3
        strategyNodeLinks.Add(3); //from  Node 4
        //list of turns needed to reasrch each node
        startegyTurnsToUnlock.Add(3);
        startegyTurnsToUnlock.Add(4);
        startegyTurnsToUnlock.Add(4);
        startegyTurnsToUnlock.Add(5);
        startegyTurnsToUnlock.Add(3);
    }

    public void populateEvolutionTrees(PlayerManager playerManager)
    {
        //creating the dictionary <Node ID,Node info>
        for (int i = 0; i <= powerEvolutionCount; i++)
        {
            List<string> list = new List<string>();
            list.Add(powerNodeLinks[i].ToString());//previos node id
            list.Add(powerNodeNames[i]); // node name
            list.Add("false"); // Node state default 0 - not researched
            list.Add(powerTurnsToUnlock[i].ToString());//turns to research
            list.Add(powerTurnsToUnlock[i].ToString());
            playerManager.powerEvolution.TryAdd(i, list);
        }

        for (int i = 0; i <= startegyEvolutionCount; i++)
        {
            List<string> list = new List<string>();
            list.Add(strategyNodeLinks[i].ToString());
            list.Add(strategyNodeNames[i]);
            list.Add("false"); // Node state default 0 - not researched
            list.Add(startegyTurnsToUnlock[i].ToString());//turns to research
            list.Add(startegyTurnsToUnlock[i].ToString());
            playerManager.strategyEvolution.TryAdd(i, list);
        }
    }

    private void updateColorsOfTree(GameObject branchRoot, Dictionary<int, List<string>> nodeDict, string nodeBaseName, int numberOfNodes, (int, string) currResearch)
    {
        for (int i = 0; i <= numberOfNodes; i++)
        {
            string researchTimeLeft = nodeDict[i][3] + " TURNS";
            GameObject currEvolveNode = (branchRoot.transform.Find(nodeBaseName + i).gameObject);
            GameObject backGround = currEvolveNode.transform.Find("Background").gameObject;
            Color newColor;
            if (nodeBaseName.Contains(currResearch.Item2) && i == currResearch.Item1)
            {//Node under current research
                newColor = new Color(0.5f, 1, 0.5f, 1);

            }
            else if (bool.Parse(nodeDict[i][2])) // if object is reaserched
            {
                newColor = new Color(1, 1, 0.65f, 1);
                researchTimeLeft = "RESEARCHED";
            }
            else if (bool.Parse(nodeDict[Int32.Parse(nodeDict[i][0])][2]) || (i == 0))//i==0 in special case it is root node
            {//if previous node is reserached and this one is avialable to research
                newColor = new Color(1, 0.92f, 0.016f, 1);
            }
            else
            { //node unavilable to research
                newColor = new Color(0.5f, 0.5f, 0.5f, 1);
            }
            backGround.GetComponent<Image>().color = newColor;// new Color(171.0f/255.0f, 183.0f/255.0f, 183.0f/255.0f,1);
            backGround.transform.Find("Frame").gameObject.GetComponent<Image>().color = newColor;
            backGround.transform.Find("TurnCounter").gameObject.GetComponent<TextMeshProUGUI>().text = researchTimeLeft;

        }
    }

    public void updateStrategyBranch(Dictionary<int, List<string>> strategyEvolution, (int, string) currResearch)
    {
        GameObject strategy = rootOfTreeCanvas.transform.Find("Strategy").gameObject;
        updateColorsOfTree(strategy, strategyEvolution, "StrategyEvolution", startegyEvolutionCount, currResearch);
    }

    public void updatePowerBranch(Dictionary<int, List<string>> powerEvolution, (int, string) currResearch)
    {
        GameObject power = rootOfTreeCanvas.transform.Find("Power").gameObject;
        updateColorsOfTree(power, powerEvolution, "PowerEvolution", powerEvolutionCount, currResearch);
    }
    public void togleEvolutionTree()
    {
        if(gameManager.activePlayer.isInMenu && !panelActive)
        {
            return;
        }
        Dictionary<int, List<string>> powerEvolvCurrPLayer = gameManager.activePlayer.powerEvolution;
        Dictionary<int, List<string>> strategyEvolvCurrPLayer = gameManager.activePlayer.strategyEvolution;
        (int, string) currResearch = gameManager.activePlayer.researchNode;

        panelActive = !panelActive;

        if (gameManager.isMultiplayer)
        {
	        if (!gameManager.activePlayer.IsOwner)
	        {
		        panelActive = false;
	        }
        }

		this.gameObject.SetActive(panelActive);
        if (panelActive)
        {
            updateStrategyBranch(strategyEvolvCurrPLayer, currResearch);
            updatePowerBranch(powerEvolvCurrPLayer, currResearch);

            ProgressButtonText.SetActive(true);
            ProgressBurronIcon.SetActive(false);
            gameManager.activePlayer.isInMenu = true;
        }
        else
        {
            ProgressButtonText.SetActive(false);
            ProgressBurronIcon.SetActive(true);
            gameManager.activePlayer.isInMenu = false;
        }
    }

    public void evolvePower(int evolutionID)
    {
        Dictionary<int, List<string>> powerEvolvCurrPLayer = gameManager.activePlayer.powerEvolution;
        Dictionary<int, List<string>> strategyEvolvCurrPLayer = gameManager.activePlayer.strategyEvolution;


        if ((!bool.Parse(powerEvolvCurrPLayer[evolutionID][2]) && bool.Parse(powerEvolvCurrPLayer[Int32.Parse(powerEvolvCurrPLayer[evolutionID][0])][2])) ||
            evolutionID == 0 && !bool.Parse(powerEvolvCurrPLayer[evolutionID][2]))//in case node is root node
        {
            //not researched selcted node yet && previous Node is reserched
            gameManager.activePlayer.researchNode = (evolutionID, "Power");
            (int, string) currResearch = gameManager.activePlayer.researchNode;
            updatePowerBranch(powerEvolvCurrPLayer, currResearch);
            updateStrategyBranch(strategyEvolvCurrPLayer, currResearch);
            float turnsResearched = float.Parse(powerEvolvCurrPLayer[currResearch.Item1][3]);
            float totalTurnsToResearch = float.Parse(powerEvolvCurrPLayer[currResearch.Item1][4]);
            updateProgressCircle((totalTurnsToResearch - turnsResearched) / totalTurnsToResearch);
        }
    }

    public void evolveStrategy(int evolutionID)
    {
        Dictionary<int, List<string>> powerEvolvCurrPLayer = gameManager.activePlayer.powerEvolution;
        Dictionary<int, List<string>> strategyEvolvCurrPLayer = gameManager.activePlayer.strategyEvolution;

        if ((!bool.Parse(strategyEvolvCurrPLayer[evolutionID][2]) && bool.Parse(strategyEvolvCurrPLayer[Int32.Parse(strategyEvolvCurrPLayer[evolutionID][0])][2]))|| 
            evolutionID == 0 && !bool.Parse(strategyEvolvCurrPLayer[evolutionID][2]))//in case node is root node)
        {
            //not researched selcted node yet && previous Node is reserched
            gameManager.activePlayer.researchNode = (evolutionID, "Strategy");
            (int, string) currResearch = gameManager.activePlayer.researchNode;
            updateStrategyBranch(strategyEvolvCurrPLayer, currResearch);
            updatePowerBranch(powerEvolvCurrPLayer, currResearch);
            float turnsResearched = float.Parse(strategyEvolvCurrPLayer[currResearch.Item1][3]);
            float totalTurnsToResearch = float.Parse(strategyEvolvCurrPLayer[currResearch.Item1][4]);
            updateProgressCircle((totalTurnsToResearch - turnsResearched) / totalTurnsToResearch);
        }
    }

    private void researchHelper(Dictionary<int, List<string>> evolutionBranch, (int, string) currResearch)
    {
        int turnsLeft = Int32.Parse(evolutionBranch[currResearch.Item1][3]);
        turnsLeft--;
        if (turnsLeft <= 0)
        {
			if (gameManager.isMultiplayer)
            {
                gameManager.activePlayer.ResearchTreeRpc(currResearch.Item1, currResearch.Item2);
            }
            else
            {
	            gameManager.activePlayer.ResearchTree(currResearch.Item1, currResearch.Item2);
			}

            updateProgressCircle(1f);
		}
        else
        {
            evolutionBranch[currResearch.Item1][3] = turnsLeft.ToString();
            float turnsResearched = float.Parse(evolutionBranch[currResearch.Item1][3]);
            float totalTurnsToResearch = float.Parse(evolutionBranch[currResearch.Item1][4]);
            updateProgressCircle((totalTurnsToResearch - turnsResearched) / totalTurnsToResearch);
        }
    }

    public void reserachProgress()
    {
        Dictionary<int, List<string>> powerEvolvCurrPLayer = gameManager.activePlayer.powerEvolution;
        Dictionary<int, List<string>> strategyEvolvCurrPLayer = gameManager.activePlayer.strategyEvolution;
        (int, string) currResearch = gameManager.activePlayer.researchNode;

        if (currResearch.Item2.Equals("Strategy"))
        {
            researchHelper(strategyEvolvCurrPLayer, currResearch);
        }
        else if (currResearch.Item2.Equals("Power"))
        {
            researchHelper(powerEvolvCurrPLayer, currResearch);
        }
        else
        {
            updateProgressCircle(0.0f);
        }
    }

    private void updateProgressCircle(float ammount)
    {
        ProgressCricle.gameObject.GetComponent<Image>().fillAmount = ammount;
    }

    public bool isNodeResearched(int nodeID,string branchType)
    {
        if(gameManager.activePlayer == null)
        {
            return false;
        }
        Dictionary<int, List<string>> powerEvolvCurrPLayer = gameManager.activePlayer.powerEvolution;
        Dictionary<int, List<string>> strategyEvolvCurrPLayer = gameManager.activePlayer.strategyEvolution;

        if (branchType.Equals("Strategy"))
        {
            return bool.Parse(strategyEvolvCurrPLayer[nodeID][2]);
        }
        else if (branchType.Equals("Power"))
        {
            return bool.Parse(powerEvolvCurrPLayer[nodeID][2]);
        }
        return false;
    }
    
    public bool isNodeOfPlayerResearched(int nodeID,string branchType, PlayerManager player)
    {
        Dictionary<int, List<string>> powerEvolvCurrPLayer = player.powerEvolution;
        Dictionary<int, List<string>> strategyEvolvCurrPLayer = player.strategyEvolution;

        if (branchType.Equals("Strategy"))
        {
            return bool.Parse(strategyEvolvCurrPLayer[nodeID][2]);
        }
        else if (branchType.Equals("Power"))
        {
            return bool.Parse(powerEvolvCurrPLayer[nodeID][2]);
        }
        return false;
    }

    public bool isUnitUnlocked(string unitName)
    {
        if(unitIDS.ContainsKey(unitName.ToUpper()))
        {
            return isNodeResearched(unitIDS[unitName.ToUpper()], "Power");
        }
        else
        {
            return true;
        }
    }
}
