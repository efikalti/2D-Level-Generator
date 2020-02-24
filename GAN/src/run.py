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

    gan = GAN(epochs=1000, batch_size=64, sample_interval=1000,
              file_parser=file_parser)
    gan.train_generator(data)

    gan.train(data)

    gan.write_results()
    gan.save_models()


if __name__ == "__main__":
    main()
