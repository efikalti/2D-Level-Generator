import numpy as np
import sys
import getopt

from data_io.file_reader import FileReader
from gan import DENSE_GAN
from gan_cnn import GAN_CNN
from data_transform import DataTransformation

class bcolors:
    HEADER = '\033[95m'
    OKBLUE = '\033[94m'
    OKGREEN = '\033[92m'
    WARNING = '\033[93m'
    FAIL = '\033[91m'
    ENDC = '\033[0m'
    BOLD = '\033[1m'
    UNDERLINE = '\033[4m'
#print(f"{bcolors.HEADER}HEADER{bcolors.ENDC}")

np.set_printoptions(formatter={'float': lambda x: "{0:0.3f}".format(x)})

# FileParser object to load input data and write results
file_reader = FileReader()

TRANFORM = False
FUZZY = False


# Function to create and train a cnn gan network
def train_cnn(data, args):
    # Transform data to matrix, cnn expects a ndim matrix of data not an array
    transformed_data = []
    for sample in data:
        matrix = file_reader.data_transformation.transform_to_matrix(sample)
        transformed_data.append(matrix)

    # Create network with the provided parameters
    gan = GAN_CNN(epochs=args["epochs"], batch_size=args["batch"],
                  sample_interval=args["sample"], d_trainable=True,
                  transform=TRANFORM)
    gan.train_generator(transformed_data)

    gan.train(transformed_data)

    # Save results including the trained model and weights
    gan.write_results()
    gan.save_models()


# Function to create and train a dense gan network
def train_dense(data, args):
    # Create network with the provided parameters
    gan = DENSE_GAN(epochs=args["epochs"], batch_size=args["batch"],
                    sample_interval=args["sample"], d_trainable=True,
                    transform=TRANFORM)
    gan.train_generator(data)

    gan.train(data)

    # Save results including the trained model and weights
    gan.write_results()
    gan.save_models()


# Function used to parse the cmd arguments
def parse_args(argv):
    args_dict = {
        "model": "dense",
        "epochs": 10000,
        "batch": 64,
        "sample": 1000
    }
    help_message = str(f"run.py -m {bcolors.OKBLUE}<dense/cnn>{bcolors.ENDC} -e {bcolors.OKBLUE}<epochs>{bcolors.ENDC} -b {bcolors.OKBLUE}<batch size>{bcolors.ENDC} "
                       + f"-s {bcolors.OKBLUE}<sample interval>{bcolors.ENDC}")
    try:
        opts, args = getopt.getopt(argv, "he:b:s:m:",
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
            args_dict["model"] = arg
        elif opt in ("-e", "--epochs"):
            args_dict["epochs"] = int(arg)
        elif opt in ("-b", "--batch"):
            args_dict["batch"] = int(arg)
        elif opt in ("-s", "--sample"):
            args_dict["sample"] = int(arg)
    print(str(f"Running model {bcolors.OKGREEN}%s{bcolors.ENDC} for {bcolors.OKGREEN}%s{bcolors.ENDC} epochs with batch size {bcolors.OKGREEN}%s{bcolors.ENDC} and "
              + f"sample interval {bcolors.OKGREEN}%s{bcolors.ENDC}") %
          (args_dict["model"], args_dict["epochs"],
           args_dict["batch"], args_dict["sample"]))
    return args_dict


def main(argv):
    # Parse cmd arguments
    args = parse_args(argv)

    # Load csv data into array
    data = file_reader.get_csv_data()

    # Transform data
    data_transformation = DataTransformation(transform=TRANFORM, fuzzy=FUZZY)
    data = data_transformation.transform_multiple(data)

    if (args["model"] == "dense"):
        # Train dense model
        train_dense(data, args)
    elif (args["model"] == "cnn"):
        # Train cnn model
        train_cnn(data, args)


if __name__ == "__main__":
    main(sys.argv[1:])
