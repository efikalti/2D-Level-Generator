from file_parser import FileParser
from gan import GAN
from gan_cnn import GAN_CNN
from data_transform import DataTransformation

import numpy as np

np.set_printoptions(formatter={'float': lambda x: "{0:0.3f}".format(x)})

file_parser = FileParser()


def train_cnn(data):
    # Transform data to matrix
    transformed_data = []
    for sample in data:
        matrix = file_parser.data_transformation.transform_to_matrix(sample)
        transformed_data.append(matrix)

    gan = GAN_CNN(epochs=50000, batch_size=64, sample_interval=1000,
                  file_parser=file_parser, train_discriminator=True)
    gan.train_generator(transformed_data)

    gan.train(transformed_data)

    return gan


def train_dense(data):
    gan = GAN(epochs=50000, batch_size=64, sample_interval=1000,
              file_parser=file_parser, train_discriminator=True)
    gan.train_generator(data)

    gan.train(data)

    return gan


def main():
    data = file_parser.get_csv_data()

    data_transformation = DataTransformation()
    data = data_transformation.transform_multiple(data)

    # Train dense model
    # gan = train_dense(data)

    # Train cnn model
    gan = train_cnn(data)

    gan.write_results()
    gan.save_models()


if __name__ == "__main__":
    main()
