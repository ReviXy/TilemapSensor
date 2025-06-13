using UnityEngine;
using Unity.MLAgents.Sensors;
using System;
using System.Collections.Generic;
using UnityEngine.Tilemaps;
using System.Linq;
using System.Text;

public class TilemapSensor : ISensor
{
    public event Action UpdateEvent;
    public event Action ResetEvent;
    public Detector Detector { get; private set; }
    public Encoder Encoder { get; private set; }

    private readonly string m_Name;
    private readonly TilemapBuffer m_TilemapBuffer;
    private readonly ObservationSpec m_ObservationSpec;

    private float[] m_Observation;

    public TilemapSensor(string name, TilemapBuffer buffer, ObservationType observationType, Detector detector, Encoder encoder)
    {
        m_Name = name;
        m_TilemapBuffer = buffer;
        Detector = detector;
        Encoder = encoder;

        m_ObservationSpec = ObservationSpec.Vector(encoder.result.Length, observationType);
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
        writer.AddList(m_Observation);

        return m_Observation.Length;
    }

    public virtual void Update()
    {
        Detector.OnSensorUpdate();
        m_Observation = Encoder.Encode();

        UpdateEvent?.Invoke();
    }

    public virtual void Reset()
    {
        Detector?.OnSensorReset();
        ResetEvent?.Invoke();
    }


}
