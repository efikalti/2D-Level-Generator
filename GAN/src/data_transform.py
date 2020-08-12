import data_info as di
import numpy as np

from numpy import argmax
from keras.utils import to_categorical
from data_info import DUNGEON_DIMENSION


class DataTransformation:

    def __init__(self, transform=True, one_hot_enabled=True):
        self.transform_value_enabled = transform
        self.one_hot_enabled = one_hot_enabled

    def transform_single(self, data):
        for i in range(0, len(data)):
            original_value = data[i]
            # Transform label value to one hot encoding
            if self.one_hot_enabled:
                data[i] = to_categorical(original_value, num_classes=3)
            if self.transform_value_enabled:
                if original_value in di.DATA_TRANSFORMATIONS:
                    data[i] = di.DATA_TRANSFORMATIONS[original_value]
        # Return transformed data
        return data

    def transform_single_to_original(self, data):
        transformed_data = np.zeros((DUNGEON_DIMENSION, DUNGEON_DIMENSION, 1))

        for i in range(0, len(data)):
            for j in range (0, len(data[i])):
                # Transform from categorical to single value
                if self.one_hot_enabled:
                    transformed_data[i][j] = argmax(data[i][j])
                if self.transform_value_enabled:
                    value = round(data[i][j])
                    if value in di.DATA_TRANSFORMATIONS_TO_ORIGINAL:
                        data[i][j] = di.DATA_TRANSFORMATIONS_TO_ORIGINAL[value]
        # Return data in original format
        return transformed_data

    def transform_multiple(self, data):
        for i in range(0, len(data)):
            data[i] = self.transform_single(data[i])
        return data

    def transform_multiple_to_original(self, data):
        for i in range(0, len(data)):
            data[i] = self.transform_single_to_original(data[i])
        return data

    def transform_to_matrix(self, array):
        dimension = di.DUNGEON_DIMENSION
        matrix = np.zeros((dimension, dimension, 3))
        index = 0
        for y in range(dimension-1, -1, -1):
            for x in range(0, dimension, 1):
                matrix[x][y] = array[index]
                index += 1
        matrix = np.expand_dims(matrix, axis=-1)
        return matrix

    def transform_to_array(self, matrix):
        dimension = di.DUNGEON_DIMENSION
        array = np.zeros((dimension * dimension, ))
        index = 0
        for y in range(dimension-1, -1, -1):
            for x in range(0, dimension, 1):
                array[index] = matrix[x][y]
                index += 1
        return array
