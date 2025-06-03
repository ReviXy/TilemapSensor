using UnityEngine;

[CreateAssetMenu(fileName = "Grid Map", menuName = "Grid Map")]
public class GridMap : ScriptableObject
{
    public int row = 1;
    public int column = 1;
    public int[] gridArray = new int[1];
    public bool consider_agent_tile_an_observation = false;

    public int[,] Grid()
    {
        return Grid(row, column, gridArray);
    }

    public static int[,] Grid(int row, int column, int[] array)
    {
        int[,] grid = new int[row, column];

        for (int i = 0; i < row; i++)
        {
            for (int j = 0; j < column; j++)
            {
                grid[i, j] = array[(row - j - 1) * row + i];

            }
        }

        return grid;
    }

}
