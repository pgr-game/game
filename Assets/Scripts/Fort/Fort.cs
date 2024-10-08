using RedBjorn.ProtoTiles;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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


    // returns 1 if successful, 0 if not
    public int Init(int id, PlayerManager owner, Vector3Int hexPosition)
    {
        this.id = id;
        this.owner = owner;
        this.hexPosition = hexPosition;
        this.isBuilt = false;
        this.turnsUntilBuilt = 10;

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
}
