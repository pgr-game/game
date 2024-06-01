using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreeLoadData
{
    public Dictionary<int, List<string>> powerEvolution;
    public Dictionary<int, List<string>> strategyEvolution;
    public (int, string) researchNode;

    public TreeLoadData(Dictionary<int, List<string>> powerEvolution, Dictionary<int, List<string>> strategyEvolution, (int, string) researchNode)
    {
        this.powerEvolution = powerEvolution;
        this.strategyEvolution = strategyEvolution;
        this.researchNode = researchNode;
    }
}
