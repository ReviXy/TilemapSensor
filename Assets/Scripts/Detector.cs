using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using Unity.MLAgents.Sensors;
using Unity.VisualScripting;

public class Detector
{
    public Tilemap tilemap;
    public List<Vector2Int> observationCoords;
    public GameObject agent;
    public TilemapBuffer tilemapBuffer;
    public List<TilemapSensorComponent.TileType> tileTypes;

    private Dictionary<Tile, int> tileTypesDict;

    public Detector(Tilemap tilemap, GameObject agent, TilemapBuffer tilemapBuffer, List<Vector2Int> observationCoords, List<TilemapSensorComponent.TileType> tileTypes)
    {
        this.tilemap = tilemap;
        this.agent = agent;
        this.tilemapBuffer = tilemapBuffer;
        this.observationCoords = observationCoords;
        this.tileTypes = tileTypes;

        tileTypesDict = new Dictionary<Tile, int>();
        for (int i = 0; i < tileTypes.Count; i++)
        {
            foreach (Tile t in tileTypes[i].tiles)
                tileTypesDict.Add(t, i + 1);
        }
    }

    public void OnSensorUpdate() {
        Vector3Int cellAgentPos = tilemap.WorldToCell(agent.transform.position);

        int c = 0;
        foreach(Vector2Int v in observationCoords)
        {
            Tile tile = tilemap.GetTile<Tile>(new Vector3Int(cellAgentPos.x + v.x, cellAgentPos.y + v.y, 0));
            tilemapBuffer.Write(0, c++, (tile.IsUnityNull() || !tileTypesDict.ContainsKey(tile)) ? 0 : tileTypesDict[tile]);
        }
    }

    public void OnSensorReset() {
        tileTypesDict = new Dictionary<Tile, int>();
        for (int i = 0; i < tileTypes.Count; i++)
        {
            foreach (Tile t in tileTypes[i].tiles)
                tileTypesDict.Add(t, i + 1);
        }
    }

}
