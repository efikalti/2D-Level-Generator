import os
import random
import string
from datetime import datetime
from data_transform import DataTransformation

import pandas as pd
import numpy as np

import data_info


class FileWriter:
    def __init__(self, transform=True):
        # Set input path from global settings - data_info
        self.results_folder = data_info.RESULTS_FOLDER

        # Variables used to write the generated results in files
        self.dungeon_dimension = data_info.DUNGEON_DIMENSION
        self.number_of_lines = self.dungeon_dimension * self.dungeon_dimension
        self.positions_array = np.empty([self.number_of_lines, 2], dtype=int)

        # Setup positions array, used when writing the gan generated results
        self.setup_position_array()

        # Instantiate DataTransformation object
        self.data_transformation = DataTransformation(transform=transform)

    # Create the output folder and subfolder to store all the result files
    def create_output_folder(self, folder_name="folder-"):
        # Get datetime to add to the folder name
        now = datetime.now()
        dt_string = now.strftime("%d_%m_%Y_%H_%M_%S")

        # Create and store the path of the output folder
        self.output_path = str(data_info.OUTPUT_FOLDER + folder_name
                               + dt_string + "/")
        self.create_folder(self.output_path)

        # Create folder to store the image results
        self.image_folder = self.output_path + "/Images/"
        self.create_folder(self.image_folder)

        # Create folder to store the trained models
        model_path = self.output_path + data_info.MODEL_FOLDER
        self.create_folder(model_path)

        # Create folder to store the generated dungeon files
        results_path = self.output_path + data_info.RESULTS_FOLDER
        self.create_folder(results_path)

    # Function to create a folder if id does not exist
    def create_folder(self, folder_path):
        if not os.path.exists(folder_path):
            os.makedirs(folder_path)

    # Function to create a new file to store one gan generated dungeon
    def get_new_file(self, file_prefix=""):
        path = self.output_path \
            + self.results_folder \
            + file_prefix \
            + datetime.now().strftime("%d-%m-%Y_%H-%M-%S") \
            + self.random_string() \
            + '.csv'
        file = open(path, "w+")
        return file

    # Get a random string of letters to add to file name
    def random_string(self, k=4):
        return ''.join(random.choices(string.ascii_uppercase + string.digits,
                                      k=k))

    # Function to write gan generated data to a new file
    def write_to_csv(self, data, transform=True, file_prefix="output-"):
        if transform:
            data = self.data_transformation.transform_single_to_original(data)

        file = self.get_new_file(file_prefix)
        print(self.positions_array.shape)
        print(data.shape)
        data_with_positions = np.insert(self.positions_array, 2, data, axis=1)
        pd.DataFrame(data_with_positions).to_csv(path_or_buf=file, index=None,
                                                 header=False)
        file.close()

    # Given an array of string results, write them in the results file
    def write_results(self, results):
        results_filename = self.output_path + "results.txt"
        file = open(results_filename, "a+")

        for str_result in results:
            file.write(str_result + "\n")

        file.close()

    # Function to save a gan model to files
    def save_model(self, model, filename):
        model_filename = str(self.output_path + data_info.MODEL_FOLDER
                             + filename + ".json")
        weights_filename = str(self.output_path + data_info.MODEL_FOLDER
                               + filename + ".h5")
        # serialize model to JSON
        model_json = model.to_json()
        with open(model_filename, "w") as json_file:
            json_file.write(model_json)
        # serialize weights to HDF5
        model.save_weights(weights_filename)

    # Function to create the positions we need to append before the results
    # in order to match expected format
    def setup_position_array(self):
        for x in range(0, self.dungeon_dimension):
            for y in range(0, self.dungeon_dimension):
                position = (x * self.dungeon_dimension) + y
                self.positions_array[position][0] = x
                self.positions_array[position][1] = y
