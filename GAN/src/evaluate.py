import numpy as np
from models import Vector, Bounds

from data_info import TILE_TYPES, DUNGEON_DIMENSION


class Evaluator:

    def __init__(self):
        self.data = np.ndarray(shape=(DUNGEON_DIMENSION,
                                      DUNGEON_DIMENSION), dtype=int)
        self.rooms = None
        self.results = None

    def convert(self, generated_data):
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
                                        TILE_TYPES["WALL"]):
                    # Found wall tile
                    result, next_position, room_bounds = self.is_room(position,
                                                                      bounds)
                    if result:
                        room_areas.append(room_bounds)
        self.rooms = room_areas
        return room_areas

    def is_room(self, start_position, area_bounds):
        found_floor = False
        current_position = Vector(start_position.x, start_position.y + 1)

        # Check vertical for the room bounds
        while(current_position.y <= area_bounds.y_max
              and not found_floor):
            if not self.is_tile_of_type(current_position,
                                        TILE_TYPES["WALL"]):
                found_floor = True
                current_position.y -= 1
            else:
                current_position.y += 1
        room_bounds_y = current_position.y
        next_position = room_bounds_y + 1

        if not found_floor or room_bounds_y == start_position.y:
            return False, next_position, None

        # Check horizontal for the room bounds
        found_floor = False
        current_position.x += 1

        while(current_position.x <= area_bounds.x_max
              and not found_floor):
            if not self.is_tile_of_type(current_position,
                                        TILE_TYPES["WALL"]):
                found_floor = True
                current_position.x -= 1
            else:
                current_position.x += 1

        room_bounds_x = current_position.x
        if not found_floor or room_bounds_x == start_position.x:
            return False, next_position, None

        # Check vertical with the bounds found for y
        for y in range(start_position.y, room_bounds_y + 1):
            current_position.y = y
            if not self.is_tile_of_type(current_position,
                                        TILE_TYPES["WALL"]):
                return False, next_position, None

        # Check horizontal with the bounds found for y
        current_position.y = start_position.y
        for x in range(start_position.x, room_bounds_x + 1):
            current_position.x = x
            if not self.is_tile_of_type(current_position,
                                        TILE_TYPES["WALL"]):
                return False, next_position, None

        if abs(start_position.x - room_bounds_x) == 1 or \
           abs(start_position.y - room_bounds_y) == 1:
            return False, next_position, None

        room_bounds = Bounds(start_position.x, start_position.y,
                             room_bounds_x, room_bounds_y)
        return True, next_position, room_bounds

    def evaluate_rooms(self, bounds):
        rooms = self.find_rooms(bounds)
        print(len(rooms))
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
                room_area, TILE_TYPES["ROOM"])
        return sum

    def evaluate_dungeon(self, data):
        self.convert(data)

        results = list()

        bounds = Bounds(0, 0, DUNGEON_DIMENSION, DUNGEON_DIMENSION)

        # Evaluate bounds of tilemap are walls
        bounds_are_walls_result = self.evaluate_bounds_are_of_type(
            bounds,
            TILE_TYPES["WALL"])

        results.append(bounds_are_walls_result)
        print("bounds_are_walls_result: " + str(bounds_are_walls_result))

        # Evaluate cells next to bounds of tilemap are corridors
        corridor_bounds = Bounds(bounds.x_min + 1, bounds.y_min + 1,
                                 bounds.x_max - 1, bounds.y_max - 1)
        next_to_bounds_are_corridors_result = self.evaluate_bounds_are_of_type(
            corridor_bounds,
            TILE_TYPES["CORRIDOR"])

        results.append(next_to_bounds_are_corridors_result)
        print("next_to_bounds_are_corridors_result: "
              + str(next_to_bounds_are_corridors_result))

        # Evaluate if there are any rooms in the dungeon
        room_bounds = Bounds(bounds.x_min + 2, bounds.y_min + 2,
                             bounds.x_max - 2, bounds.y_max - 2)
        number_of_rooms_result = self.evaluate_rooms(room_bounds)

        results.append(number_of_rooms_result)
        print("number_of_rooms_result: "
              + str(number_of_rooms_result))

        # Evaluate the cells in room areas
        room_area_result = self.evaluate_room_areas(self.rooms)
        results.append(room_area_result)

        print("room_area_result: "
              + str(room_area_result))

        self.total_score(results)

    def total_score(self, results):
        sum = 0
        for result in results:
            sum += result

        print("Total room score: " + str(sum))
