using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;

using System.Collections.Generic;
using System.Collections;
using System;
using System.ComponentModel;
using UnityEngine.Tilemaps;
using System.Text;

public class TilemapSensorComponent : SensorComponent
{
    public string SensorName
    {
        get { return m_SensorName; }
        set { m_SensorName = value; }
    }
    [SerializeField]
    [Tooltip("Name of the generated GridSensor.")]
    private string m_SensorName = "TilemapSensor";

    public int ObservationStacks
    {
        get { return m_ObservationStacks; }
        set { m_ObservationStacks = value; }
    }
    [SerializeField, Min(1)]
    [Tooltip("The number of stacked observations. Enable stacking (value > 1) "
            + "if agents need to infer movement from observations.")]
    private int m_ObservationStacks = 1;

    public ObservationType ObservationType
    {
        get { return m_ObservationType; }
        set { m_ObservationType = value; }
    }
    [SerializeField]
    [Tooltip("The observation type of the sensor.")]
    private ObservationType m_ObservationType = ObservationType.Default;

    [SerializeField]
    [Tooltip("Number of observation channels for buffer.")]
    private int m_NumChannels = 1;

    [SerializeField]
    [Tooltip("ScriptableObject that defines observation shape.")]
    private GridMap m_GridMap;
        
    [SerializeField]
    [Tooltip("Tilemap the data will be taken from.")]
    private Tilemap m_Tilemap;

    [SerializeField]
    [Tooltip("Groups of tiles for sensor to detect.")]
    public List<TileType> m_TileTypes = new List<TileType>();

    [System.Serializable]
    public class TileType
    {
        public string name;
        public List<Tile> tiles;

        public TileType(string name, List<Tile> tiles)
        {
            this.name = name;
            this.tiles = tiles;
        }
    }
    
    [HideInInspector]
    public TilemapSensor m_TilemapSensor;

    [HideInInspector]
    public TilemapBuffer m_TilemapBuffer;

    [HideInInspector]
    public List<Vector2Int> m_ObservationCoords = new List<Vector2Int>();

    public bool HasSensor
    {
        get { return m_TilemapSensor != null; }
    }

    public override ISensor[] CreateSensors()
    {
        Vector2Int agentCoord = new Vector2Int(-1, -1);
        m_ObservationCoords = new List<Vector2Int>();
        int[,] grid = m_GridMap.Grid();

        // Process GridMap
        // Find agent tile
        for (int y = 0; y < m_GridMap.row; y++)
            for (int x = 0; x < m_GridMap.column; x++)
                if (grid[x, y] == 2)
                {
                    if (agentCoord.x == -1 && agentCoord.y == -1) agentCoord = new Vector2Int(x, y);
                    else throw new ArgumentException("More than one agent tile.");
                }
        if (agentCoord.x == -1 && agentCoord.y == -1) throw new ArgumentException("No agent tile.");

        // Get Coordinates of captured tiles relative to agent
        for (int y = 0; y < m_GridMap.row; y++)
            for (int x = 0; x < m_GridMap.column; x++)
            {
                if (grid[x, y] == 1) m_ObservationCoords.Add(new Vector2Int(x - agentCoord.x, y - agentCoord.y));
                if (grid[x, y] == 2 && m_GridMap.consider_agent_tile_an_observation) m_ObservationCoords.Add(new Vector2Int(0, 0));
            }
        if (m_ObservationCoords.Count == 0) throw new ArgumentException("No observation tiles.");

        // Create buffer
        if (m_TilemapBuffer == null)
            m_TilemapBuffer = new TilemapBuffer(m_NumChannels, m_ObservationCoords.Count);

        Detector detector = new Detector(m_Tilemap, gameObject, m_TilemapBuffer, m_ObservationCoords, m_TileTypes);
        Encoder encoder = new Encoder(m_TilemapBuffer, m_ObservationCoords, m_TileTypes);

        m_TilemapSensor = new TilemapSensor(m_SensorName, m_TilemapBuffer, m_ObservationType, detector, encoder);

        // Handle stacking sensors
        if (m_ObservationStacks > 1)
            return new ISensor[] { new StackingSensor(m_TilemapSensor, m_ObservationStacks) };
        return new ISensor[] { m_TilemapSensor };
    }

    private void FixedUpdate()
    {
        m_TilemapSensor?.Update();
    }
}
