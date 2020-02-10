# DATA

# Dungeon specific data
TILE_TYPE_COLUMN = 'Tile Type'
HEADER_LINE = 1
DUNGEON_DIMENSION = 30
TILE_TYPES = {"CORRIDOR": 0, "WALL": 1, "ROOM": 2}

# Data transformation
DATA_TRANSFORMATIONS = {0: -1, 1: 0, 2: 1}
DATA_TRANSFORMATIONS_TO_ORIGINAL = {-1: 0, 0: 1, 1: 2}

# Fuzzy logic data
FUZZY_LOGIC_TRANSFORMATIONS = {0: [0, 0.75], 1: [-0.24, 0.25],
                               2: [-0.75, 0]}
FUZZY_LOGIC_TO_ORIGINAL = [
    [-1, -0.24, 0],
    [-0.24, 0.25, 1],
    [0.25, 1, 2]
]

# Noise data
NOISE = {"min": -1, "max": 1}

# Folder paths
INPUT_FOLDER = '../../Data/Test/'
OUTPUT_FOLDER = '../../Data/GAN_Output/'
