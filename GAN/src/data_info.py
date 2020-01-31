TILE_TYPE_COLUMN = 'Tile Type'
HEADER_LINE = 1
DUNGEON_DIMENSION = 30
TILE_TYPES = {"CORRIDOR": 0, "WALL": 1, "ROOM": 2}
DATA_TRANSFORMATIONS = {0: -1, 1: 0, 2: 1}
DATA_TRANSFORMATIONS_TO_ORIGINAL = {-1: 0, 0: 1, 1: 2}
FUZZY_LOGIC_TRANSFORMATIONS = {0: [0, 0.75], 1: [-0.24, 0.25],
                               2: [-0.75, 0]}
FUZZY_LOGIC_TO_ORIGINAL = [
    [-1, -0.24, -1],
    [-0.24, 0.25, 0],
    [0.25, 1, 1]
]
