using UnityEngine;
using Unity.MLAgents;
using System;

public class TilemapBuffer
{
    public int NumChannels
    {
        get { return m_NumChannels; }
        set { m_NumChannels = value; Initialize(); }
    }
    private int m_NumChannels;

    public int Count
    {
        get { return m_Count; }
        set { m_Count = value; Initialize(); }
    }
    private int m_Count;


    private float[][] m_Values;

    public TilemapBuffer(int numChannels, int count)
    {
        m_NumChannels = numChannels;
        m_Count = count;
        Initialize();
    }

    protected virtual void Initialize()
    {
        m_Values = new float[m_NumChannels][];

        for (int i = 0; i < m_NumChannels; i++)
            m_Values[i] = new float[Count];
    }

    public virtual float[] GetChannel(int channel)
    {
        return m_Values[channel];
    }

    public virtual void ClearChannel(int channel)
    {
        if (channel < m_NumChannels)
            Array.Clear(m_Values[channel], 0, m_Values[channel].Length);
    }

    public virtual void ClearChannels(int start, int count)
    {
        for (int i = 0; i < count; i++)
            ClearChannel(start + i);
    }

    public virtual void Clear()
    {
        ClearChannels(0, m_NumChannels);
    }

    public virtual bool ContainsPosition(int index)
    {
        return index >= 0 && index < m_Count;
    }

    public virtual void Write(int channel, int index, float value)
    {
        m_Values[channel][index] = value;
    }

    public virtual bool TryWrite(int channel, int index, float value)
    {
        bool contains = ContainsPosition(index);
        if (contains) Write(channel, index, value);
        return contains;
    }

    public virtual float Read(int channel, int index)
    {
        return m_Values[channel][index];
    }

    public virtual bool TryRead(int channel, int index, out float value)
    {
        bool contains = ContainsPosition(index);
        value = contains ? m_Values[channel][index] : 0;
        return contains;
    }

}
