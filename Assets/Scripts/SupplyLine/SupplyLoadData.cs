using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public record SupplyLoadData
{
    public List<SupplyLineController> supplyLines;

    public SupplyLoadData(List<SupplyLineController> supplyLines)
    {
        this.supplyLines = supplyLines;
    }
}
