using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts;
using Assets.Scripts.DataStructures;
using Assets.Scripts.DataStructures.Biomes;
using Assets.Scripts.DataStructures.Objects.Interactive.Artifacts;
using Assets.Scripts.DataStructures.Objects.Interactive.Pillar;
using Assets.Scripts.DataStructures.Objects.Static.Environmental;
using Assets.Scripts.DataStructures.Objects.Static.Structural.Puzzles;
using Assets.Scripts.Utils;
using UnityEngine;
using UnityEngine.Serialization;
using Cinemachine;

public class MapGenerator : Singleton<MapGenerator>
{
    #region Inspector variables

    [Tooltip("The Terrain Game Object")]
    [SerializeField]
    private Terrain tileHolder;

    [Header("Biomes Spawn Order")]
    [Tooltip(
        "Attach specific biome scriptable objects. Biomes will be spawned in order from the first to the last member of the list.")]
    [SerializeField]
    private List<BiomeData> biomesList;

    [Header("Map Properties")]

    [Tooltip("What types of pillars (and artifacts) should be present on the map")]
    [SerializeField] private List<PillarType> pillarsList;

    [Tooltip("How many scenery object should be placed in a single tile (larger number - more dense!")]
    [Range(0,4)]
    [SerializeField] private int sceneryPerTile = 2;

    [Tooltip("The minimum allowed distance between two spawned structures (larger number - larger the distance in between!)")]
    [Range(1,5)]
    [SerializeField] private int minDistBetweenStructures = 1;

    [Tooltip("How many tiles around the bonfire are left empty (bonfire ring)")]
    [Range(2,20)]
    [SerializeField] private int bonfireDisplacement = 5;

    [FormerlySerializedAs("puzzleDisplacement")]
    [Tooltip("How many tiles around the other important objects (puzzles, bonfires) are left empty")]
    [Range(1,5)]
    [SerializeField] private int generalDisplacement = 2;

    [Tooltip("How many tiles does each of the biomes take (size of specific biomes)")]
    [Range(3,50)]
    [SerializeField] private int perBiomeRadius = 10;

    [Tooltip("This value multiplies the size of individual tiles (default: 1)")]
    [Range(0.1f, 10f)]
    [SerializeField] private float tileMultiplier = 1;

    [Tooltip("How often should the game check for the amount of logs currently placed, and refill them")]
    [SerializeField]
    [Range(60,240)]
    private int logsRespawnFrequency = 120;

    #endregion

    public int MinLogCheck { get; private set; }
    public int MapSize { get; private set; }
    public int MinDistBetweenStructures => minDistBetweenStructures;
    public int BonfireDisplacement => bonfireDisplacement;
    public int GeneralDisplacement => generalDisplacement;
    public int PerBiomeRadius => perBiomeRadius;
    public int SceneryPerTile => sceneryPerTile;
    public List<BiomeData> BiomesList => biomesList;
    public List<PillarType> PillarsList => pillarsList;
    public Terrain TileHolder => tileHolder;
    public MapData MapData { get; set; }

    private TileMap _tileMap;
    private ProceduralGenerator _procGen;

    private GameObject player;
    private CinemachineFreeLook freeLook;

    private void Start()
    {
        _procGen = new ProceduralGenerator();
        InstantiateTerrain();
        CalculateLumber();
        GenerateTileMap();

        MapData = GenerateMapData();
        player = GameObject.FindGameObjectWithTag("Player") as GameObject;
        GenerateMap(_tileMap, MapData, player);

        freeLook = GameObject.FindGameObjectWithTag("FreeLook").GetComponent<CinemachineFreeLook>();
        
        
        freeLook.Follow = GameObject.FindGameObjectWithTag("PlayerBoots").transform;
        freeLook.LookAt = GameObject.FindGameObjectWithTag("PlayerHead").transform;

        //TODO: This doesn't belong to the map generator at all, move to somewhere suitable (maybe make a map manager, which calls MapGen on start, and handles this too...)
        InvokeRepeating(nameof(AssertEnoughLogs), logsRespawnFrequency, logsRespawnFrequency);
    }

    private static void GenerateMap(TileMap tileMap, MapData mapData, GameObject player)
    {
        PrefabSpawner.Instance.SpawnInitialMapState(tileMap, mapData, player);
    }

    private void AssertEnoughLogs()
    {
        var countedLogs = MapData.GetLumberCount();
        if (countedLogs >= MinLogCheck) return;

        var logList = _procGen.RegenerateLogs(MapData, MinLogCheck - countedLogs);
        foreach (var log in logList.Cast<LogData>())
        {
            PrefabSpawner.Instance.CallSpawnLogPrefab(_tileMap, log);
        }
    }

    private void InstantiateTerrain()
    {
        if (!biomesList.Any()) throw new Exception("No biomes specified, map requires at least one biome");

        MapSize = ((bonfireDisplacement + (perBiomeRadius * biomesList.Count))*2)+1;
        tileHolder.terrainData.size = new Vector3(MapSize, 10, MapSize) * tileMultiplier;
    }

    //TODO: Might wanna expose this logic to the inspector, or refine it somehow
    private void CalculateLumber()
    {
        if (MapSize == 0) throw new Exception("Something went wrong, map size could not be fetched");

        MinLogCheck = MapSize * 2;
    }

    private MapData GenerateMapData() => _procGen.GenerateMapData();
    private void GenerateTileMap() => _tileMap = new TileMap(MapSize, MapSize, tileHolder);

    public int GetGeneralDisplacement(GeneratedObjects generatedObjType, PuzzleType puzzleType = PuzzleType.None)
    {
        if (puzzleType == PuzzleType.None) return GeneralDisplacement;
        switch (puzzleType)
        {
          case PuzzleType.Maze:
          case PuzzleType.RotationPuzzle:
          case PuzzleType.SimonSays: 
              return GeneralDisplacement + 1;
          default:
              return GeneralDisplacement;
        }
    }
    public BiomeData GetBiomeByType(BiomeType type) => BiomesList.FirstOrDefault(biome => biome.BiomeType == type);
    public EnvironmentalObjectType GetEnvironmentalObjectFrequency(BiomeType type) => GetBiomeByType(type).GetEnvironmentalObjectFrequency();
}
