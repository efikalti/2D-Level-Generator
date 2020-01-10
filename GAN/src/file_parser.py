import os
import random
import string
from datetime import datetime

import pandas as pd
import numpy as np

import data_info


class FileParser:
    def __init__(self):
        # PATH = '../../Assets/Data/Output/'
        self.input_path = '../../Assets/Data/Test/'
        self.output_path = '../../Assets/Data/GAN_Output/'
        self.csv_suffix = '.csv'
        self.csv_prefix = 'gan_output-'
        self.dungeon_dimension = data_info.DUNGEON_DIMENSION
        self.number_of_lines = self.dungeon_dimension * self.dungeon_dimension
        self.positions_array = np.empty([self.number_of_lines, 2], dtype=int)
        self.setup_position_array()

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

    def get_new_file(self):
        path = self.output_path \
               + self.csv_prefix \
               + datetime.now().strftime("%d-%m-%Y_%H-%M-%S") \
               + self.random_string() \
               + self.csv_suffix
        file = open(path, "w+")
        return file

    def write_to_csv(self, data):
        file = self.get_new_file()
        data_with_positions = np.insert(self.positions_array, 2, data, axis=1)
        pd.DataFrame(data_with_positions).to_csv(path_or_buf=file, index=None,
                                                 header=False)
        file.close()

    def random_string(self):
        return ''.join(random.choices(string.ascii_uppercase + string.digits,
                                      k=4))
