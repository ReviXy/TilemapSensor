using UnityEngine;
using Unity.MLAgents.Sensors;
using System;
using System.Collections.Generic;
using UnityEngine.Tilemaps;
using System.Linq;

public class TilemapSensor : ISensor
{
    public event Action UpdateEvent;
    public event Action ResetEvent;
    public Detector Detector { get; private set; }
    public Encoder Encoder { get; private set; }

    public bool AutoDetectionEnabled { get; private set; }

    private readonly string m_Name;
    private readonly TilemapBuffer m_TilemapBuffer;
    private readonly ObservationSpec m_ObservationSpec;
    // PNG compression.
    private Texture2D m_PerceptionTexture;
    private List<byte> m_CompressedObs;


    public TilemapSensor(string name, TilemapBuffer buffer, ObservationType observationType)
    {
        m_Name = name;
        m_TilemapBuffer = buffer;
        m_ObservationSpec = ObservationSpec.Vector(m_TilemapBuffer.Count, observationType);
    }

    public void SetDetectorEncoder(Detector detector, Encoder encoder)
    {
        Detector = detector;
        Encoder = encoder;
        AutoDetectionEnabled = true;
    }

    public string GetName()
    {
        return m_Name;
    }

    public ObservationSpec GetObservationSpec()
    {
        return m_ObservationSpec;
    }

    public CompressionSpec GetCompressionSpec()
    {
        return new CompressionSpec(SensorCompressionType.None);
    }

    public byte[] GetCompressedObservation()
    {
        throw new ArgumentException("Compressed format not supported.");
    }

    public int Write(ObservationWriter writer)
    {
        int n = m_TilemapBuffer.NumChannels;

        float[] onehot = Encoder.Encode(0);
        writer.AddList(onehot);

        for (int c = 1; c < n; c++)
            writer.AddList(m_TilemapBuffer.GetChannel(c));

        return Detector.observationCoords.Count * n;
    }

    public virtual void Update()
    {
        if (AutoDetectionEnabled)
        {
            Detector.OnSensorUpdate();
            float[] onehot = Encoder.Encode(0);
        }

        UpdateEvent?.Invoke();
    }

    public virtual void Reset()
    {
        Detector?.OnSensorReset();
        ResetEvent?.Invoke();
    }


}
