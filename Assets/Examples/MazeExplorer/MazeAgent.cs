using UnityEngine;
using UnityEngine.Diagnostics;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using System;
using Unity.VisualScripting;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using System.Linq;
using NUnit.Framework;

public class MazeAgent : Agent
{
    public Maze maze;
    public Tilemap tm;

    private float m_RewardDecrement = 0.25f;
    private bool m_MaskActions = true;
    private float m_StepDuration = 0.1f;
    private float m_StepTime;

    private const int c_Stay = 0;
    private const int c_Up = 1;
    private const int c_Down = 2;
    private const int c_Left = 3;
    private const int c_Right = 4;

    private Vector2Int m_TilemapPosition;

    private List<int> m_ValidActions;
    private Vector2Int[] m_Directions;

    private Vector3 m_LocalPosNext;
    private Vector3 m_LocalPosPrev;

    private bool m_IsTraining;
    private bool m_IsActive;

    GameObject m_camera;
    TilemapSensorComponent sensorComp;
    private List<Tile> walls;
    private List<Tile> foods;

    private float[] visited;

    public override void Initialize()
    {
        visited = new float[maze.width * maze.height];
        m_camera = GameObject.FindWithTag("MainCamera");
        m_IsTraining = Academy.Instance.IsCommunicatorOn;
        sensorComp = GetComponent<TilemapSensorComponent>();
        walls = sensorComp.m_TileTypes.First(x => x.name == "Wall").tiles;
        foods = sensorComp.m_TileTypes.First(x => x.name == "Food").tiles;

        m_ValidActions = new List<int>(5);

        m_Directions = new Vector2Int[]
        {
                Vector2Int.zero,
                Vector2Int.up,
                Vector2Int.down,
                Vector2Int.left,
                Vector2Int.right
        };

    }

    public override void OnEpisodeBegin()
    {
        Array.Clear(visited, 0, visited.Length);
        m_TilemapPosition = maze.GenerateMaze();
        transform.localPosition = tm.CellToWorld((Vector3Int)m_TilemapPosition) + new Vector3(tm.cellSize.x / 2, tm.cellSize.x / 2, 0);
    }

    public override void WriteDiscreteActionMask(IDiscreteActionMask actionMask)
    {
        m_ValidActions.Clear();
        m_ValidActions.Add(c_Stay);

        for (int action = 1; action < 5; action++)
        {
            bool isValid = !walls.Contains(tm.GetTile((Vector3Int)(m_TilemapPosition + m_Directions[action])));

            if (isValid)
            {
                m_ValidActions.Add(action);
            }
            else if (m_MaskActions)
            {
                actionMask.SetActionEnabled(0, action, false);
            }
        }
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        var action = actions.DiscreteActions[0];
        m_LocalPosPrev = m_LocalPosNext;

        bool isDone;
        if (m_ValidActions.Contains(action))
        {
            m_TilemapPosition += m_Directions[action];
            m_LocalPosNext = new Vector3(m_TilemapPosition.x, m_TilemapPosition.y, 0) + tm.cellSize / 2;

            // Reward/penalize depending on visit value.
            isDone = ValidatePosition(true);
        }
        else
        {
            // Penalize invalid action, m_MaskActions = false.
            AddReward(-1);

            // Don't reward/penalize, but update visit value.
            isDone = ValidatePosition(false);
        }

        if (isDone)
        {
            // Visit value for m_GridPosition reached maximum.
            m_IsActive = false;
            EndEpisode();
        }
        else if (m_IsTraining || action == c_Stay || m_StepDuration == 0)
        {
            // Immediate update.
            transform.localPosition = m_LocalPosNext;
        }
        else
        {
            // Animate to next position.
            m_IsActive = false;
            m_StepTime = 0;
        }
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var discreteActionsOut = actionsOut.DiscreteActions;
        discreteActionsOut[0] = c_Stay;

        if (Input.GetKey(KeyCode.D))
        {
            discreteActionsOut[0] = c_Right;
        }
        if (Input.GetKey(KeyCode.W))
        {
            discreteActionsOut[0] = c_Up;
        }
        if (Input.GetKey(KeyCode.A))
        {
            discreteActionsOut[0] = c_Left;
        }
        if (Input.GetKey(KeyCode.S))
        {
            discreteActionsOut[0] = c_Down;
        }
    }

    private void FixedUpdate()
    {
        if (m_IsActive)
        {
            UpdateVisitedBuffer();
            RequestDecision();
        }
        else if (m_StepDuration > 0)
        {
            m_StepTime += Time.fixedDeltaTime;
            m_IsActive = m_StepTime >= m_StepDuration;
            // Animate to next position.
            transform.localPosition = Vector3.Lerp(m_LocalPosPrev,
                m_LocalPosNext, m_StepTime / m_StepDuration);
        }
        else
        {
            // Wait one step before activating.
            m_IsActive = true;
        }

        m_camera.transform.position = new Vector3(gameObject.transform.position.x, gameObject.transform.position.y, m_camera.transform.position.z);
    }

    private bool ValidatePosition(bool rewardAgent)
    {
        // From 0 to +1. 
        Vector3Int agentTile = tm.WorldToCell(gameObject.transform.position);
        float visitValue = visited[agentTile.y * maze.width + agentTile.x];

        visited[agentTile.y * maze.width + agentTile.x] = Mathf.Min(1, visitValue + m_RewardDecrement);

        if (rewardAgent)
        {
            // From +0.5 to -0.5.
            AddReward(0.5f - visitValue);

            if (foods.Contains(tm.GetTile((Vector3Int)m_TilemapPosition)))
            {
                // Reward for finding food.
                AddReward(1);
                tm.SetTile((Vector3Int)m_TilemapPosition, null);
            }
        }

        return visitValue == 1;
    }

    private void UpdateVisitedBuffer()
    {
        Vector3Int agentTile = tm.WorldToCell(gameObject.transform.position);
        int c = 0;
        foreach (Vector2Int pos in sensorComp.m_ObservationCoords)
        {
            int x = agentTile.x + pos.x;
            int y = agentTile.y + pos.y;

            sensorComp.m_TilemapBuffer.Write(1, c++, (x < 0 || x >= maze.width || y < 0 || y >= maze.height) ? 1 : visited[y * maze.width + x]);
        }
        
    }

}
