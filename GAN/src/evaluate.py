import numpy as np
from models import Vector, Bounds

import data_info


class Evaluator:

    def __init__(self):
        self.data = np.ndarray(shape=(data_info.DUNGEON_DIMENSION,
                                      data_info.DUNGEON_DIMENSION), dtype=int)

    def Convert(self, generated_data):
        # Generated data are a dungeon_dimension^2 1D array
        # Convert to the 2D  dungeon_dimension x dungeon_dimension array
        index = 0
        for x in range(0, data_info.DUNGEON_DIMENSION):
            for y in range(0, data_info.DUNGEON_DIMENSION):
                self.data[x][y] = generated_data[index]
                index += 1

        print(self.data)

    def is_tile_of_type(self, vector, type_id):
        if vector is None:
            return False

        position_tile_id = self.data[vector.x][vector.y]

        return position_tile_id == type_id

    def evaluate_tile_type(self, vector, type_id):
        if self.is_tile_of_type(vector, type_id):
            return 1
        return -1

    def evaluate_bounds_are_of_type(self, bounds, type_id):
        sum = 0
        position = Vector(0, 0)

        # Bottom horizontal bound
        position.y = bounds.y_min
        for x in range(bounds.x_min, bounds.x_max):
            position.x = x
            sum += self.evaluate_tile_type(position, type_id)

        # Top horizontal bound
        position.y = bounds.y_max - 1
        for x in range(bounds.x_min, bounds.x_max):
            position.x = x
            sum += self.evaluate_tile_type(position, type_id)

        # Left vertical bound
        position.x = bounds.x_min
        for y in range(bounds.y_min, bounds.y_max - 1):
            position.y = y
            sum += self.evaluate_tile_type(position, type_id)

        # Right vertical bound
        position.x = bounds.x_max - 1
        for y in range(bounds.y_min + 1, bounds.y_max - 1):
            position.y = y
            sum += self.evaluate_tile_type(position, type_id)

        return sum
