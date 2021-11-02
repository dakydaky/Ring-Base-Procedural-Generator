using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.DataStructures;
using Assets.Scripts.DataStructures.Biomes;
using Assets.Scripts.DataStructures.Objects;
using Assets.Scripts.DataStructures.Objects.Interactive;
using Assets.Scripts.DataStructures.Objects.Static;
using Assets.Scripts.DataStructures.Objects.Static.Environmental;
using Assets.Scripts.DataStructures.Objects.Static.Structural;
using Assets.Scripts.DataStructures.Objects.Static.Structural.Puzzles;
using Assets.Scripts.Utils;
using Assets.Scripts.Character;
using UnityEngine;
using Random = UnityEngine.Random;
using Cinemachine;

public class PrefabSpawner : Singleton<PrefabSpawner>
{
    //TODO: Find out why it still clips 
    private const float MinOffset = -0.35f;
    private const float MaxOffset = 0.35f;

    [Header("Player Prefab")] [SerializeField]
    private GameObject playerPrefab;
    private GameObject playerGO;
    private CinemachineFreeLook freeLook;

    [Header("Temp Ground Prefabs")]
    [SerializeField] private GameObject forestPrefab;
    [SerializeField] private GameObject snowPrefab;
    [SerializeField] private GameObject burntPrefab;
    [SerializeField] private GameObject barrenPrefab;

    [Header("Puzzle Object Prefabs")]
    [SerializeField] private GameObject simonPrefab;
    [SerializeField] private GameObject mazePrefab;
    [SerializeField] private GameObject rotationPrefab;

    [Header("Unique Object Prefabs")]
    [SerializeField] private List<GameObject> logPrefabList;
    [SerializeField] private GameObject bonfirePrefab;
    [SerializeField] private List<GameObject> pillarPrefabList;
    [SerializeField] private List<GameObject> rockPrefabList;
    //TODO: See if we use them and disable them if we don't
    [SerializeField] private List<GameObject> additionalStructuresPrefabList;
    [SerializeField] private List<GameObject> bushPrefabList;
    [SerializeField] private List<GameObject> grassPrefabList;

    [Header("Unique Tree Prefabs")]
    [SerializeField] private List<GameObject> treePrefabList;
    [SerializeField] private List<GameObject> deadTreePrefabList;
    [SerializeField] private List<GameObject> treeStumpPrefabList;


    public Vector3 GenerateOffset(TileMap tileMap, int column, int row, int objectNum)
    {
        var x = tileMap[column, row].x;
        if (objectNum % 2 == 0 || objectNum == 0)         
            return new Vector3(
            tileMap[column, row].x + Random.Range(MinOffset, 0f),
            tileMap[column, row].y,
            tileMap[column, row].z + Random.Range(MinOffset, 0f));

        return new Vector3(
            tileMap[column, row].x + Random.Range(0f, MaxOffset),
            tileMap[column, row].y,
            tileMap[column, row].z + Random.Range(0f, MaxOffset));
    }

    public void SpawnInitialMapState(TileMap tileMap, MapData mapData, GameObject player)
    {
        PopulateTileMap(tileMap, mapData, player);
    }

    private void PopulateTileMap(TileMap tileMap, MapData mapData, GameObject player)
    {
        for (var column = 0; column < MapGenerator.Instance.MapSize; column++)
        {
            for (var row = 0; row < MapGenerator.Instance.MapSize; row++)
            {
                var objectNum = 0;
                foreach (var objectData in mapData[column, row])
                {
                    switch (objectData.ObjectType)
                    {
                        case ObjectType.None:
                            break;
                        case ObjectType.Interactive:
                            SpawnInteractiveObject(tileMap, objectData, column, row, objectNum, player);
                            break;
                        case ObjectType.Static:
                            SpawnStaticObject(tileMap, objectData, column, row, objectNum);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException(nameof(tileMap));
                    }
                    objectNum++;
                }
            }
        }
    }

    private void SpawnInteractiveObject(TileMap tileMap, ObjectData objectData, int col, int row, int objectNum, GameObject player)
    {
        var interactiveObjData = (InteractiveObjectData) objectData;
        switch (interactiveObjData.InteractiveObjectType)
        {
            case InteractiveObjectType.Player:
                if (player == null)
                {
                    var thisPlayer = (PlayerData) interactiveObjData;
                    playerGO = SpawnPrefab(playerPrefab, new Vector3(col, 1.0f, row), Quaternion.identity, new Vector3(.5f,.54f,.5f));
                    PlayerStateManager.Instance.SetPlayerObject(playerGO);
                }
                
                break;
            case InteractiveObjectType.Log:
                var thisLog = (LogData)interactiveObjData;
                SpawnPrefab(logPrefabList[Random.Range(0, logPrefabList.Count)],
                            GenerateOffset(tileMap, col, row, objectNum), Quaternion.Euler(thisLog.LogRotation),
                            thisLog.LogScale);
                break;
            case InteractiveObjectType.Artifact:
                var thisArtifact = (ArtifactData)interactiveObjData;
                /*SpawnPrefab(artifactPrefabList.First(obj => obj.CompareTag(thisArtifact.ArtifactType.ToString())),
                            GenerateOffset(tileMap, col, row, objectNum), Quaternion.identity);*/
                break;
            case InteractiveObjectType.Pillar:
                var thisPillar = (PillarData)interactiveObjData;
                SpawnPrefab(pillarPrefabList.First(obj => obj.CompareTag(thisPillar.PillarType.ToString())),
                            GenerateOffset(tileMap, col, row, objectNum), Quaternion.identity);
                SpawnPillarObject();
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(interactiveObjData.InteractiveObjectType));
        }
    }

    private void SpawnStaticObject(TileMap tileMap, ObjectData objectData, int col, int row, int objectNum)
    {
        var staticObjData = (StaticObjectData) objectData;
        switch (staticObjData.StaticObjectType)
        {
            case StaticObjectType.Structural:
                SpawnStructuralObject(tileMap, objectData, col, row, objectNum);
                break;
            case StaticObjectType.Environmental:
                SpawnEnvironmentalObject(tileMap, objectData, col, row, objectNum);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(staticObjData.StaticObjectType));
        }
    }

    private void SpawnEnvironmentalObject(TileMap tileMap, ObjectData objectData, int col, int row, int objectNum)
    {
        var enviroObjData = (EnvironmentalObjectData) objectData;
        switch (enviroObjData.EnvironmentalObjectType)
        {
            case EnvironmentalObjectType.Tree:
                var thisTree = (TreeData)objectData;
                var thisBiome = MapGenerator.Instance.BiomesList.First(obj =>
                    obj.BiomeType == MapGenerator.Instance.MapData.GetBiomeType(col, row));

                var treeType = thisBiome.GetTreeTypeByFrequency();
                switch (treeType)
                {
                    case TreeType.Stump:
                        SpawnPrefab(treeStumpPrefabList[Random.Range(0, treeStumpPrefabList.Count)],
                            GenerateOffset(tileMap, col, row, objectNum), Quaternion.Euler(thisTree.TreeRotation), thisTree.TreeScale);
                        break;
                    case TreeType.Grown:
                        SpawnPrefab(treePrefabList[Random.Range(0, treePrefabList.Count)],
                            GenerateOffset(tileMap, col, row, objectNum), Quaternion.Euler(thisTree.TreeRotation), thisTree.TreeScale);
                        break;
                    case TreeType.Dead:
                        SpawnPrefab(deadTreePrefabList[Random.Range(0, deadTreePrefabList.Count)],
                            GenerateOffset(tileMap, col, row, objectNum), Quaternion.Euler(thisTree.TreeRotation), thisTree.TreeScale);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(treeType));
                }

                break;
            case EnvironmentalObjectType.Rock:
                var thisRock = (RockData)objectData;
                SpawnPrefab(rockPrefabList[Random.Range(0, rockPrefabList.Count)],
                    GenerateOffset(tileMap, col, row, objectNum), Quaternion.Euler(thisRock.RockRotation), thisRock.RockScale);
                break;
            case EnvironmentalObjectType.Bushes:
                var thisBush = (BushesData) objectData;
                SpawnPrefab(bushPrefabList[Random.Range(0, bushPrefabList.Count)],
                    GenerateOffset(tileMap, col, row, objectNum), Quaternion.Euler(thisBush.BushesRotation), thisBush.BushesScale);
                break;
            case EnvironmentalObjectType.Grass:
                var thisGrass = (GrassData) objectData;
                SpawnPrefab(grassPrefabList[Random.Range(0, grassPrefabList.Count)],
                    GenerateOffset(tileMap, col, row, objectNum), Quaternion.Euler(thisGrass.GrassRotation), thisGrass.GrassScale);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(enviroObjData.EnvironmentalObjectType));
        }
    }

    private void SpawnStructuralObject(TileMap tileMap, ObjectData objectData, int col, int row, int objectNum)
    {
        var structuralObjData = (StructuralObjectData) objectData;
        switch (structuralObjData.StructuralObjectType)
        {
            case StructuralObjectType.Puzzle:
                SpawnPuzzleObject(tileMap, objectData, col, row, objectNum);
                break;
            case StructuralObjectType.Bonfire:
                SpawnPrefab(bonfirePrefab, GenerateOffset(tileMap, col, row, objectNum));
                break;
            // TODO: Implement this if we use additional structures
            case StructuralObjectType.AdditionalStructure:
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(structuralObjData.StructuralObjectType));
        }
    }
    //TODO: Implement this!
    private void SpawnPillarObject()
    {

    }

    private void SpawnPuzzleObject(TileMap tileMap, ObjectData objectData, int col, int row, int objectNum)
    {
        var puzzleObjData = (PuzzleData) objectData;
        switch (puzzleObjData.PuzzleType)
        {
            case PuzzleType.PuzzleBody:
                break;
            case PuzzleType.SimonSays:
                SpawnPrefab(simonPrefab, GenerateOffset(tileMap, col, row, objectNum), Quaternion.identity, puzzleObjData.Scale);
                break;
            case PuzzleType.Maze:
                SpawnPrefab(mazePrefab, GenerateOffset(tileMap, col, row, objectNum), Quaternion.identity, puzzleObjData.Scale);
                break;
            case PuzzleType.RotationPuzzle:
                SpawnPrefab(rotationPrefab, GenerateOffset(tileMap, col, row, objectNum), Quaternion.identity, puzzleObjData.Scale);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(puzzleObjData.PuzzleType));

        }
    }

    public GameObject SpawnPrefab(GameObject prefab, Vector3 position, Quaternion quaternion = default, Vector3 scale = default)
    {
        var parent = MapGenerator.Instance.TileHolder.transform;

        if (quaternion == default)
        {
            quaternion = Quaternion.identity;
        }

        if (scale == default)
        {
            scale = new Vector3(0.25f, 0.25f, 0.25f);
        }

        var spawned = Instantiate(prefab, position, quaternion, parent);
        spawned.transform.localScale = scale;
        return spawned;
    }

    public void CallSpawnLogPrefab(TileMap tileMap, LogData log) 
        =>  SpawnPrefab(logPrefabList[Random.Range(0, logPrefabList.Count)],
            GenerateOffset(tileMap, log.ObjectPosX, log.ObjectPosY, 0),
            Quaternion.Euler(log.LogRotation), log.LogScale);
}

public enum BiomeDependentPrefab
{
    Tree,
    Rock,
    Grass,
    Bush
}