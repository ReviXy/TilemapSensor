using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using Unity.MLAgents.Sensors;
using Unity.VisualScripting;

public class Detector
{
    private Tilemap tm;
    public List<Vector2Int> observationCoords;
    private GameObject agent;

    private TilemapBuffer tilemapBuffer;
    public Dictionary<Tile, int> tileTypes;

    public Detector(Tilemap tm, List<Vector2Int> observationCoords, GameObject agent, TilemapBuffer tilemapBuffer, Dictionary<Tile, int> tileTypes)
    {
        this.tm = tm;
        this.observationCoords = observationCoords;
        this.agent = agent;
        this.tilemapBuffer = tilemapBuffer;
        this.tileTypes = tileTypes;
    }

    public void OnSensorUpdate() {
        Vector3Int cellAgentPos = tm.LocalToCell(tm.WorldToLocal(agent.transform.position));

        int c = 0;
        foreach(Vector2Int v in observationCoords)
        {
            Tile tile = tm.GetTile<Tile>(new Vector3Int(cellAgentPos.x + v.x, cellAgentPos.y + v.y, 0));
            tilemapBuffer.Write(0, c++, tile.IsUnityNull() ? 0 : tileTypes[tile]);
        }
    }

    public void OnSensorReset() { }

}
