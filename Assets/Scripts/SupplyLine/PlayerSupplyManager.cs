using RedBjorn.ProtoTiles;
using RedBjorn.ProtoTiles.Example;
using RedBjorn.Utils;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerSupplyManager
{
    public PlayerManager playerManager;
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

    public void Init(PlayerManager playerManager, SupplyLoadData loadData)
    {
        this.playerManager = playerManager;
        this.gameManager = playerManager.gameManager;
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
            supplyLines = loadData.supplyLines;
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

    public void OpenSupplyLineDrawer(City originCity)
    {
        this.originCity = originCity;
        drawingStartPosition = originCity.cityTiles.First().transform.position;

        passableArea.Show(playerManager.mapManager.MapEntity.WalkableBorder((Vector3)drawingStartPosition, float.MaxValue), playerManager.mapManager.MapEntity);

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
    private bool IfSupplyLineVaiable(List<TileEntity> path)
    {
        //its one anyway so ahrdcode it // i tak wina miko�aja �e rysowanie nie dziala
        //1tile size = 1
        float distance = 3f;//this.gameManager.mapManager.MapEntity.Distance(path.ElementAt(0), path.ElementAt(1));
        foreach (TileEntity tile in path)
        {

            //float distance = this.gameManager.mapManager.MapEntity.Distance(previousTile.Position, tile.Position);

            var surrourndingTiles = this.gameManager.mapManager.MapEntity.WalkableTiles(tile.Position, distance); //could work later for the surrouding tiles to get buff as well
            foreach (TileEntity tileSurr in surrourndingTiles)
            {
                bool enemyUnitPresent = tileSurr.UnitPresent != null && tileSurr.UnitPresent.owner != this.playerManager;
                bool enemyCityPresent = tileSurr.CityTilePresent != null && tileSurr.CityTilePresent.owner != this.playerManager;
                bool enemyForkPresent = tileSurr.FortPresent != null && tileSurr.FortPresent.owner != this.playerManager;
                if (enemyUnitPresent || enemyCityPresent|| enemyForkPresent)
                {
                    return false;
                }
            }

        }
        return true;
    }

    public void CreateSupplyLine(Vector3? startPosition, Vector3 endPosition)
    {
        if (startPosition == null)
        {
            startPosition = drawingStartPosition;
        }
        List<TileEntity> path = playerManager.mapManager.MapEntity.PathTiles((Vector3)startPosition, endPosition, float.MaxValue);
        if (path.Count == 0)
        {
            return;
        }
        if (!IfSupplyLineVaiable(path))
        {
            return;
        }
        DestroyOldCitySupplyLine();

        PathDrawer newSupplyLineDrawer = GameObject.Instantiate(playerManager.pathPrefab, Vector3.zero, Quaternion.identity);
        newSupplyLineDrawer.Init(supplyActiveColor, supplyInactiveColor, 0);
        newSupplyLineDrawer.ActiveState();
        newSupplyLineDrawer.Hide();
        SupplyLineController newSupplyLine = new SupplyLineController();
        newSupplyLine.Init(this, originCity, path, newSupplyLineDrawer);
        supplyLines.Add(newSupplyLine);
        ClearSupplyLineCreator();
    }

    public void ClearSupplyLineCreator()
    {
        drawingSupplyLine = false;
        justActivated = true;
        originCity = null;
        drawingStartPosition = null;
        passableArea.Hide();
        supplyLineDrawer.Hide();
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
}
