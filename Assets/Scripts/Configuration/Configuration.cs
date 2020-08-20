public static class Configuration
{
    #region Tile Asset Path
    public static readonly string BaseAssetsPath = "Assets";

    public static readonly string TileAssetsPath = $"{BaseAssetsPath}/Tiles";

    public static readonly string TileBaseFileSuffix = "asset";

    public static string[] TileBasePaths = new string[]
    {
        $"{TileAssetsPath}/WallTile.{TileBaseFileSuffix}",
        $"{TileAssetsPath}/CorridorTile.{TileBaseFileSuffix}",
        $"{TileAssetsPath}/RoomTile.{TileBaseFileSuffix}",
    };
    #endregion

    #region Tile Names
    public static readonly string RoomTileName = "RoomTile";
    public static readonly string CorridorTileName = "CorridorTile";
    public static readonly string WallTileName = "WallTile";
    #endregion
}
