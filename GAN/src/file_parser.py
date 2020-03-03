import os
import random
import string
from datetime import datetime
from data_transform import DataTransformation
from keras.models import model_from_json

import pandas as pd
import numpy as np

import data_info


class FileParser:
    def __init__(self, output_path=None):
        self.input_path = data_info.INPUT_FOLDER
        self.csv_suffix = '.csv'
        self.csv_prefix = 'output-'
        self.dungeon_dimension = data_info.DUNGEON_DIMENSION
        self.results_folder = data_info.RESULTS_FOLDER
        self.number_of_lines = self.dungeon_dimension * self.dungeon_dimension
        self.positions_array = np.empty([self.number_of_lines, 2], dtype=int)
        self.setup_position_array()
        self.data_transformation = DataTransformation()
        self.image_folder = data_info.IMAGE_FOLDER

        if output_path is None:
            self.create_output_folder()
        else:
            self.output_path = output_path
            self.image_folder = self.output_path + "/Images/"

        self.results_filename = self.output_path + "results.txt"

    def create_output_folder(self):
        now = datetime.now()
        # dd_mm_YY_H_M_S
        dt_string = now.strftime("%d_%m_%Y_%H_%M_%S")
        new_path = data_info.OUTPUT_FOLDER + "folder-" + dt_string + "/"
        self.create_folder(new_path)
        self.output_path = new_path
        self.results_filename = str(self.output_path + "results_"
                                    + dt_string + ".txt")

        new_image_path = new_path + "/Images/"
        self.create_folder(new_image_path)
        self.image_folder = new_image_path

        model_path = new_path + data_info.MODEL_FOLDER
        self.create_folder(model_path)

        results_path = new_path + data_info.RESULTS_FOLDER
        self.create_folder(results_path)

    def create_folder(self, folder_path):
        if not os.path.exists(folder_path):
            os.makedirs(folder_path)

    def setup_position_array(self):
        for x in range(0, self.dungeon_dimension):
            for y in range(0, self.dungeon_dimension):
                position = (x * self.dungeon_dimension) + y
                self.positions_array[position][0] = x
                self.positions_array[position][1] = y

    def get_csv_data(self, path=None):
        if path is None:
            path = self.input_path

        data = []
        csv_files = self.find_csv_files(path)
        for file in csv_files:
            df = self.read_csv_file(file)
            data.append(self.get_tile_type(df))
        return data

    def find_csv_files(self, path=None):
        if path is None:
            path = self.input_path

        files = []
        for root, dir, file in os.walk(path):
            for f in file:
                if f.endswith(self.csv_suffix):
                    files.append(os.path.join(root, f))
        return files

    def read_csv_file(self, file):
        return pd.read_csv(file, header=data_info.HEADER_LINE)

    def get_tile_type(self, data):
        return data[data_info.TILE_TYPE_COLUMN].values.tolist()

    def get_new_file(self, file_prefix=""):
        path = self.output_path \
            + self.results_folder \
            + file_prefix \
            + self.csv_prefix \
            + datetime.now().strftime("%d-%m-%Y_%H-%M-%S") \
            + self.random_string() \
            + self.csv_suffix
        file = open(path, "w+")
        return file

    def write_to_csv(self, data, transform=True, file_prefix=""):
        if transform:
            data = self.data_transformation.transform_single_to_original(data)

        file = self.get_new_file(file_prefix)
        data_with_positions = np.insert(self.positions_array, 2, data, axis=1)
        pd.DataFrame(data_with_positions).to_csv(path_or_buf=file, index=None,
                                                 header=False)
        file.close()

    def random_string(self):
        return ''.join(random.choices(string.ascii_uppercase + string.digits,
                                      k=4))

    def write_results(self, results):
        file = open(self.results_filename, "a+")

        for str_result in results:
            file.write(str_result + "\n")

        file.close()

    def write_results_from_stream(self, stream):
        file = open(self.results_filename, "a+")

        str_result = stream.getvalue()
        file.write(str_result + "\n\n")

        file.close()

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

    def load_model(self, path, filename):
        model_filename = path + data_info.MODEL_FOLDER + filename + ".json"
        weights_filename = path + data_info.MODEL_FOLDER + filename + ".h5"
        # load json and create model
        json_file = open(model_filename, 'r')
        loaded_model_json = json_file.read()
        json_file.close()
        loaded_model = model_from_json(loaded_model_json)
        # load weights into new model
        loaded_model.load_weights(weights_filename)
