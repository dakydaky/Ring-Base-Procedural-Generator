using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.DataStructures.Biomes;
using Assets.Scripts.DataStructures.Objects;
using Assets.Scripts.DataStructures.Objects.Interactive;

namespace Assets.Scripts.DataStructures
{
    public class MapData
    {
        public MapData(int columns, int rows)
        {
            Columns = columns;
            Rows = rows;
            for (var y = 0; y < rows; y++)
            {
                var row = new List<TileData>();
                Map.Add(row);
                for (var x = 0; x < columns; x++)
                {
                    row.Add(new TileData());
                }
            }
        }

        /// <summary>
        ///     Gets or sets 3D list keeping TileData elements.
        /// </summary>
        /// <value>
        ///     The map.
        /// </value>
        public List<List<TileData>> Map { get; set; } = new List<List<TileData>>();

        /// <summary>
        ///     Gets or sets the columns.
        /// </summary>
        /// <value>
        ///     The columns.
        /// </value>
        public int Columns { get; set; }

        /// <summary>
        ///     Gets or sets the rows.
        /// </summary>
        /// <value>
        ///     The rows.
        /// </value>
        public int Rows { get; set; }

        /// <summary>
        ///     Gets or sets the list of <see cref="ObjectData" /> at the specified column and row.
        /// </summary>
        /// <value>
        ///     The <see cref="ObjectData" />.
        /// </value>
        /// <param name="coordX">The X coordinate in tile grid.</param>
        /// <param name="coordY">The Y coordinate in tile grid.</param>
        public List<ObjectData> this[int coordX, int coordY]
        {
            get => Map[coordX][coordY].ObjectDataList;
            set
            {
                value.ForEach(val => val.ObjectPosX = coordX);
                value.ForEach(val => val.ObjectPosY = coordY);
                Map[coordX][coordY].ObjectDataList.AddRange(value);
            }
        }

        /// <summary>
        ///    Replaces the <see cref="ObjectData" /> at the specified column, row and index of the map.
        /// </summary>
        /// <param name="x">The X coordinate in tile grid.</param>
        /// <param name="y">The Y coordinate in tile grid.</param>
        /// <param name="z">The Z coordinate in tile grid.</param>
        /// <param name="objectData">The object data</param>
        public void ReplaceObjectData(int x, int y, int z, ObjectData objectData)
        {
            objectData.ObjectPosX = x;
            objectData.ObjectPosY = y;
            Map[x][y].ObjectDataList[z] = objectData;
        }

        public BiomeType GetBiomeType(int x, int y) => Map[x][y].BiomeType;

        /// <summary>
        ///   Counts the total amount of Lumber data present on the map.
        /// </summary>
        /// <returns> A count of Lumber placed</returns>
        public int GetLumberCount()
        {
            var lumberCount = 0;
            for (var x = 0; x < Rows; x++)
            {
                for (var y = 0; y < Columns; y++)
                {
                    lumberCount += (from objectData in Map[x][y].ObjectDataList where objectData.ObjectType == ObjectType.Interactive
                        select (InteractiveObjectData) objectData).Count(interactiveObj => interactiveObj.InteractiveObjectType == InteractiveObjectType.Log);
                }
            }
            return lumberCount;
        }
    }

    public class TileData
    {
        public TileData()
        {
            BiomeType = BiomeType.None;
            ObjectDataList = new List<ObjectData>();
        }

        public BiomeType BiomeType { get; set; }
        public List<ObjectData> ObjectDataList { get; }
    }
}