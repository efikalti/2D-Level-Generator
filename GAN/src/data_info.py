from tensorflow.keras.optimizers import Adam, SGD, RMSprop
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
INPUT_FOLDER = '../../Data/GAN_Input/'
OUTPUT_FOLDER = '../../Data/GAN_Output/'
IMAGE_FOLDER = '../../Data/Images/'
RESULTS_FOLDER = '/Results/'

MODEL_FOLDER = 'model_data/'

# GAN Variables
optimizers = {
    "adam": Adam(0.0002),
    "sgd": SGD(0.0002),
    "rmsprop": RMSprop(0.0002),
}

DIS_LOSS = "binary_crossentropy"
GEN_LOSS = "mean_squared_error"
COM_LOSS = "binary_crossentropy"

OPTIMIZER = "sgd"

METRIC = 'accuracy'

# Plot variables
colors = {
    'brown': {
        'hex': '#B15928',
        'rgb': [177,89,40],
        'float': [0.6941176470588235, 0.3490196078431373, 0.1568627450980392]
    },
    'white': {
        'hex': '#f2f2f2',
        'rgb': [242,242,242],
        'float': [0.9490196078431373, 0.9490196078431373, 0.9490196078431373]
    },
    'orange': {
        'hex': '#fed9a6',
        'rgb': [254,217,166],
        'float': [0.996078431372549, 0.8509803921568627, 0.6509803921568627]
    },
    'black': {
        'hex': '#000000',
        'rgb': [0, 0, 0],
        'float': [0, 0, 0]
    },
}