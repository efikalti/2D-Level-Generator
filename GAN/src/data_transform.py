import random
import data_info as di


class DataTransformation:

    def __init__(self):
        self.transform_value_enabled = True
        self.fuzzy_logic_enabled = False

    def transform_single(self, data):
        for i in range(0, len(data)):
            original_value = data[i]
            if self.transform_value_enabled:
                if original_value in di.DATA_TRANSFORMATIONS:
                    data[i] = di.DATA_TRANSFORMATIONS[original_value]
            if self.fuzzy_logic_enabled:
                if original_value in di.FUZZY_LOGIC_TRANSFORMATIONS:
                    data[i] = self.fuzzy_logic_transform(
                        data[i],
                        di.FUZZY_LOGIC_TRANSFORMATIONS[original_value])
        # Return transformed data
        return data

    def fuzzy_logic_transform(self, value, fuzzy_range):
        new_value = value + random.uniform(fuzzy_range[0], fuzzy_range[1])
        return new_value

    def fuzzy_logic_transform_to_original(self, value):
        for original_range in di.FUZZY_LOGIC_TO_ORIGINAL:
            if value >= original_range[0] and value < original_range[1]:
                return original_range[2]
        return value

    def transform_single_to_original(self, data):
        for i in range(0, len(data)):
            value = round(data[i])
            if self.fuzzy_logic_enabled:
                data[i] = self.fuzzy_logic_transform_to_original(value)
            if self.transform_value_enabled:
                if value in di.DATA_TRANSFORMATIONS_TO_ORIGINAL:
                    data[i] = di.DATA_TRANSFORMATIONS_TO_ORIGINAL[value]
        # Return data in original format
        return data

    def transform_multiple(self, data):
        for i in range(0, len(data)):
            data[i] = self.transform_single(data[i])
        return data

    def transform_multiple_to_original(self, data):
        for i in range(0, len(data)):
            data[i] = self.transform_single_to_original(data[i])
        return data
