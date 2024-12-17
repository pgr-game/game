using RedBjorn.ProtoTiles;
using RedBjorn.ProtoTiles.Example;
using RedBjorn.Utils;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class Fort : MonoBehaviour
{
    public PlayerManager owner;
    public Vector3Int hexPosition;
    public int id;
    public bool isBuilt;
    public int turnsUntilBuilt;
    private City adjacentCity;
    private const int supplyBlockingRange = 3;

    // Photo chosen for the fort
    public int amountOfImages;
    public Sprite[] images;
    public SpriteRenderer sprite;

    public GameObject barFiller;
    public GameObject barText;
    public GameObject wholeBar;

    public AreaOutline Area;
    public AreaOutline AreaPrefab;

    public void UnitShow()
    {
        AreaHide();
        var tile = owner.mapManager.MapEntity.WalkableBorder(transform.position, 0);
        Area.Show(tile, owner.mapManager.MapEntity);
    }

    public void AreaHide()
    {
        Area.Hide();
    }
    // returns 1 if successful, 0 if not
    public int Init(int id, PlayerManager owner, Vector3Int hexPosition,AreaOutline areaOutline)
    {
        this.AreaPrefab = areaOutline;
        this.id = id;
        this.owner = owner;
        this.hexPosition = hexPosition;
        this.isBuilt = false;
        this.turnsUntilBuilt = 5;

        Area = Spawner.Spawn(AreaPrefab, Vector3.zero, Quaternion.identity);
        AreaHide();
        sprite = GetComponentInChildren<SpriteRenderer>();
        if(sprite == null)
        {
            Debug.LogError("Could not find sprite renderer in children of fort object for fort id: " + id);
            return 0;
        }
        int random = Random.Range(0, amountOfImages);
        if(random < images.Length)
        {
            sprite.sprite = images[random];
            sprite.color = new Color(1f, 1f, 1f, 0.5f);

            // add fort to the tile
            var tile = owner.mapManager.MapEntity.Tile(hexPosition);
            tile.FortPresent = this;

            return 1;
        } 
        else 
        {
            Debug.LogError("Image index is out of range for the 'images' array. Please check if all images are assigned.");
            return 0;
        }

    }

    public void BuildComplete()
    {
        isBuilt = true;
        sprite.color = new Color(1f, 1f, 1f, 1f);
        var adjacentTiles = owner.mapManager.MapEntity.WalkableTiles(hexPosition, 1.0f);
        foreach (var tile in adjacentTiles)
        {
            if(tile.CityTilePresent)
            {
                adjacentCity = tile.CityTilePresent.city;
                break;
            }
        }

        if(adjacentCity != null)
        {
            adjacentCity.UpdateBesiegedStatus();
        }

        var tilesBlockingSupply = owner.mapManager.MapEntity.WalkableTiles(hexPosition, supplyBlockingRange);
        foreach (var tile in tilesBlockingSupply)
        {
            tile.FortsBlockingSupply.Add(this);
        }
    }

    public void Destroy(TileEntity fortTile)
    {
        var tilesBlockingSupply = owner.mapManager.MapEntity.WalkableTiles(hexPosition, supplyBlockingRange);
        foreach (var tile in tilesBlockingSupply)
        {
            tile.FortsBlockingSupply.Remove(this);
        }

        fortTile.FortPresent = null;

        owner.playerFortsManager.RemoveFort(this, adjacentCity);
    }

    public void DestroyAndRefundFort(TileEntity fortTile)
    {
        if(turnsUntilBuilt > 0)
        {
            this.owner.gold += PlayerManager.costOfFort / turnsUntilBuilt;
        }
        Destroy(fortTile);
    }

    public void ProgressBuild(TileEntity tile)
    {
        this.turnsUntilBuilt--;
        float fillAmm = (5.0f - turnsUntilBuilt) / 5.0f;
        barFiller.GetComponent<Image>().fillAmount = fillAmm;
        barText.GetComponent<Text>().text = ("TURNS LEFT: " + this.turnsUntilBuilt); 
        if (this.turnsUntilBuilt == 0)
        {
            this.BuildComplete();
            Destroy(wholeBar);
        }
    }
}
