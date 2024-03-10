using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fort : MonoBehaviour
{
    public PlayerManager owner;
    public Vector3Int hexPosition;
    public int id;

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
}
