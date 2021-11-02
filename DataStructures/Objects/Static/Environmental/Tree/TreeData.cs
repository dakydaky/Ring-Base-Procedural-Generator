using System;
using Assets.Scripts.DataStructures.Objects.Static.Environmental;
using UnityEngine;
using Random = UnityEngine.Random;

public class TreeData : EnvironmentalObjectData
{
    public TreeData()
    {
        EnvironmentalObjectType = EnvironmentalObjectType.Tree;

        TreeScale = new Vector3(Random.Range(0.22f, 0.28f), Random.Range(0.23f, 0.32f), Random.Range(0.22f, 0.28f));
        TreeRotation = new Vector3(Random.Range(-2, +3), Random.Range(0, 360), Random.Range(-2, +3));
    }

    public Vector3 TreeScale { get; set; }
    public Vector3 TreeRotation { get; set; }
}
