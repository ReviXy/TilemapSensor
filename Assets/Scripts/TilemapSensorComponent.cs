using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;

using System.Collections.Generic;
using System.Collections;
using System;
using System.ComponentModel;
using UnityEngine.Tilemaps;
using System.Text;

public class TilemapSensorComponent : SensorComponent, IDisposable
{
    public string SensorName
    {
        get { return m_SensorName; }
        set { m_SensorName = value; }
    }

    [SerializeField]
    private string m_SensorName;

    public int ObservationStacks
    {
        get { return m_ObservationStacks; }
        set { m_ObservationStacks = value; }
    }
    [SerializeField, Min(1)]
    private int m_ObservationStacks = 1;

    public ObservationType ObservationType
    {
        get { return m_ObservationType; }
        set { m_ObservationType = value; }
    }
    [SerializeField]
    private ObservationType m_ObservationType = ObservationType.Default;

    //public List<ChannelLabel> ChannelLabels
    //{
    //    get { return m_ChannelLabels; }
    //    set { m_ChannelLabels = new List<ChannelLabel>(value); }
    //}
    //[SerializeField, HideInInspector]
    //protected List<ChannelLabel> m_ChannelLabels;

    //// Non-Editor flag for subcomponents.
    //protected bool m_Debug_IsEnabled;

    //public TilemapBuffer TilemapBuffer
    //{
    //    get { return m_TilemapBuffer; }
    //    set { m_TilemapBuffer = value; }
    //}
    private TilemapBuffer TilemapBuffer;
        
    [SerializeField]
    private Tilemap tm;

    [SerializeField]
    private List<TileType> tileTypes = new List<TileType>();

    [SerializeField]
    private GridMapperLite gridMap;
    //_____________________________________________________________

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

    //_______________________

    public TilemapSensor TilemapSensor
    {
        get { return m_TilemapSensor; }
    }
    protected TilemapSensor m_TilemapSensor;

    public bool HasSensor
    {
        get { return m_TilemapSensor != null; }
    }

    public override ISensor[] CreateSensors()
    {
        Vector2Int agentCoord = new Vector2Int(-1, -1);
        List<Vector2Int> observationCoords = new List<Vector2Int>();
        int[,] grid = gridMap.Grid();

        for (int y = 0; y < gridMap.row; y++)
            for (int x = 0; x < gridMap.column; x++)
                if (grid[x, y] == 2)
                {
                    if (agentCoord.x == -1 && agentCoord.y == -1) agentCoord = new Vector2Int(x, y);
                    else throw new ArgumentException("More than one agent tile.");
                }
        if (agentCoord.x == -1 && agentCoord.y == -1) throw new ArgumentException("No agent tile.");

        for (int y = 0; y < gridMap.row; y++)
            for (int x = 0; x < gridMap.column; x++)
            {
                if (grid[x, y] == 1) observationCoords.Add(new Vector2Int(x - agentCoord.x, y - agentCoord.y));
                if (grid[x, y] == 2 && gridMap.consider_agent_tile_an_observation) observationCoords.Add(new Vector2Int(0, 0));
            }
        if (observationCoords.Count == 0) throw new ArgumentException("No observation tiles.");

        if (TilemapBuffer == null)
            TilemapBuffer = new TilemapBuffer(1, observationCoords.Count);

        Dictionary<Tile, int> temp = new Dictionary<Tile, int>();
        for (int i = 0; i < tileTypes.Count; i++)
        {
            foreach (Tile t in tileTypes[i].tiles)
                temp.Add(t, i + 1);
        }

        Detector detector = new Detector(tm, observationCoords, gameObject, TilemapBuffer, temp);
        Encoder encoder = new Encoder(TilemapBuffer, temp, observationCoords);

        m_TilemapSensor = new TilemapSensor(m_SensorName, TilemapBuffer, m_ObservationType);
        m_TilemapSensor.SetDetectorEncoder(detector, encoder);

        //--------

        if (m_ObservationStacks > 1)
            return new ISensor[] { new StackingSensor(m_TilemapSensor, m_ObservationStacks) };
        return new ISensor[] { m_TilemapSensor };
    }

    private void FixedUpdate()
    {
        m_TilemapSensor?.Update();
    }

    //________________________

    private void Reset()
    {
        HandleReset();
    }

    protected virtual void HandleReset() { }
    private void OnDestroy()
    {
        Dispose();
    }

    public virtual void Dispose() { }



}
