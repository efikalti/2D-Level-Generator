import os
import data_info
from keras.models import model_from_json

import pandas as pd


class FileReader:
    def __init__(self):
        # Set input path from global settings - data_info
        self.input_path = data_info.INPUT_FOLDER
        # CSV file suffix, used to find all csv files in a folder
        self.csv_suffix = '.csv'

    # Function that loads the data from a csv file, ignoring headers
    def read_csv_file(self, file):
        return pd.read_csv(file, header=data_info.HEADER_LINE)

    # Function that finds all the csv files in a system path
    def find_csv_files(self, path=None):
        if path is None:
            path = self.input_path

        files = []
        # Find all the files that end with the csv suffix
        # and save them in a list
        for root, dir, file in os.walk(path):
            for f in file:
                if f.endswith(self.csv_suffix):
                    files.append(os.path.join(root, f))
        # Return all csv files
        return files

    # Function that keeps only the column with the Tile Type data
    def get_tile_type(self, data):
        return data[data_info.TILE_TYPE_COLUMN].values.tolist()

    # Function that loads all the data stored in csv in a specific path
    def get_csv_data(self, path=None):
        if path is None:
            path = self.input_path

        data = []
        csv_files = self.find_csv_files(path)
        for file in csv_files:
            df = self.read_csv_file(file)
            data.append(self.get_tile_type(df))
        return data

    # Function that loads a trained keras model from file
    # The model is stored as in a json format and the model's weights
    # are stored in a h5 format
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
        return loaded_model
