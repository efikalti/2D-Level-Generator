from file_parser import FileParser
from gan import GAN


def main():
    file_parser = FileParser()
    data = file_parser.get_csv_data()

    gan = GAN()
    gan.train(data, epochs=100000, batch_size=48, sample_interval=10000)


if __name__ == "__main__":
    main()
