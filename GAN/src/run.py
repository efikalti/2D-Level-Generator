from file_parser import FileParser
from gan import GAN
from data_transform import DataTransformation

import numpy as np

np.set_printoptions(formatter={'float': lambda x: "{0:0.3f}".format(x)})


def main():
    file_parser = FileParser()
    data = file_parser.get_csv_data()

    data_transformation = DataTransformation()
    data = data_transformation.transform_multiple(data)

    gan = GAN(epochs=1, batch_size=32, sample_interval=1000)
    gan.train_generator(data)

    gan.sample_epoch(1)

    gan.train(data)

    gan.sample_epoch(1)

    gan.write_results()


if __name__ == "__main__":
    main()
