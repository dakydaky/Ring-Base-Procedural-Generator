using Assets.Scripts.DataStructures.Biomes;
using UnityEngine;

public class TileMap
{
    private readonly float _rowSize;
    private readonly float _columnSize;
    private readonly float _columnCenter;
    private readonly float _rowCenter;
    private readonly Vector3 _origin;

    public TileMap(int columns, int rows, Terrain ground)
    {
        Columns = columns;
        Rows = rows;
        Ground = ground;
        var size = Ground.terrainData.size;
        _columnSize = size.x / Columns;
        _rowSize = size.z / Rows;
        _origin = Ground.transform.position;
        _rowCenter = _rowSize / 2;
        _columnCenter = _columnSize / 2;
    }

    public Terrain Ground { get; }

    public int Columns { get; set; }

    public int Rows { get; set; }

    public Vector3 this[int column, int row] => GetCoordinates(column, row);

    private Vector3 GetCoordinates(int column, int row)
    {
        var offset = new Vector3((_columnSize * column) + _columnCenter, 0, (_rowSize * row) + _rowCenter);
        return _origin + offset;
    }
}
