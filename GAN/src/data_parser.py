import os
import pandas


PATH = '../../Assets/Data/Output/'
CSV_SUFFIX = '.csv'


def read_files(path):
    files = []
    for root, dir, file in os.walk(PATH):
        for f in file:
            print(f)
            if f.endswith(CSV_SUFFIX):
                files.append(os.path.join(root, f))
    return files


def read_csv_file(file):
    print(file)
    df = pandas.read_csv(file)
    print(df)


def main():
    files = read_files(PATH)
    for file in files:
        read_csv_file(file)


if __name__ == "__main__":
    main()
