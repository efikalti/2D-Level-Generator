import sys
import getopt
import numpy as np
import tensorflow as tf

from gan.cnn_gan import CNN_GAN
from gan.dense_gan import DENSE_GAN

# Local libraries
from data_models.data_info import ONE_HOT, bcolors
from data_io.file_reader import FileReader
from data_models.data_transform import DataTransformation

# Assert that GPU training with CUDA is enabled otherwise end with error
assert tf.test.is_gpu_available()
assert tf.test.is_built_with_cuda()

# Console output color setup
np.set_printoptions(formatter={'float': lambda x: "{0:0.3f}".format(x)})

args = {
    "model": "cnn",
    "epochs": 10000,
    "batch": 128,
    "sample": 1000,
    "generator": False
}

def train_gan(gan, data):
    
    if args["generator"] is True:
        gan.train_generator(data)
    else:
        gan.train(data)
    
    # Save results including the trained model and weights
    gan.write_results()
    gan.save_models()

# Function to create and train a cnn gan network
def train_cnn(data):
    # Create network with the provided parameters
    gan = CNN_GAN("cnn", epochs=args["epochs"], batch_size=args["batch"],
                  sample_interval=args["sample"],
                  one_hot_enabled=ONE_HOT)
    train_gan(gan, data)


# Function to create and train a dense gan network
def train_dense(data):
    # Create network with the provided parameters
    gan = DENSE_GAN("dense", epochs=args["epochs"], batch_size=args["batch"],
                    sample_interval=args["sample"],
                    one_hot_enabled=ONE_HOT)
    
    train_gan(gan, data)


# Function used to parse the cmd arguments
def parse_args(argv):
    help_message = str(f"run.py -m {bcolors.OKBLUE}<dense/cnn>{bcolors.ENDC} -e {bcolors.OKBLUE}<epochs>{bcolors.ENDC} -b {bcolors.OKBLUE}<batch size>{bcolors.ENDC} "
                       + f"-s {bcolors.OKBLUE}<sample interval>{bcolors.ENDC}")
    try:
        opts, args_list = getopt.getopt(argv, "he:b:s:m:",
                                   ["epochs=", "batch_size=",
                                    "sample_interval=", "model="])
    except getopt.GetoptError:
        print(help_message)
        sys.exit(2)
    for opt, arg in opts:
        if opt == '-h':
            print(help_message)
            sys.exit()
        elif opt in ("-m", "--model"):
            args["model"] = arg
        elif opt in ("-e", "--epochs"):
            args["epochs"] = int(arg)
        elif opt in ("-b", "--batch"):
            args["batch"] = int(arg)
        elif opt in ("-s", "--sample"):
            args["sample"] = int(arg)
        elif opt in ("--generator"):
            args["generator"] = True
    print(str(f"Running model {bcolors.OKGREEN}%s{bcolors.ENDC} for {bcolors.OKGREEN}%s{bcolors.ENDC} epochs with batch size {bcolors.OKGREEN}%s{bcolors.ENDC} and "
              + f"sample interval {bcolors.OKGREEN}%s{bcolors.ENDC}") %
          (args["model"], args["epochs"],
           args["batch"], args["sample"]))
    return args


def main(argv):
    # Parse cmd arguments
    args = parse_args(argv)

    # FileParser object to load input data and write results
    file_reader = FileReader()
    # Load csv data into array
    data = file_reader.get_csv_data()

    # Transform data
    data_transformation = DataTransformation(one_hot_enabled=ONE_HOT)
    data = data_transformation.transform_multiple(data)

    # Transform data to matrix, cnn expects a ndim matrix of data not an array
    transformed_data = []
    for sample in data:
        matrix = file_reader.data_transformation.transform_to_matrix(sample)
        transformed_data.append(matrix)

    if (args["model"] == "dense"):
        # Train dense model
        train_dense(transformed_data)
    elif (args["model"] == "cnn"):
        # Train cnn model
        train_cnn(transformed_data)


if __name__ == "__main__":
    main(sys.argv[1:])
