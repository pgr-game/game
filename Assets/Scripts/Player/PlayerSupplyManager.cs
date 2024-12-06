using RedBjorn.ProtoTiles;
using RedBjorn.ProtoTiles.Example;
using RedBjorn.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class PlayerSupplyManager : NetworkBehaviour
{
    public PlayerManager playerManager;
    public GameObject hexHighlitPrefab;
    public GameManager gameManager;
    private List<SupplyLineController> supplyLines;

    // Creating supply lines
    public bool drawingSupplyLine = false;
    public bool justActivated = true;
    private City originCity;
    private Vector3? drawingStartPosition;
    private AreaOutline passableArea;
    private PathDrawer supplyLineDrawer;
    private Color supplyActiveColor;
    private Color supplyInactiveColor;
    private const int maxSupplyRange = 1000;

    public void Init(PlayerManager playerManager, List<SupplyLoadData> loadData, GameObject hexHighlitPrefab)
    {
        this.playerManager = playerManager;
        this.gameManager = playerManager.gameManager;
        this.hexHighlitPrefab = hexHighlitPrefab;
        supplyLines = new List<SupplyLineController>();
        passableArea = GameObject.Instantiate(playerManager.passableAreaPrefab, Vector3.zero, Quaternion.identity);
        passableArea.Hide();
        supplyLineDrawer = GameObject.Instantiate(playerManager.pathPrefab, Vector3.zero, Quaternion.identity);
        supplyLineDrawer.Hide();

        supplyActiveColor = playerManager.color;
        supplyActiveColor.a = supplyLineDrawer.ActiveColor.a;
        supplyInactiveColor = playerManager.color;
        supplyInactiveColor.a = supplyLineDrawer.InactiveColor.a;

        if (loadData != null)
        {
            foreach (var supplyLoadData in loadData)
            {
                drawingStartPosition = supplyLoadData.startPosition;
                CreateSupplyLineToPosition(supplyLoadData.endPosition);
            }
        }
    }

    public void CheckSupplyLines()
    {
        bool ifEnemyOnSupplyLine = supplyLines.Count > 0 && supplyLines.First().EnemyOnSupplyLine();
        if (ifEnemyOnSupplyLine)
        {
            DestroyOldCitySupplyLine();
        }

    }

    private HashSet<Vector3Int> GetAvailableSupplyTiles(Vector3Int origin)
    {
        var surroundingTiles = gameManager.mapManager.MapEntity.WalkableTiles(origin, maxSupplyRange);

        return surroundingTiles
            .Where((tile) => IsTileViableForSupply(tile))
            .Select((tile) => tile.Position)
            .ToHashSet();
    }

    private bool IsTileViableForSupply(TileEntity tile)
    {
        bool citiesBlocking = tile.CitiesBlockingSupply.Count == 0 ? false : true;
        bool fortsBlocking = tile.FortsBlockingSupply.Count == 0 ? false : true;
        if (!citiesBlocking && !fortsBlocking)
        {
            // short-circuit the calculation for most tiles
            return true;
        }
        if (citiesBlocking)
        {
            foreach (var city in tile.CitiesBlockingSupply)
            {
                if (city.Owner == playerManager)
                {
                    return true;
                }
            }
        }
        if (fortsBlocking)
        {
            foreach (var fort in tile.FortsBlockingSupply)
            {
                if (fort.owner == playerManager)
                {
                    return true;
                }
            }
        }

        if (!citiesBlocking && !fortsBlocking)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public void OpenSupplyLineDrawer(City originCity)
    {
        this.originCity = originCity;
        drawingStartPosition = originCity.cityTiles.First().transform.position;

        var availableSupplyTiles = GetAvailableSupplyTiles(originCity.cityTiles.First().tile.Position);
        var availableSupplyBorder = playerManager.mapManager.MapEntity.WalkableBorder((Vector3)drawingStartPosition, maxSupplyRange, availableSupplyTiles);

        passableArea.Show(availableSupplyBorder, playerManager.mapManager.MapEntity);

        supplyLineDrawer.Show(new List<Vector3>() { }, playerManager.mapManager.MapEntity);
        supplyLineDrawer.Init(supplyActiveColor, supplyInactiveColor, 0);
        supplyLineDrawer.ActiveState();
        supplyLineDrawer.IsEnabled = true;
        drawingSupplyLine = true;
    }

    public void UpdateSupplyLineDrawer()
    {
        var tile = playerManager.mapManager.MapEntity.Tile(MyInput.GroundPosition(playerManager.mapManager.MapEntity.Settings.Plane()));
        if (tile != null && tile.Vacant)
        {
            var path = playerManager.mapManager.MapEntity.PathPoints((Vector3)drawingStartPosition, playerManager.mapManager.MapEntity.WorldPosition(tile.Position), float.MaxValue);

            supplyLineDrawer.Show(path, playerManager.mapManager.MapEntity);
            supplyLineDrawer.ActiveState();
        }
        else
        {
            supplyLineDrawer.InactiveState();
        }
    }
    private bool IsSupplyLineViable(List<TileEntity> path)
    {
        foreach (TileEntity tile in path)
        {
            if(!IsTileViableForSupply(tile))
            {
                return false;
            }
        }
        return true;
    }

	public void CreateSupplyLineToPosition(Vector3 endPosition)
	{
		var startPositionNonNullable = (Vector3)drawingStartPosition;
		if (gameManager.isMultiplayer)
		{
			CreateSupplyLineRpc(startPositionNonNullable, endPosition);
		}
		else
		{
			CreateSupplyLine(startPositionNonNullable, endPosition);
		}
	}

	[Rpc(SendTo.Everyone)]
    public void CreateSupplyLineRpc(Vector3 startPosition, Vector3 endPosition)
    {
	    CreateSupplyLine(startPosition, endPosition);
    }

    public void CreateSupplyLine(Vector3 startPosition, Vector3 endPosition)
    {
        List<TileEntity> path = playerManager.mapManager.MapEntity.PathTiles(startPosition, endPosition, float.MaxValue);
        if (path.Count == 0)
        {
            return;
        }
        if (!IsSupplyLineViable(path))
        {
            return;
        }
        DestroyOldCitySupplyLine();

        PathDrawer newSupplyLineDrawer = GameObject.Instantiate(playerManager.pathPrefab, Vector3.zero, Quaternion.identity);
        newSupplyLineDrawer.Init(supplyActiveColor, supplyInactiveColor, 0);
        newSupplyLineDrawer.ActiveState();
        newSupplyLineDrawer.Hide();
        SupplyLineController newSupplyLine = new SupplyLineController();
        newSupplyLine.Init(this, originCity, path, newSupplyLineDrawer,hexHighlitPrefab);
        supplyLines.Add(newSupplyLine);
        ClearSupplyLineCreator();
    }

    public void ClearSupplyLineCreator()
    {
        drawingSupplyLine = false;
        justActivated = true;
        originCity = null;
        drawingStartPosition = null;
        passableArea?.Hide();
        supplyLineDrawer?.Hide();
    }

    private void DestroyOldCitySupplyLine()
    {
        SupplyLineController overwrittenSupplyLine = supplyLines.Find(supplyLine => supplyLine.originCity == originCity);
        if (overwrittenSupplyLine == null && supplyLines.Count() > 0)
        {
            overwrittenSupplyLine = supplyLines.First();
        }
        if (overwrittenSupplyLine != null)
        {
            supplyLines.Remove(overwrittenSupplyLine);
            overwrittenSupplyLine.Destroy();
        }
    }

    public SupplyLoadData[] GetSupplyLoadData()
    {
        SupplyLoadData[] supplyLoadData = new SupplyLoadData[supplyLines.Count];
        int i = 0;
        foreach (var supplyLine in supplyLines)
        {
            supplyLoadData[i] = supplyLine.GetSupplyLoadData();
            i++;
        }

        return supplyLoadData;
    }

    public int GetSupplyLineCount()
    {
        return supplyLines.Count;
    }
}
