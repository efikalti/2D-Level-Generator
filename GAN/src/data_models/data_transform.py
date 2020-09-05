
import numpy as np
from numpy import argmax
from keras.utils import to_categorical

# Local libraries
import data_models.data_info as di
from data_models.data_info import DUNGEON_DIMENSION, DUNGEON_LABELS


class DataTransformation:

    def __init__(self, one_hot_enabled=True):
        self.one_hot_enabled = one_hot_enabled

    def transform_single(self, data):
        for i in range(0, len(data)):
            original_value = data[i]
            # Transform label value to one hot encoding
            if self.one_hot_enabled:
                data[i] = to_categorical(original_value, num_classes=DUNGEON_LABELS)
        # Return transformed data
        return data

    def transform_single_to_original(self, data):
        transformed_data = np.zeros((DUNGEON_DIMENSION, DUNGEON_DIMENSION, 1))

        for i in range(0, len(data)):
            for j in range (0, len(data[i])):
                # Transform from categorical to single value
                if self.one_hot_enabled:
                    transformed_data[i][j] = self.from_categorical(data[i][j])
                else:
                    value = round(data[i][j][0])
                    transformed_data[i][j] = value
        # Return data in original format
        return transformed_data

    def from_categorical(self, c_data):
        max = 0
        index = 0
        for i in range(0, len(c_data)):
            if max < c_data[i]:
                max = c_data[i]
                index = i
        return index

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
        matrix = np.zeros((dimension, dimension, DUNGEON_LABELS))
        index = 0
        for y in range(dimension-1, -1, -1):
            for x in range(0, dimension, 1):
                matrix[x][y] = array[index]
                index += 1
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
