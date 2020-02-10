public static class Configuration
{
    public static readonly string BaseAssetsPath = "Assets";

    public static readonly string TileAssetsPath = $"{BaseAssetsPath}/Tiles";

    public static readonly string TileBaseFileSuffix = "asset";

    public static string[] TileBasePaths = new string[]
    {
        $"{TileAssetsPath}/WallTile.{TileBaseFileSuffix}",
        $"{TileAssetsPath}/CorridorTile.{TileBaseFileSuffix}",
        $"{TileAssetsPath}/RoomTile.{TileBaseFileSuffix}",
    };
}
