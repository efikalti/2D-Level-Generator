import os
import pandas

import data_info


# PATH = '../../Assets/Data/Output/'
PATH = '../../Assets/Data/Test/'
CSV_SUFFIX = '.csv'


def find_csv_files(path):
    files = []
    for root, dir, file in os.walk(PATH):
        for f in file:
            if f.endswith(CSV_SUFFIX):
                print(f)
                files.append(os.path.join(root, f))
    return files


def read_csv_file(file):
    return pandas.read_csv(file, header=data_info.HEADER_LINE)


def get_tile_type(data):
    return data[data_info.TILE_TYPE_COLUMN].values.tolist()


def get_training_data(dir_path):
    data = []
    csv_files = find_csv_files(PATH)
    for file in csv_files:
        df = read_csv_file(file)
        data.append(get_tile_type(df))
    return data


def main():
    data = get_training_data(PATH)
    print(data)
    print(len(data))


if __name__ == "__main__":
    main()
