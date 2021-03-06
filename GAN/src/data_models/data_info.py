from tensorflow.keras.optimizers import Adam, SGD, RMSprop
# DATA
DUNGEON_LABELS = 2
ONE_HOT = True

# Dungeon specific data
TILE_TYPE_COLUMN = 'Tile Type'
HEADER_LINE = 1
DUNGEON_DIMENSION = 100
TILE_TYPES = {"WALL": 0, "ROOM": 1, "CORRIDOR": 2}

# Data transformation
DATA_TRANSFORMATIONS = {0: -1, 1: 0, 2: 1}
DATA_TRANSFORMATIONS_TO_ORIGINAL = {-1: 0, 0: 1, 1: 2}

LATENT_DIM = 100

# Folder paths
INPUT_FOLDER = '../../Data/GAN_Input/'
OUTPUT_FOLDER = '../../Data/GAN_Output/'
IMAGE_FOLDER = '../../Data/Images/'
RESULTS_FOLDER = '/Results/'
MODEL_FOLDER = 'model_data/'
COM_TRAIN_FOLDER = '/com_logs/'

# GAN Variables
optimizers = {
    "adam": Adam(learning_rate=0.0002, beta_1=0.8),
    "sgd": SGD(0.01),
    "rmsprop": RMSprop(0.0002),
}

LOSS = "binary_crossentropy"
OPTIMIZER = "adam"
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

# Console output colors
class bcolors:
    HEADER = '\033[95m'
    OKBLUE = '\033[94m'
    OKGREEN = '\033[92m'
    WARNING = '\033[93m'
    FAIL = '\033[91m'
    ENDC = '\033[0m'
    BOLD = '\033[1m'
    UNDERLINE = '\033[4m'