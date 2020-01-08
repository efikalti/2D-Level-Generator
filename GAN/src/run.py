from file_parser import FileParser
from gan import GAN


def main():
    file_parser = FileParser()
    data = file_parser.get_csv_data()

    gan = GAN()
    gan.train(data, epochs=100, batch_size=2, sample_interval=50)


if __name__ == "__main__":
    main()
