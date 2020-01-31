from file_parser import FileParser
from gan import GAN
from data_transform import DataTransformation
from evaluate import Evaluator


def main():
    file_parser = FileParser()
    data = file_parser.get_csv_data()

    data_transformation = DataTransformation()
    data = data_transformation.transform_multiple(data)

    gan = GAN()
    gan.train(data, epochs=10000, batch_size=100, sample_interval=1000)

    #evaluator = Evaluator()
    #evaluator.evaluate_dungeon(data[0])


if __name__ == "__main__":
    main()
