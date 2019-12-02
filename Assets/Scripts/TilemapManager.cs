using Assets.Scripts;
using Assets.Scripts.Enums;
using Assets.Scripts.Models;
using Assets.Scripts.Models.DataStructures;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TilemapManager : MonoBehaviour
{
    public BoundsInt tilemapBounds;
    public TileItem[] TilesArray;

    public int numberOfRooms = 5;

    private Tilemap tilemap;
    private TilemapHelper tilemapHelper;

    private readonly Dictionary<TRANFROM_RULE, ITransformRule> rules = new Dictionary<TRANFROM_RULE, ITransformRule>();

    private readonly int sideSize = 30;
    private readonly int neighborOffset = 1;
    private readonly int neighborhoodSize = 3;

    private TileItem defaultTile;

    private System.Random randomGenerator = new System.Random();

    private Graph TilemapGraph;

    private const string defaultNodeName = "node";

    void Start()
    {
        Setup();
    }

    void Update()
    {
        
    }

    public void Setup()
    {
        // Create tilemap if it does not exist
        tilemap = GetComponent<Tilemap>();
        if (tilemap == null)
        {
            tilemap = transform.gameObject.AddComponent<Tilemap>();
            transform.gameObject.AddComponent<TilemapRenderer>();
        }
        // Initialize TilemapHelper object
        tilemapHelper = new TilemapHelper(TilesArray);
        // Set tilemap bounds object to the value of sideSize x  sideSize x 0
        tilemapBounds = new BoundsInt(Vector3Int.zero, new Vector3Int(sideSize, sideSize, 0));

        // Initialize and fill the rules dictionary
        rules.Add(TRANFROM_RULE.WALL_FROM_BOUNDS, new TransformToTileFromBounds(TILE_TYPE.WALL));
        rules.Add(TRANFROM_RULE.WALL_FROM_ADJACENTS, new TransformToWallFromAdjacents());
        rules.Add(TRANFROM_RULE.WALL_FOR_ROOM, new TransformToWallForRoom());
        rules.Add(TRANFROM_RULE.FLOOR_FROM_ADJACENTS, new TransformToRoomFromAdjacents());
        rules.Add(TRANFROM_RULE.FLOOR_FROM_BOUNDS, new TransformToTileFromBounds(TILE_TYPE.CORRIDOR));

        foreach (var tile in TilesArray)
        {
            if (tile.TileType == TILE_TYPE.ROOM_1)
            {
                defaultTile = tile;
            }
        }

        // Create Graph object
        TilemapGraph = new Graph();

        GenerateLevel();
    }

    public void GenerateLevel()
    {
        // Step 1. Fill area with tiles
        FillAreaWithTile(tilemapBounds, defaultTile.Tile);
        tilemapBounds = tilemap.cellBounds;

        // Step 2. Transform tiles at bounds to walls
        TransformTilemapArea(tilemapBounds, new TRANFROM_RULE[] { TRANFROM_RULE.WALL_FROM_BOUNDS });

        // Step 3. Transform tiles at next to walls to corridors
        var corridorBounds = new BoundsInt(tilemapBounds.xMin + 1, tilemapBounds.yMin + 1, 0, tilemapBounds.xMax - 2, tilemapBounds.yMax - 2, 0);
        TransformTilemapArea(corridorBounds, new TRANFROM_RULE[] { TRANFROM_RULE.FLOOR_FROM_BOUNDS });

        // Step 3. Create rooms for this tilemap
        var rooms = CalculateRooms();

        // Step 4. Add the rooms in the tilemap
        AddRoomsToMap(rooms);

        // Step 5. Add walls to rooms
        TransformTilemapArea(tilemapBounds, new TRANFROM_RULE[] { TRANFROM_RULE.WALL_FOR_ROOM });

        // Step 6. Map tilemap to graph
        TilemapGraph = MapTilemapToGraph();
    }

    /// <summary>
    /// Fill the bounded tilemap area with random tiles
    /// The tiles are randomly selected according to the frequency of each tile type
    /// </summary>
    /// <param name="bounds">The bounded tilemap area to fill with random tiles</param>
    public void AddRandomTiles(BoundsInt bounds)
    {
        TileBase tile;
        for (int x = 0; x < bounds.xMax; x++)
        {
            for (int y = 0; y < bounds.yMax; y++)
            {
                tile = tilemapHelper.GetRandomTile();

                if (tile != null)
                {
                    tilemap.SetTile(new Vector3Int(x, y, 0), tile);
                }
            }
        }
    }

    /// <summary>
    /// Fill the bounded tilemap area with one type of tile
    /// </summary>
    /// <param name="bounds">The bounded tilemap area to fill with this tile</param>
    public void FillAreaWithTile(BoundsInt bounds, TileBase tile)
    {
        for (int x = bounds.xMin; x < bounds.xMax; x++)
        {
            for (int y = bounds.yMin; y < bounds.yMax; y++)
            {
                tilemap.SetTile(new Vector3Int(x, y, 0), tile);
            }
        }
    }

    /// <summary>
    /// Apply all the rules supplied in the arguments to the tile in this position
    /// </summary>
    /// <param name="tile">The tile we want to apply the rules</param>
    /// <param name="position">The position of the tile on the tilemap</param>
    /// <returns></returns>
    public TileBase ApplyRules(TileBase tile, Vector3Int position, TRANFROM_RULE[] rules, BoundsInt bounds)
    {
        var neighbors = GetTileNeighbors(position);
        TileBase newTile;
        foreach (TRANFROM_RULE rule in rules)
        {
            newTile = ApplyRule(rule, position, neighbors, bounds);
            if (newTile != null)
            {
                return newTile;
            }
        }
        return tile;
    }

    /// <summary>
    /// Apply a specific transformation rule to a specific tile
    /// </summary>
    /// <param name="rule">The TRANFROM_RULE to be applied to this tile</param>
    /// <param name="position">The position of the tile on the tilemap</param>
    /// <param name="neighbors">the tile's neighborhood, the 3x3 area around the tile as a 1D array</param>
    /// <returns></returns>
    public TileBase ApplyRule(TRANFROM_RULE rule, Vector3Int position, TileBase[] neighbors, BoundsInt bounds)
    {
        if (rule == TRANFROM_RULE.WALL_FROM_BOUNDS)
        {
            var newTileType = rules[TRANFROM_RULE.WALL_FROM_BOUNDS].Apply(position, bounds);
            if (newTileType != null)
            {
                return tilemapHelper.GetTileByType(newTileType);
            }
        }
        if (rule == TRANFROM_RULE.WALL_FROM_ADJACENTS)
        {
            var newTileType = rules[TRANFROM_RULE.WALL_FROM_ADJACENTS].Apply(neighbors);
            if (newTileType != null)
            {
                return tilemapHelper.GetTileByType(newTileType);
            }
        }
        if (rule == TRANFROM_RULE.WALL_FOR_ROOM)
        {
            var newTileType = rules[TRANFROM_RULE.WALL_FOR_ROOM].Apply(neighbors);
            if (newTileType != null)
            {
                return tilemapHelper.GetTileByType(newTileType);
            }
        }
        if (rule == TRANFROM_RULE.FLOOR_FROM_ADJACENTS)
        {
            var newTileType = rules[TRANFROM_RULE.FLOOR_FROM_ADJACENTS].Apply(neighbors);
            if (newTileType != null)
            {
                return tilemapHelper.GetTileByType(newTileType);
            }
        }
        if (rule == TRANFROM_RULE.FLOOR_FROM_BOUNDS)
        {
            var newTileType = rules[TRANFROM_RULE.FLOOR_FROM_BOUNDS].Apply(position, bounds);
            if (newTileType != null)
            {
                return tilemapHelper.GetTileByType(newTileType);
            }
        }
        return null;
    }

    /// <summary>
    /// Transform the area that is inside the provided bounds in the tilemap by applying the list of rules provided
    /// </summary>
    /// <param name="bounds">The bounded area of the tilemap to be transformed</param>
    /// <param name="rules">The list of rules to be applied to all the tiles in the bounded area</param>
    public void TransformTilemapArea(BoundsInt bounds, TRANFROM_RULE[] rules)
    {
        TileBase currentTile, newTile;

        Vector3Int position = new Vector3Int();
        for (int x = bounds.xMin; x < bounds.xMax; x++)
        {
            for (int y = bounds.yMin; y < bounds.yMax; y++)
            {
                position.x = x;
                position.y = y;
                currentTile = tilemap.GetTile(position);
                if (currentTile != null)
                {
                    newTile = ApplyRules(currentTile, position, rules, bounds);
                    if (newTile.name != currentTile.name)
                    {
                        tilemap.SetTile(new Vector3Int(x, y, 0), newTile);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Get the neighborhood of a specific tile
    /// A neighborhood is an array with 9 elements that correspond to the 3x3 area around a tile
    /// </summary>
    /// <param name="position">Position of the center tile</param>
    /// <returns></returns>
    public TileBase[] GetTileNeighbors(Vector3Int position)
    {
        var neighborBounds = new BoundsInt(position.x - neighborOffset, position.y - neighborOffset, position.z, neighborhoodSize, neighborhoodSize, 1);
        //PrintTileNeighborhood(position, tilemap.GetTilesBlock(neighborBounds));
        return tilemap.GetTilesBlock(neighborBounds);
    }

    public List<RoomsList> CalculateRooms()
    {
        Room room;
        int wallTiles = 2;
        int outerCorridorTiles = 2;

        // create a double list to keep the rooms of the dungeon
        List<RoomsList> dungeonRooms = new List<RoomsList>();

        // Remove 2 tiles for the outer walls and 2 tiles for the surrounding corridors
        int availableHeight = tilemapBounds.yMax - wallTiles - outerCorridorTiles;
        int availableWidth = tilemapBounds.xMax - wallTiles - outerCorridorTiles;

        while (availableHeight > 0)
        {
            // Initialize variables
            int remainingTilesForWidth = 0;
            int remainingTilesForHeight = 0;
            RoomSizeTransformations sizeTransformationForWidth;
            RoomSizeTransformations sizeTransformationForHeight;

            int availableTiles = availableHeight;

            // Create the range for rooms, it is calculated as (1/4 - 1/2) of the available tiles
            int minNumberOfRooms = Math.Max(availableTiles / 10, 1);
            int maxNumberOfRooms = Math.Max(availableTiles / 5, 1);

            int maxHeight = 0;

            // create a double list to keep the size of each room for this height
            RoomsList roomList = new RoomsList();

            // Pick a random number in the room range, upper bound is exclusive so add one to take the correct range
            int numberOfRooms = randomGenerator.Next(minNumberOfRooms, maxNumberOfRooms + 1);
            Debug.Log($"Number of rooms: {numberOfRooms}");

            // We require n-1 corridors between rooms, where n is the number of rooms
            int corridors = numberOfRooms - 1;

            // Remove corridors from the number of available tiles
            availableTiles -= corridors;

            // Calculate the available tile number per room if all rooms are the same size
            int availableTilesPerRoom = 0;
            if (numberOfRooms > 0)
            {
                availableTilesPerRoom = availableTiles / numberOfRooms;
            }

            // Calculate each room, add/subtract/leave as is from each room and then shuffle them randomly
            for (int i = 0; i < numberOfRooms; i++)
            {
                room = new Room(availableTilesPerRoom, availableTilesPerRoom);
                // Calculate the dimension transformations for this room width and height
                sizeTransformationForWidth = GetRoomDimensionTransformation(remainingTilesForWidth, i, numberOfRooms);
                sizeTransformationForHeight = GetRoomDimensionTransformation(remainingTilesForHeight, i, numberOfRooms);

                // Calculate the new dimension for the room according to the selected transformation
                room.Width = CalculateRoomDimension(sizeTransformationForWidth, availableTilesPerRoom, remainingTilesForWidth, out int newRemainingTilesForWidth);
                remainingTilesForWidth = newRemainingTilesForWidth;

                room.Height = CalculateRoomDimension(sizeTransformationForHeight, availableTilesPerRoom, remainingTilesForHeight, out int newRemainingTilesForHeight);
                remainingTilesForHeight = newRemainingTilesForHeight;

                // Update max height
                if (room.Height > maxHeight)
                {
                    maxHeight = room.Height;
                }

                // Add room to the list of rooms
                roomList.Rooms.Add(room);
            }
            // Remove max height and one more for the intermediate corridor
            availableHeight -= maxHeight + 1;

            // Suffle rooms
            roomList.Rooms.OrderBy(a => randomGenerator.Next());
            roomList.MaxHeight = maxHeight;
            roomList.MaxWidth = availableWidth;
            dungeonRooms.Add(roomList);
        }

        return dungeonRooms;
    }

    public void AddRoomsToMap(List<RoomsList> rooms)
    {
        // Get tilebase tiles for the room and the corridor
        TileBase roomTile = tilemapHelper.GetTileByType(TILE_TYPE.ROOM_1);
        TileBase corridorTile = tilemapHelper.GetTileByType(TILE_TYPE.CORRIDOR);

        BoundsInt corridorBounds;

        int roomY = 2;
        foreach(var roomList in rooms)
        {
            int roomX = 2;
            foreach(var room in roomList.Rooms)
            {
                var roomBounds = new BoundsInt(roomX, roomY, 0, room.Width, room.Height, 1);
                FillAreaWithTile(roomBounds, roomTile);

                corridorBounds = new BoundsInt(roomX + room.Width, roomY, 0, 1, roomList.MaxHeight, 1);
                FillAreaWithTile(corridorBounds, corridorTile);

                roomX += room.Width + 1;
            }
            roomY += roomList.MaxHeight;

            corridorBounds = new BoundsInt(2, roomY, 0, roomList.MaxWidth, 1, 0);
            roomY++;
            Debug.Log($"roomList.MaxWidth: {roomList.MaxWidth}");
            FillAreaWithTile(corridorBounds, corridorTile);
        }
    }

    public RoomSizeTransformations GetRoomDimensionTransformation(int remainingTiles, int currentRoomIndex, int numberOfRooms)
    {
        if (remainingTiles > 0)
        {
            return RoomSizeTransformations.ADD;
        }
        else if (remainingTiles < 0)
        {
            return RoomSizeTransformations.REMOVE;
        }
        else if (currentRoomIndex + 1 < numberOfRooms)
        {
            return (RoomSizeTransformations)randomGenerator.Next(0, 3);
        }
        return RoomSizeTransformations.DO_NOTHING;
    }


    public int CalculateRoomDimension(RoomSizeTransformations transformation, int availableTiles, int remainingTiles, out int newRemainingTiles)
    {
        int tilesToReassign;    
        newRemainingTiles = remainingTiles;
        switch (transformation)
        {
            case RoomSizeTransformations.ADD:
                tilesToReassign = Math.Max(((availableTiles) * 10) / 100, 1);
                availableTiles += tilesToReassign;
                newRemainingTiles -= tilesToReassign;
                break;
            case RoomSizeTransformations.REMOVE:
                tilesToReassign = Math.Max((((availableTiles) * 10) / 100), 1);
                availableTiles -= tilesToReassign;
                newRemainingTiles += tilesToReassign;
                break;
            case RoomSizeTransformations.DO_NOTHING:
            default:
                break;
        }
        return availableTiles;
    }

    public Graph MapTilemapToGraph()
    {
        var graph = new Graph();

        BoundsInt bounds = tilemap.cellBounds;
        TileBase currentTile;

        Vector3Int position = new Vector3Int();
        Node node;
        for (int x = bounds.xMin; x < bounds.xMax; x++)
        {
            for (int y = bounds.yMin; y < bounds.yMax; y++)
            {
                position.x = x;
                position.y = y;
                currentTile = tilemap.GetTile(position);
                if (currentTile != null)
                {
                    // Create node for current tile
                    node = new Node(currentTile.name, tilemapHelper.GetTileTypeFromSpriteName(currentTile.name));
                    // Add all neighboring tiles as links for this node
                    var neighbors = GetTileNeighbors(position);
                    foreach(var neighbor in neighbors)
                    {
                        if (neighbor != null)
                        {
                            node.AddLink(new Node(neighbor.name, tilemapHelper.GetTileTypeFromSpriteName(neighbor.name)));
                        }
                    }
                    // Add node with its links to graph
                    graph.AddNode(node);
                }
            }
        }

        return graph;
    }
}
