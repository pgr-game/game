using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public record SupplyLoadData
{
    public Vector3 startPosition;
    public Vector3 endPosition;

    public SupplyLoadData(Vector3 startPosition, Vector3 endPosition)
    {
        this.startPosition = startPosition;
        this.endPosition = endPosition;
    }
}
