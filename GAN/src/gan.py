import numpy as np
from keras.layers import Input, Dense  # , Dropout
from keras.layers import BatchNormalization
from keras.models import Sequential, Model
from tensorflow.keras.optimizers import Adam
from keras import backend as K
from keras.layers.advanced_activations import LeakyReLU

import matplotlib.pyplot as plt

from file_parser import FileParser
from evaluate import Evaluator

from data_info import NOISE, DUNGEON_DIMENSION


def relu_advanced(x):
    return K.relu(x, max_value=2)


class GAN():
    def __init__(self, epochs=10000, batch_size=128, sample_interval=1000,
                 output_folder=None, file_parser=None, create_models=True,
                 train_discriminator=False):
        # Define training parameters
        self.epochs = epochs
        self.batch_size = batch_size
        self.sample_interval = sample_interval

        self.str_outputs = []

        # Define dungeon dimensions
        self.dungeon_dimension = DUNGEON_DIMENSION * DUNGEON_DIMENSION
        self.dungeon_shape = (self.dungeon_dimension, )

        self.train_discriminator = train_discriminator

        if not file_parser:
            self.file_parser = FileParser()
        else:
            self.file_parser = file_parser

        self.evaluator = Evaluator()

        self.output_images = True

        if create_models:
            self.setup_new_models()

    def setup_new_models(self):
        # Define optimizer with parameters
        self.optimizer = Adam(0.0002, 0.5)

        # Create descriminator object
        self.discriminator = self.build_discriminator()

        self.discriminator.trainable = self.train_discriminator

        # Parameterize descriminator
        self.discriminator.compile(loss='binary_crossentropy',
                                   optimizer=self.optimizer,
                                   metrics=['accuracy'])

        # Create generator object
        self.generator = self.build_generator()
        # Parameterize Generator
        self.generator.compile(loss='mean_squared_error',
                               optimizer=self.optimizer,
                               metrics=['accuracy'])

        # Initialize noise input
        z = Input(shape=self.dungeon_shape)

        dungeon_generator = self.generator(z)
        validity = self.discriminator(dungeon_generator)
        self.combined = Model(z, validity)
        self.combined.compile(loss='binary_crossentropy',
                              optimizer=self.optimizer,
                              metrics=['accuracy'])

    def build_generator(self):
        self.str_outputs.append("Generator model - Sequential")
        self.model = None
        self.model = Sequential()

        self.add_layer(Dense(units=256, input_dim=self.dungeon_dimension),
                       "Dense, units=256, input_dim="
                       + str(self.dungeon_dimension))

        self.add_layer(LeakyReLU(alpha=0.2), "LeakyReLU, alpha=0.2")

        self.add_layer(BatchNormalization(momentum=0.8),
                       "BatchNormalization, momentum=0.8")

        self.add_layer(Dense(units=512), "Dense, units=1024")

        self.add_layer(LeakyReLU(alpha=0.2), "LeakyReLU, alpha=0.2")

        self.add_layer(BatchNormalization(momentum=0.8),
                       "BatchNormalization, momentum=0.8")

        self.add_layer(Dense(units=1024), "Dense, units=1024")

        self.add_layer(LeakyReLU(alpha=0.2), "LeakyReLU, alpha=0.2")

        self.add_layer(BatchNormalization(momentum=0.8),
                       "BatchNormalization, momentum=0.8")

        self.add_layer(Dense(units=1024), "Dense, units=1024")

        self.add_layer(LeakyReLU(alpha=0.2), "LeakyReLU, alpha=0.2")

        self.add_layer(BatchNormalization(momentum=0.8),
                       "BatchNormalization, momentum=0.8")

        self.add_layer(Dense(units=1024), "Dense, units=1024")

        self.add_layer(LeakyReLU(alpha=0.2), "LeakyReLU, alpha=0.2")

        self.add_layer(BatchNormalization(momentum=0.8),
                       "BatchNormalization, momentum=0.8")

        self.add_layer(Dense(units=1024), "Dense, units=1024")

        self.add_layer(LeakyReLU(alpha=0.2), "LeakyReLU, alpha=0.2")

        self.add_layer(BatchNormalization(momentum=0.8),
                       "BatchNormalization, momentum=0.8")

        self.add_layer(Dense(units=1024), "Dense, units=1024")

        self.add_layer(LeakyReLU(alpha=0.2), "LeakyReLU, alpha=0.2")

        self.add_layer(BatchNormalization(momentum=0.8),
                       "BatchNormalization, momentum=0.8")

        self.add_layer(Dense(np.prod(self.dungeon_shape), activation="tanh"),
                       "Dense, activation=tanh")

        noise = Input(shape=self.dungeon_shape)
        dungeon = self.model(noise)

        return Model(noise, dungeon)

    def build_discriminator(self):
        self.str_outputs.append("Discriminator model - Sequential")
        self.model = None
        self.model = Sequential()

        self.add_layer(Dense(units=512, input_dim=self.dungeon_dimension),
                       "Dense, units=512, input_dim="
                       + str(self.dungeon_dimension))

        self.add_layer(LeakyReLU(alpha=0.2), "LeakyReLU, alpha=0.2")

        # model.add(Dropout(0.3))

        self.add_layer(Dense(units=256), "Dense, units=256")

        self.add_layer(LeakyReLU(alpha=0.2), "LeakyReLU, alpha=0.2")

        # model.add(Dropout(0.3))

        self.add_layer(Dense(1, input_dim=self.dungeon_dimension,
                             activation="sigmoid"),
                       "Dense, units=1, input_dim="
                       + str(self.dungeon_dimension)
                       + " activation=sigmoid")

        dungeon = Input(shape=self.dungeon_shape)
        validity = self.model(dungeon)

        return Model(dungeon, validity)

    def add_layer(self, layer, layer_info):
        self.model.add(layer)
        self.str_outputs.append(layer_info)

    def train(self, data):
        self.add_train_info("Combined training.")
        X_train = np.array(data)
        batch_size = self.batch_size

        valid = np.ones((batch_size, 1))

        fake = np.zeros((batch_size, 1))

        for epoch in range(1, self.epochs + 1):
            idx = np.random.randint(0, len(X_train), batch_size)
            sample = X_train[idx]

            noise = self.get_noise(batch_size)

            gen_imgs = self.generator.predict(noise)
            self.discriminator.train_on_batch(sample, valid)
            self.discriminator.train_on_batch(gen_imgs, fake)

            noise = self.get_noise(batch_size)

            g_loss = self.combined.train_on_batch(noise, valid)

            if epoch % self.sample_interval == 0:
                self.print_epoch_result(epoch, g_loss)
                self.sample_epoch(epoch, file_prefix="combined_")

    def train_generator(self, data):
        self.add_train_info("Generator training.")
        X_train = np.array(data)

        for epoch in range(1, self.epochs + 1):
            idx = np.random.randint(0, len(X_train), self.batch_size)
            sample = X_train[idx]

            noise = self.get_noise(self.batch_size)

            g_loss = self.generator.train_on_batch(noise, sample)

            if epoch % self.sample_interval == 0:
                self.print_epoch_result(epoch, g_loss)
                self.sample_epoch(epoch, file_prefix="generator_")

    def train_discriminator(self, data):
        self.add_train_info("Discriminator training.")
        X_train = np.array(data)
        valid = np.ones((self.batch_size, 1))
        fake = np.zeros((self.batch_size, 1))

        for epoch in range(1, self.epochs + 1):
            idx = np.random.randint(0, len(X_train), self.batch_size)
            sample = X_train[idx]
            noise = self.get_noise(self.batch_size)

            gen_output = self.generator.predict(noise)
            d_loss_real = self.discriminator.train_on_batch(sample, valid)
            d_loss_fake = self.discriminator.train_on_batch(gen_output, fake)
            d_loss = 0.5 * np.add(d_loss_real, d_loss_fake)

            if epoch % self.sample_interval == 0:
                self.print_epoch_result(epoch, d_loss)
                self.sample_epoch(epoch, file_prefix="discriminator_")

    def get_noise(self, number_of_samples):
        return np.random.randint(NOISE["min"], NOISE["max"] + 1,
                                 size=(number_of_samples,
                                       self.dungeon_dimension))

    # Sample functions
    def sample_epoch(self, epoch, file_prefix=""):
        self.sample_dungeon(epoch)
        if self.output_images:
            self.sample_images(epoch, file_prefix)

    def sample_dungeon(self, epoch):
        noise = self.get_noise(1)

        gen_data = self.generator.predict(noise)
        self.file_parser.write_to_csv(gen_data.flatten())

    def sample_images(self, epoch, file_prefix=""):
        r, c = 2, 2
        noise = self.get_noise(r * c)
        gen = self.generator.predict(noise)
        gen = 0.5 * gen + 1

        imgs = np.empty(shape=(r * c, 30, 30))
        for count in range(r * c):
            index = 0
            for i in range(30):
                for j in range(30):
                    imgs[count][i][j] = gen[count][index]
                    index += 1

        fig, axs = plt.subplots(r, c)
        cnt = 0
        for i in range(r):
            for j in range(c):
                axs[i, j].imshow(imgs[cnt, :, :], cmap='gray')
                axs[i, j].axis('off')
                cnt += 1
        fig.savefig(self.file_parser.image_folder + file_prefix
                                                  + "%d.png" % epoch)
        plt.close()

    def print_epoch_result(self, epoch, result):
        str_results = "%d [Loss: %f, Acc.: %.2f%%]" % (epoch,
                                                       result[0], result[1])
        print(str_results, end='\r')
        self.str_outputs.append(str_results)

    def add_train_info(self, train_info):
        self.str_outputs.append(
         train_info + "  Epochs: " + str(self.epochs)
         + "  Batch size: " + str(self.batch_size)
         + "  Sample interval: " + str(self.sample_interval))

    def write_results(self):
        self.file_parser.write_results(self.str_outputs)

    def save_models(self):
        self.file_parser.save_model(self.generator, "generator")
        self.file_parser.save_model(self.discriminator, "discriminator")

    def load_models(self, path):
        self.generator = self.file_parser.load_model(path, "generator")
        self.discriminator = self.file_parser.load_model(path, "discriminator")
