using UnityEngine;
using UnityEngine.Diagnostics;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using System;
using Unity.VisualScripting;

public class Maze : MonoBehaviour
{
    // Channel numbers
    public const int Map = 0;
    public const int Visit = 1;

    public Tilemap tm;

    public Tile wall;
    public Tile food;

    public int width;
    public int height;

    private Vector2Int spawnPos = default;
    private List<Vector2Int> foodPos;

    private float foodPropability = 0.04f;

    bool RandomBool(float propability = 0.5f)
    {
        return UnityEngine.Random.value <= propability;
    }

    public Vector2Int GenerateMaze()
    {
        tm.ClearAllTiles();

        for (int x = 0; x <= width; x++)
        {
            for (int y = 0; y <= height; y++)
            {
                if (x == 0 || y == 0 || x == width || y == width)
                    tm.SetTile(new Vector3Int(x, y, 0), wall);
                else if (x % 2 == 0 && y % 2 == 0)
                {
                    tm.SetTile(new Vector3Int(x, y, 0), wall);
                    int a = RandomBool() ? 0 : (RandomBool() ? -1 : 1);
                    int b = RandomBool() ? 0 : (RandomBool() ? -1 : 1);
                    tm.SetTile(new Vector3Int(x + a, y + b, 0), wall);
                }

            }
        }

        for (int x = 1; x < width; x++)
        {
            for (int y = 1; y < height; y++)
            {
                if (tm.GetTile(new Vector3Int(x, y, 0)).IsUnityNull())
                {
                    if (spawnPos == default)
                    {
                        spawnPos = new Vector2Int(x, y);
                    }
                    else if (RandomBool(foodPropability))
                    {
                        tm.SetTile(new Vector3Int(x, y, 0), food);
                    }
                }
            }
        }
        return spawnPos;
    }

}
