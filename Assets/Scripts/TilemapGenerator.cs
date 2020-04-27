using Assets.Scripts.Controllers.TilemapController;
using Assets.Scripts.Enums;
using Assets.Scripts.Models;
using Assets.Scripts.Models.DataStructures;
using Assets.Scripts.Models.Evaluation.CA;
using Assets.Scripts.Reporting;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Assets.Scripts
{
    public class TilemapGenerator : TilemapControllerBase
    {
        public bool IsEvaluationEnabled = false;

        private BoundsInt tilemapBounds;

        private readonly Dictionary<TransformRule, ITransformRule> rules = new Dictionary<TransformRule, ITransformRule>();

        private TileItem defaultTile;

        public int numberOfRooms = 5;

        private readonly int sideSize = 30;
        private readonly int neighborOffset = 1;
        private readonly int neighborhoodSize = 3;

        void Start()
        {
            SetupTilemapGeneration();
        }

        void Update()
        {

        }


        public void SetupTilemapGeneration()
        {
            // Create tilemap if it does not exist
            tilemap = GetComponent<Tilemap>();
            if (tilemap == null)
            {
                tilemap = transform.gameObject.AddComponent<Tilemap>();
                transform.gameObject.AddComponent<TilemapRenderer>();
            }

            // Set tilemap bounds object to the value of sideSize x  sideSize x 0
            tilemapBounds = new BoundsInt(Vector3Int.zero, new Vector3Int(sideSize, sideSize, 0));

            // Initialize and fill the rules dictionary
            rules.Add(TransformRule.WALL_FROM_BOUNDS, new TransformToTileFromBounds(TileType.WALL));
            rules.Add(TransformRule.WALL_FROM_ADJACENTS, new TransformToWallFromAdjacents());
            rules.Add(TransformRule.WALL_FOR_ROOM, new TransformToWallForRoom());
            rules.Add(TransformRule.ROOM_FROM_ADJACENTS, new TransformToRoomFromAdjacents());
            rules.Add(TransformRule.FLOOR_FROM_BOUNDS, new TransformToTileFromBounds(TileType.CORRIDOR));

            defaultTile = TilemapHelper.GetDefaultTile();

            // Create DataParser object
            fileParser = new DataParser();
        }

        public override void GenerateLevel()
        {
            // Step 1. Fill area with tiles
            FillAreaWithTile(tilemapBounds, defaultTile.Tilebase);
            tilemapBounds = tilemap.cellBounds;

            // Step 2. Transform tiles at bounds to walls
            TransformTilemapArea(tilemapBounds, new TransformRule[] { TransformRule.WALL_FROM_BOUNDS });

            // Step 3. Transform tiles at next to walls to corridors
            var corridorBounds = new BoundsInt(tilemapBounds.xMin + 1, tilemapBounds.yMin + 1, 0, tilemapBounds.xMax - 2, tilemapBounds.yMax - 2, 0);
            TransformTilemapArea(corridorBounds, new TransformRule[] { TransformRule.FLOOR_FROM_BOUNDS });

            // Step 3. Create rooms for this tilemap
            var rooms = CalculateRooms();

            // Step 4. Add the rooms in the tilemap
            AddRoomsToMap(rooms);

            // Step 5. Add walls to rooms
            TransformTilemapArea(tilemapBounds, new TransformRule[] { TransformRule.WALL_FOR_ROOM });

            // Step 6. Write tilemap to file
            fileParser.WriteTilemap(tilemap);

            // Step 7. Evaluate generated Level (Optional, default is disabled)
            if (IsEvaluationEnabled)
            {
                var evaluationRulesCA = new EvaluationRulesCA(tilemap);
                evaluationRulesCA.EvaluateCALevel();
            }
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
                    tile = TilemapHelper.GetRandomTile();

                    if (tile != null)
                    {
                        tilemap.SetTile(new Vector3Int(x, y, 0), tile);
                    }
                }
            }
        }


        /// <summary>
        /// Apply all the rules supplied in the arguments to the tile in this position
        /// </summary>
        /// <param name="tile">The tile we want to apply the rules</param>
        /// <param name="position">The position of the tile on the tilemap</param>
        /// <returns></returns>
        public TileBase ApplyRules(TileBase tile, Vector3Int position, TransformRule[] rules, BoundsInt bounds)
        {
            var neighbors = GetTileNeighbors(position);
            TileBase newTile;
            foreach (TransformRule rule in rules)
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
        public TileBase ApplyRule(TransformRule rule, Vector3Int position, TileBase[] neighbors, BoundsInt bounds)
        {
            if (rule == TransformRule.WALL_FROM_BOUNDS)
            {
                var newTileType = rules[TransformRule.WALL_FROM_BOUNDS].Apply(position, bounds);
                if (newTileType != null)
                {
                    return TilemapHelper.GetTileByType(newTileType);
                }
            }
            if (rule == TransformRule.WALL_FROM_ADJACENTS)
            {
                var newTileType = rules[TransformRule.WALL_FROM_ADJACENTS].Apply(neighbors);
                if (newTileType != null)
                {
                    return TilemapHelper.GetTileByType(newTileType);
                }
            }
            if (rule == TransformRule.WALL_FOR_ROOM)
            {
                var newTileType = rules[TransformRule.WALL_FOR_ROOM].Apply(neighbors);
                if (newTileType != null)
                {
                    return TilemapHelper.GetTileByType(newTileType);
                }
            }
            if (rule == TransformRule.ROOM_FROM_ADJACENTS)
            {
                var newTileType = rules[TransformRule.ROOM_FROM_ADJACENTS].Apply(neighbors);
                if (newTileType != null)
                {
                    return TilemapHelper.GetTileByType(newTileType);
                }
            }
            if (rule == TransformRule.FLOOR_FROM_BOUNDS)
            {
                var newTileType = rules[TransformRule.FLOOR_FROM_BOUNDS].Apply(position, bounds);
                if (newTileType != null)
                {
                    return TilemapHelper.GetTileByType(newTileType);
                }
            }
            return null;
        }

        /// <summary>
        /// Transform the area that is inside the provided bounds in the tilemap by applying the list of rules provided
        /// </summary>
        /// <param name="bounds">The bounded area of the tilemap to be transformed</param>
        /// <param name="rules">The list of rules to be applied to all the tiles in the bounded area</param>
        public void TransformTilemapArea(BoundsInt bounds, TransformRule[] rules)
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
            TileBase roomTile = TilemapHelper.GetTileByType(TileType.ROOM);
            TileBase corridorTile = TilemapHelper.GetTileByType(TileType.CORRIDOR);

            BoundsInt corridorBounds;

            int roomY = 2;
            foreach (var roomList in rooms)
            {
                int roomX = 2;
                foreach (var room in roomList.Rooms)
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
                        node = new Node(currentTile.name, new Vector3Int(x, y, 0), TilePositions.MIDDLE, TilemapHelper.GetTileTypeFromSpriteName(currentTile.name));
                        
                        // Add all neighboring tiles as links for this node
                        var neighbors = GetTileNeighbors(position);
                        for(int index = 0; index < neighbors.Length; index++)
                        {
                            var neighbor = neighbors[index];
                            if (neighbor != null)
                            {
                                node.AddLink(new Node(neighbor.name, Vector3Int.zero, (TilePositions)index, TilemapHelper.GetTileTypeFromSpriteName(neighbor.name)));

                            }
                        }

                        //foreach (var neighbor in neighbors)
                        //{
                        //    if (neighbor != null)
                        //    {
                        //        node.AddLink(new Node(neighbor.name, tilemapHelper.GetTileTypeFromSpriteName(neighbor.name)));
                        //    }
                        //}
                        // Add node with its links to graph
                        graph.AddNode(node);
                    }
                }
            }

            return graph;
        }
    }
}
