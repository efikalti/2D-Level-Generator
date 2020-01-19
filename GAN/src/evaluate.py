import numpy as np
from models import Vector, Bounds

from data_info import TILE_TYPES, DUNGEON_DIMENSION


class Evaluator:

    def __init__(self):
        self.data = np.ndarray(shape=(DUNGEON_DIMENSION,
                                      DUNGEON_DIMENSION), dtype=int)
        self.rooms = None

    def Convert(self, generated_data):
        # Generated data are a dungeon_dimension^2 1D array
        # Convert to the 2D  dungeon_dimension x dungeon_dimension array
        index = 0
        for x in range(0, DUNGEON_DIMENSION):
            for y in range(0, DUNGEON_DIMENSION):
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

    def evaluate_tiles_in_bounds_are_of_type(self, bounds, type_id):
        sum = 0
        position = Vector(0, 0)

        for x in range(bounds.x_min, bounds.x_max):
            position.x = x
            for y in range(bounds.y_min, bounds.y_max):
                position.y = y
                sum += self.evaluate_tile_type(position, type_id)

        return sum

    def find_rooms(self, bounds):
        room_areas = list()

        position = Vector(0, 0)
        # Evaluate the area for rooms
        next_position = 0
        for x in range(bounds.x_min, bounds.x_max):
            position.x = x
            for y in range(bounds.y_min, bounds.y_max):
                position.y = y
                if self.is_tile_of_type(position,
                                        TILE_TYPES["WALL"]) is True:
                    # Found wall tile
                    result, next_position, room_bounds = self.is_room(position,
                                                                      bounds)
                    if result is True:
                        room_areas.append(room_bounds)
        self.rooms = room_areas
        return room_areas

    def is_room(self, start_position, area_bounds):
        found_floor = False
        current_position = Vector(start_position.x, start_position.y + 1)

        # Check vertical for the room bounds
        while(current_position.y <= area_bounds.y_max and not found_floor):
            if self.is_tile_of_type(current_position,
                                    TILE_TYPES["WALL"]) is True:
                found_floor = True
                current_position.y -= 1
            else:
                current_position.y += 1
        room_bounds_y = current_position.y
        next_position = room_bounds_y + 1

        if found_floor is False or room_bounds_y == start_position.y:
            return False, next_position

        # Check horizontal for the room bounds
        found_floor = False
        current_position.x += 1

        while(current_position.x <= area_bounds.x_max and not found_floor):
            if self.is_tile_of_type(current_position,
                                    TILE_TYPES["WALL"]) is True:
                found_floor = True
                current_position.x -= 1
            else:
                current_position.x += 1

        room_bounds_x = current_position.x
        if found_floor is False or room_bounds_x == start_position.x:
            return False, next_position

        # Check vertical with the bounds found for y
        for y in range(start_position.y, room_bounds_y + 1):
            current_position.y = y
            if self.is_tile_of_type(current_position,
                                    TILE_TYPES["WALL"]) is False:
                return False, next_position

        # Check horizontal with the bounds found for y
        current_position.y = start_position.y
        for x in range(start_position.x, room_bounds_x + 1):
            current_position.x = x
            if self.is_tile_of_type(current_position,
                                    TILE_TYPES["WALL"]) is False:
                return False, next_position

        if abs(start_position.x - room_bounds_x) == 1 or \
           abs(start_position.y - room_bounds_y) == 1:
            return False, next_position

        room_bounds = Bounds(start_position.x, start_position.y,
                             room_bounds_x, room_bounds_y)
        return True, next_position, room_bounds

    def evaluate_rooms(self):
        rooms = self.find_rooms()

        score = 0
        if len(rooms) > 3:
            score = 100
        elif len(rooms) > 0:
            score = 50

        return score

    def evaluate_room_areas(self, rooms):
        sum = 0
        for room_area in rooms:
            sum += self.evaluate_tiles_in_bounds_are_of_type(
                room_area, TILE_TYPES["WALLS"])
        return sum
