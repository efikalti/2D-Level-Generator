from file_parser import FileParser
from gan import GAN
from evaluate import Evaluator


def main():
    file_parser = FileParser()
    data = file_parser.get_csv_data()

    #gan = GAN()
    #gan.train(data, epochs=100000, batch_size=48, sample_interval=10000)

    evaluator = Evaluator()
    evaluator.evaluate_dungeon(data[0])


if __name__ == "__main__":
    main()
