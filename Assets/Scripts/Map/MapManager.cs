using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RedBjorn.ProtoTiles;
    
public class MapManager : MonoBehaviour
{
    public MapSettings Map;
    public KeyCode GridToggle = KeyCode.G;
    public MapView MapView;

    public MapEntity MapEntity { get; private set; }

    public void Init()
    {
        if (!MapView)
        {
            MapView = GameObject.FindObjectOfType<MapView>();
        }
        MapEntity = new MapEntity(Map, MapView);
        if (MapView)
        {
            MapView.Init(MapEntity);
        }
        else
        {
            Debug.Log("Can't find MapView. Random errors can occur");
        }
        Debug.Log("Map manager initiated");
    }


    void Update()
    {
        if (Input.GetKeyUp(GridToggle))
        {
            MapEntity.GridToggle();
        }
    }
}
