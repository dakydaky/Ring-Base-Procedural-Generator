namespace Assets.Scripts.DataStructures.Objects
{
    /// <summary>
    ///     Base class for data stored in objects
    /// </summary>
    public class ObjectData
    {
        /// <summary>
        ///     empty object data.
        /// </summary>
        public static readonly ObjectData Empty = new ObjectData
        {
            ObjectPosX = 0,
            ObjectPosY = 0,
            ObjectType = ObjectType.None,
        };

        /// <summary>
        ///     Gets or sets the X coordinate of the object.
        /// </summary>
        /// <value>
        ///     The X coordinate of the object.
        /// </value>
        public int ObjectPosX { get; set; }

        /// <summary>
        ///     Gets or sets the Y coordinate of the object.
        /// </summary>
        /// <value>
        ///     The Y coordinate of the object.
        /// </value>
        public int ObjectPosY { get; set; }

        /// <summary>
        ///     Gets or sets the type of the Object Data.
        /// </summary>
        /// <value>
        ///     The type of the object
        /// </value>
        public ObjectType ObjectType { get; set; }
    }
}