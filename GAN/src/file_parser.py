import os
from datetime import datetime

import pandas as pd

import data_info


class FileParser:
    def __init__(self):
        # PATH = '../../Assets/Data/Output/'
        self.input_path = '../../Assets/Data/Test/'
        self.output_path = '../../Assets/Data/GAN_Output/'
        self.csv_suffix = '.csv'
        self.csv_prefix = 'gan_output-'

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
                    print(f)
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
               + self.csv_suffix
        file = open(path, "w+")
        return file

    def write_to_csv(self, data):
        file = self.get_new_file()
        pd.DataFrame(data).to_csv(path_or_buf=file, index=None, header=False)
        file.close()
