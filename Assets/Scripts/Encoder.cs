using System.Collections.Generic;
using System.Linq;
using System.Text;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Encoder
{
    public TilemapBuffer tilemapBuffer;
    public List<Vector2Int> observationCoords;
    public List<TilemapSensorComponent.TileType> tileTypes;

    public float[] result;

    public Encoder(TilemapBuffer tilemapBuffer, List<Vector2Int> observationCoords, List<TilemapSensorComponent.TileType> tileTypes)
    {
        this.tilemapBuffer = tilemapBuffer;
        this.observationCoords = observationCoords;
        this.tileTypes = tileTypes;

        result = new float[(tileTypes.Count + tilemapBuffer.NumChannels) * observationCoords.Count];
    }

    public float[] Encode()
    {
        int c = 0;

        foreach (float tile in tilemapBuffer.GetChannel(0))
            for (int i = 0; i < tileTypes.Count + 1; i++)
                result[c++] = (i == tile) ? 1 : 0;

        foreach (float data in tilemapBuffer.GetChannel(1))
            result[c++] = data;

        return result;
    }



}
