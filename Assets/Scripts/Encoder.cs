using System.Collections.Generic;
using System.Linq;
using System.Text;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Encoder
{
    private TilemapBuffer tilemapBuffer;
    private Dictionary<Tile, int> tileTypes;
    private List<Vector2Int> observationCoords;

    public Encoder(TilemapBuffer tilemapBuffer, Dictionary<Tile, int> tileTypes, List<Vector2Int> observationCoords)
    {
        this.tilemapBuffer = tilemapBuffer;
        this.tileTypes = tileTypes;
        this.observationCoords = observationCoords; 
    }

    public float[] Encode(int channel)
    {
        int types_n = (tileTypes.Values.Distinct().Count() + 1);

        float[] onehot = new float[observationCoords.Count * types_n];
        int c = 0;
        foreach (float tile in tilemapBuffer.GetChannel(channel))
        {
            for (int i = 0; i < types_n; i++)
                if (i == tile) onehot[c * types_n + i] = 1; else onehot[c * types_n + i] = 0;
            c++;
        }

        //StringBuilder sb = new StringBuilder();
        //for (int j = 0; j < observationCoords.Count; j++)
        //{
        //    sb.Append("(");
        //    for (int i = 0; i < types_n; i++)
        //    {
        //        sb.Append($"{onehot[j * types_n + i]} ");
        //    }
        //    sb.AppendLine(")");
        //}
        //Debug.Log(sb.ToString());

        return onehot;
    }



}
