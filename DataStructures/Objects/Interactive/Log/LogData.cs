using Assets.Scripts.DataStructures.Objects.Interactive;
using UnityEngine;

public class LogData : InteractiveObjectData
{
    public LogData()
    {
        InteractiveObjectType = InteractiveObjectType.Log;

        LogScale = new Vector3(Random.Range(0.95f, 1.05f), Random.Range(0.85f, 1.15f), Random.Range(0.95f, 1.15f));
        LogRotation = new Vector3(Random.Range(-3, +4), Random.Range(0, 360), Random.Range(-3, +4));
    }

    public Vector3 LogScale { get; set; }

    public Vector3 LogRotation { get; set; }
}
