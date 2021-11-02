using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.DataStructures;
using Assets.Scripts.DataStructures.Biomes;
using Assets.Scripts.DataStructures.Objects;
using Assets.Scripts.DataStructures.Objects.Interactive.Artifacts;
using Assets.Scripts.DataStructures.Objects.Interactive.Pillar;
using Assets.Scripts.DataStructures.Objects.Static;
using Assets.Scripts.DataStructures.Objects.Static.Environmental;
using Assets.Scripts.DataStructures.Objects.Static.Structural.Puzzles;
using Assets.Scripts.DataStructures.Objects.Static.Structural.Puzzles.MazeData;
using Assets.Scripts.DataStructures.Objects.Static.Structural.Puzzles.PuzzleBody;
using Assets.Scripts.DataStructures.Objects.Static.Structural.Puzzles.RotationPuzzle;
using Assets.Scripts.DataStructures.Objects.Static.Structural.Puzzles.SimonSays;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Assets.Scripts
{
    public class ProceduralGenerator
    {
        private const int MaxSpawnAttempts = 1000;
        private const int TargetPillarDistance = 4;

        private static int _minDistStructures;

        private int _mapSize;
        private int _perRingRadius;

        private static int _bonfireXY;

        private readonly List<(int x, int y)> _bonfireDataHelper;
        private readonly List<(BiomeType biome,List<(int x, int y)> coords)> _biomesDataHelper;
        private readonly List<(int x, int y)> _sceneryTiles;

        private List<PillarType> _pillarsToPlace;
        private MapData _mapData;

        public ProceduralGenerator()
        {
            _biomesDataHelper = new List<(BiomeType biome, List<(int x, int y)> coords)>();
            _sceneryTiles = new List<(int x, int y)>();
            _bonfireDataHelper = new List<(int x, int y)>();
        }

        public MapData GenerateMapData()
        {
            _mapSize = MapGenerator.Instance.MapSize;
            _perRingRadius = MapGenerator.Instance.PerBiomeRadius;

            if (MapGenerator.Instance.BiomesList.Any(biome => biome.RequiredPuzzles.Any(puzzle => puzzle == PuzzleType.PuzzleBody)))
                throw new Exception("PuzzleBody cannot be given as a Biome-specific puzzle to spawn. It defines the space of the puzzle beside its pivot tile.");

            _minDistStructures = (_perRingRadius / 2 - 1) * MapGenerator.Instance.MinDistBetweenStructures;
            Debug.Log($"Min dist between structures: {_minDistStructures}");
            _mapData = new MapData(_mapSize, _mapSize);
            _pillarsToPlace = MapGenerator.Instance.PillarsList;

            GenerateStartingBonfire();
            GenerateRings();

            return _mapData;
        }

        private static bool AssertIsInRing(int x, int y, int radius)
        {
            if (x == _bonfireXY || y == _bonfireXY) radius--;
            return Math.Sqrt((Math.Pow(Math.Abs(_bonfireXY - x), 2)) + Math.Pow(Math.Abs(_bonfireXY - y), 2)) <= radius;
        }

        private static bool AssertIsInRing(int x, int y, int bonfireX, int bonfireY, int radius)
        {
            if (x == bonfireX || y == bonfireY) radius--;
            return Math.Sqrt((Math.Pow(Math.Abs(bonfireX - x), 2)) + Math.Pow(Math.Abs(bonfireY - y), 2)) <= radius;
        }

        private void GenerateRings()
        {
            var biomes = MapGenerator.Instance.BiomesList;
            var ringRadius = MapGenerator.Instance.BonfireDisplacement;
            var accumulator = 0;

            for (var ring = 1; ring <= biomes.Count + 1; ring++)
            {
                if (ring != biomes.Count+1)
                {
                    _biomesDataHelper.Add((biomes[ring-1].BiomeType, new List<(int, int)>()));
                    if(ring != 1) ringRadius = _perRingRadius;
                }
                else {_biomesDataHelper.Add((BiomeType.Fog, new List<(int, int)>()));}
                accumulator += ringRadius;

                for (var col = _bonfireXY - accumulator; col <= _bonfireXY + accumulator; col++)
                {
                    for (var row = _bonfireXY - accumulator; row <= _bonfireXY + accumulator; row++)
                    {
                        if (AssertIsInRing(col, row, accumulator) && _mapData.Map[col][row].BiomeType == BiomeType.None)
                        {
                            if (ring == 1)
                            {
                                _mapData.Map[col][row].BiomeType = BiomeType.Bonfire;
                                _mapData[col, row].Add(ObjectData.Empty);

                                if(Math.Abs(_bonfireXY-col) + Math.Abs(_bonfireXY-row) == TargetPillarDistance)
                                    _bonfireDataHelper.Add((col, row));
                                continue;
                            }
                            _mapData.Map[col][row].BiomeType = biomes[ring-2].BiomeType;
                            _biomesDataHelper[ring-2].coords.Add((col,row));
                        }
                        else if(_mapData.Map[col][row].BiomeType == BiomeType.None && AssertIsInRing(col, row, accumulator+ringRadius))
                        {
                            _mapData.Map[col][row].BiomeType = ring != biomes.Count + 1 ?  biomes[ring - 1].BiomeType : BiomeType.Fog;
                            _biomesDataHelper[ring-1].coords.Add((col,row));
                        }
                    }
                }
            }
            GenerateNonScenery();

            for (var i = 0; i < _biomesDataHelper.Count-1; i++)
            {
                _sceneryTiles.AddRange(_biomesDataHelper[i].coords);
            }

            GenerateScenery();
        }

        public List<ObjectData> RegenerateLogs(MapData mapDataN, int spawnAmount)
        {
            //TODO: Test this please
            var spawnedLogList = new List<ObjectData>();

            var logSpawned = 0;
            var spawnAttempts = 0;
            do
            {
                spawnAttempts++;
                if (spawnAttempts > MaxSpawnAttempts)
                {
                    Debug.Log($"Attempted 1000 random log spawn positions, only managed to place {logSpawned}, moving on!");
                    break;
                } // Something went wrong, avoid infinite loop

                var (selX, selY) = _sceneryTiles[Random.Range(0, _sceneryTiles.Count)];

                if (mapDataN[selX, selY].All(tile => tile.ObjectType != ObjectType.None)) continue;
                {
                    var replaceIndex = mapDataN[selX, selY].FindIndex(tile =>
                        tile.ObjectType == ObjectType.None);
                    mapDataN.ReplaceObjectData(selX, selY, replaceIndex, new LogData());

                    spawnedLogList.Add(mapDataN[selX, selY][replaceIndex]);
                    logSpawned++;
                }
            }
            while (logSpawned < spawnAmount);

            return spawnedLogList;
        }

        private void GenerateStartingBonfire()
        {
            _bonfireXY = _mapSize / 2;
            _mapData[_mapSize/2, _mapSize/2].Add(new BonfireData());
            _mapData[_mapSize/2+1, _mapSize/2+1].Add(new PlayerData());
        }

        private void GenerateNonScenery()
        {
            ObjectGenerator(GeneratedObjects.Pillar, _pillarsToPlace.Count, -1);

            for (var i = 0; i < _biomesDataHelper.Count - 1; i++)
            { 
                if(i > 0)
                    ObjectGenerator(GeneratedObjects.Bonfire, 1, i);

                if(MapGenerator.Instance.BiomesList[i].RequiredPuzzles.Count > 0)
                    ObjectGenerator(GeneratedObjects.Puzzle, MapGenerator.Instance.BiomesList[i].RequiredPuzzles.Count, i);

                if(MapGenerator.Instance.BiomesList[i].NumAdditionalStructures > 0) 
                    ObjectGenerator(GeneratedObjects.AdditionalStructure, MapGenerator.Instance.BiomesList[i].NumAdditionalStructures, i);
            }
        }

        private void GenerateScenery()
        {
            foreach (var t in _sceneryTiles)
            {
                var (x, y) = t;
                var thisBiome = _mapData.GetBiomeType(x, y);

                for (var j = 0; j < MapGenerator.Instance.SceneryPerTile; j++)
                {
                    _mapData[x,y].Add(GenerateEnvironmentalData(MapGenerator.Instance.GetEnvironmentalObjectFrequency(thisBiome)));
                }
            }
        }

        private void ObjectGenerator(GeneratedObjects objectToGen, int numToGen, int ring)
        {
            var minDistStructures = objectToGen != GeneratedObjects.Pillar ? _minDistStructures : 1;

            var numPlaced = 0;
            var placeAttempts = 0;
            do
            {
                placeAttempts++;
                if (placeAttempts > MaxSpawnAttempts)
                {
                    Debug.Log($"Attempted 1000 random spawn positions, only managed to place {numPlaced} {objectToGen}, moving on!");
                    break;
                }

                var selectedIndex = objectToGen != GeneratedObjects.Pillar ? Random.Range(0, _biomesDataHelper[ring].coords.Count) : Random.Range(0, _bonfireDataHelper.Count);
                var (selectedX, selectedY) = objectToGen != GeneratedObjects.Pillar ? _biomesDataHelper[ring].coords[selectedIndex] : _bonfireDataHelper[selectedIndex];

                var invalidPlacement = false;

                for (var adjX = selectedX - minDistStructures; adjX <= selectedX +  minDistStructures; adjX++)
                {
                    for (var adjY = selectedY -  minDistStructures; adjY <= selectedY +  minDistStructures; adjY++)
                    {
                        try{
                            //For pillars, just check if there is another obj nearby
                            if (objectToGen == GeneratedObjects.Pillar)
                            {
                                if(_mapData[adjX, adjY].Any(obj => obj.ObjectType == ObjectType.Interactive))
                                    invalidPlacement = true;
                                continue;
                            }
                            if (TileIsOutOfBounds(adjX, adjY) || _mapData.GetBiomeType(adjX, adjY) != _mapData.GetBiomeType(selectedX, selectedY))
                            {
                                if ((objectToGen == GeneratedObjects.Puzzle || objectToGen == GeneratedObjects.Bonfire)
                                    && Math.Abs(selectedX - adjX) <= MapGenerator.Instance.GetGeneralDisplacement(objectToGen, IsPuzzleReturn(objectToGen, ring, numPlaced)) 
                                    && Math.Abs(selectedY - adjY) <= MapGenerator.Instance.GetGeneralDisplacement(objectToGen, IsPuzzleReturn(objectToGen, ring, numPlaced)))
                                    invalidPlacement = true;
                                continue;
                            }
                            if (_mapData[adjX, adjY].Any(obj => obj.ObjectType == ObjectType.Interactive) ||
                                _mapData[adjX, adjY].Where(obj => obj.ObjectType == ObjectType.Static).Cast<StaticObjectData>().Any(obj => obj.StaticObjectType == StaticObjectType.Structural))
                            {
                                invalidPlacement = true;
                            }
                        } catch(Exception) {throw new Exception($"Error on reading _mapData[{adjX},{adjY}]");}
                    }
                }
                if (invalidPlacement)
                {
                    continue;
                }

                _mapData[selectedX, selectedY].Add(GenerateGeneratedObjectData(objectToGen,
                                                                                   IsPuzzleReturn(objectToGen, ring, numPlaced),
                                                                                   IsPillarReturn(objectToGen)));

                switch (objectToGen)
                {
                    case GeneratedObjects.Puzzle:
                    {
                        var puzzleDis = MapGenerator.Instance.GeneralDisplacement;
                        var puzzle = (PuzzleData)_mapData[selectedX, selectedY][0];
                        for (var adjX = selectedX - ((puzzle.Size-1)/2 + 1 + puzzleDis);
                            adjX <= selectedX + puzzle.Size / 2 + puzzleDis;
                            adjX++)
                        {
                            for (var adjY = selectedY - ((puzzle.Size-1)/2 + 1 + puzzleDis);
                                adjY <= selectedY + puzzle.Size / 2 + puzzleDis;
                                adjY++)
                            {
                                if(AssertIsInRing(adjX, adjY, selectedX, selectedY,(puzzle.Size-1)/2 + 1 + puzzleDis))
                                {
                                    _mapData[adjX, adjY].Add(new PuzzleBodyData());
                                    var index = _biomesDataHelper.FindIndex(obj => obj.coords.Contains((adjX, adjY)));
                                    _biomesDataHelper[index].coords.Remove((adjX, adjY));
                                };
                            }
                        }
                        break;
                    }
                    case GeneratedObjects.Bonfire:
                    {
                        var bonfireDis = MapGenerator.Instance.BonfireDisplacement / 2;
                        for (var adjX = selectedX - bonfireDis;
                            adjX <= selectedX + bonfireDis;
                            adjX++)
                        {
                            for (var adjY = selectedY - bonfireDis;
                                adjY <= selectedY + bonfireDis;
                                adjY++)
                            {
                                if (AssertIsInRing(adjX, adjY, selectedX, selectedY, bonfireDis))
                                {
                                    var index = _biomesDataHelper.FindIndex(obj => obj.coords.Contains((adjX, adjY)));
                                    _biomesDataHelper[index].coords.Remove((adjX, adjY));
                                }
                            }
                        }
                        break;
                    }
                }

                if(objectToGen != GeneratedObjects.Pillar)_biomesDataHelper[ring].coords.Remove((selectedX, selectedY));
                else {_bonfireDataHelper.Remove((selectedX, selectedY));}

                numPlaced++;
            } while (numPlaced < numToGen);
        }

        private PillarType IsPillarReturn(GeneratedObjects obj)
        {
            if (obj != GeneratedObjects.Pillar) return PillarType.None;

            var pillar = _pillarsToPlace[0];
            _pillarsToPlace.RemoveAt(0);
            return pillar;
        }

        private static PuzzleType IsPuzzleReturn(GeneratedObjects obj, int index, int cnt)
        {
            if (obj != GeneratedObjects.Puzzle) return PuzzleType.None;
            if (MapGenerator.Instance.BiomesList[index].RequiredPuzzles.Count == 0) return PuzzleType.None;

            var puzzle = MapGenerator.Instance.BiomesList[index].RequiredPuzzles[cnt];
            return puzzle;
        }

        private bool TileIsInBounds(int x, int y) => 
            (x >= 0) && (y >= 0) && (x < _mapSize) && (y < _mapSize);
        private bool TileIsOutOfBounds(int x, int y) => (x < 0 || y < 0 || x >= _mapSize || y >= _mapSize);

        private static ObjectData GenerateEnvironmentalData(EnvironmentalObjectType enviroObj)
        {
            return enviroObj switch
            {
                EnvironmentalObjectType.Tree => new TreeData(),
                EnvironmentalObjectType.Rock => new RockData(),
                EnvironmentalObjectType.Log => new LogData(),
                EnvironmentalObjectType.Bushes => new BushesData(),
                EnvironmentalObjectType.Grass => new BushesData(),
                _ => ObjectData.Empty,
            };
        }

        private static ObjectData GeneratePuzzleData(PuzzleType puzzleObj)
        {
            return puzzleObj switch
            {
                PuzzleType.Maze => new MazeData(),
                PuzzleType.RotationPuzzle => new RotationPuzzleData(),
                PuzzleType.SimonSays => new SimonSaysData(),
                _ => ObjectData.Empty,
            };
        }

        private static ObjectData GenerateGeneratedObjectData(GeneratedObjects genObj, PuzzleType puzzleObj = PuzzleType.None, PillarType pillarObj = PillarType.None)
        {
            return genObj switch
            {
                GeneratedObjects.Puzzle => GeneratePuzzleData(puzzleObj),
                //TODO: We currently have none, if we implement more, update accordingly
                GeneratedObjects.AdditionalStructure => ObjectData.Empty,
                GeneratedObjects.Pillar => new PillarData(pillarObj),
                GeneratedObjects.Bonfire => new BonfireData(),
                _ => ObjectData.Empty,
            };
        }
    }

    public enum GeneratedObjects
    {
        Puzzle,
        Bonfire,
        Pillar,
        Artifact,
        AdditionalStructure,
    }
}