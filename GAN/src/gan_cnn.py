import numpy as np
from keras.layers import Input, Dense, Dropout, Flatten, Conv2DTranspose
from keras.layers import BatchNormalization, Conv2D, Reshape
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


class GAN_CNN():
    def __init__(self, epochs=50000, batch_size=128, sample_interval=1000,
                 output_folder=None, file_parser=None, create_models=True,
                 train_discriminator=False, output_images=False,
                 gen_loss='mean_squared_error',
                 dis_loss='mean_squared_error',
                 com_loss='mean_squared_error'):
        # Define training parameters
        self.epochs = epochs
        self.batch_size = batch_size
        self.sample_interval = sample_interval

        self.dis_loss = dis_loss
        self.gen_loss = gen_loss
        self.com_loss = com_loss

        self.latent_size = 900

        self.str_outputs = []

        # Define dungeon dimensions
        self.dungeon_dimension = DUNGEON_DIMENSION
        self.dungeon_shape = (DUNGEON_DIMENSION, DUNGEON_DIMENSION, 1)

        self.train_discriminator = train_discriminator

        if not file_parser:
            self.file_parser = FileParser()
        else:
            self.file_parser = file_parser
        tr = self.file_parser.data_transformation.transform_value_enabled
        fl = self.file_parser.data_transformation.fuzzy_logic_enabled
        self.str_outputs.append("Data transformation : " + str(tr))
        self.str_outputs.append("Fuzzy Logic         : " + str(fl))

        self.evaluator = Evaluator()

        self.output_images = output_images

        if create_models:
            self.setup_new_models()

    def setup_new_models(self):
        # Define optimizer with parameters
        self.optimizer = Adam(0.0002, 0.5)

        # Create descriminator object
        self.discriminator = self.build_discriminator()

        self.discriminator.trainable = self.train_discriminator
        self.str_outputs.append("\nDiscriminator trainable = "
                                + str(self.train_discriminator))

        # Parameterize descriminator
        self.discriminator.compile(loss=self.dis_loss,
                                   optimizer=self.optimizer,
                                   metrics=['accuracy'])
        self.str_outputs.append("\nDiscriminator loss     : " + self.dis_loss)
        self.str_outputs.append("Discriminator metrics  : accuracy")
        self.str_outputs.append("Discriminator optimizer: Adam(0.0002, 0.5)")

        # Create generator object
        self.generator = self.build_generator()
        # Parameterize Generator
        self.generator.compile(loss=self.gen_loss,
                               optimizer=self.optimizer,
                               metrics=['accuracy'])
        self.str_outputs.append("\nGenerator loss      : " + self.gen_loss)
        self.str_outputs.append("Generator metrics   : accuracy")
        self.str_outputs.append("Generator optimizer : Adam(0.0002, 0.5)")

        # Initialize noise input
        z = Input(shape=(self.latent_size,))

        dungeon_generator = self.generator(z)
        validity = self.discriminator(dungeon_generator)
        self.combined = Model(z, validity)
        self.combined.compile(loss=self.com_loss,
                              optimizer=self.optimizer,
                              metrics=['accuracy'])
        self.str_outputs.append("\nCombined loss    : " + self.com_loss)
        self.str_outputs.append("Combined metrics   : accuracy")
        self.str_outputs.append("Combined optimizer : Adam(0.0002, 0.5)")

    def build_generator(self):
        self.str_outputs.append("\nGenerator model - Sequential")
        self.model = None
        self.model = Sequential()

        self.add_layer(Dense(units=900, input_shape=(self.latent_size,)))
        self.add_layer(Reshape(target_shape=self.dungeon_shape))

        self.add_layer(Conv2D(32, 3, padding='same', strides=2))
        self.add_layer(LeakyReLU(0.2))
        self.add_layer(Dropout(0.3))

        self.add_layer(Conv2D(64, 3, padding='same', strides=1))
        self.add_layer(LeakyReLU(0.2))
        self.add_layer(Dropout(0.3))

        self.add_layer(Conv2D(128, 3, padding='same', strides=2))
        self.add_layer(LeakyReLU(0.2))
        self.add_layer(Dropout(0.3))

        self.add_layer(Conv2D(256, 3, padding='same', strides=1))
        self.add_layer(LeakyReLU(0.2))
        self.add_layer(Dropout(0.3))

        self.add_layer(Flatten())
        self.add_layer(Dense(units=900))

        self.add_layer(Reshape(target_shape=self.dungeon_shape))

        # this is the z space commonly referred to in GAN papers
        noise = Input(shape=(self.latent_size,))
        dungeon = self.model(noise)

        return Model(noise, dungeon)

    def build_discriminator(self):
        self.str_outputs.append("\nDiscriminator model - Sequential")
        self.model = None
        self.model = Sequential()

        self.add_layer(Conv2D(32, 3, padding='same', strides=2,
                       input_shape=self.dungeon_shape))
        self.add_layer(LeakyReLU(0.2))
        self.add_layer(Dropout(0.3))

        self.add_layer(Conv2D(64, 3, padding='same', strides=1))
        self.add_layer(LeakyReLU(0.2))
        self.add_layer(Dropout(0.3))

        self.add_layer(Conv2D(128, 3, padding='same', strides=2))
        self.add_layer(LeakyReLU(0.2))
        self.add_layer(Dropout(0.3))

        self.add_layer(Conv2D(256, 3, padding='same', strides=1))
        self.add_layer(LeakyReLU(0.2))
        self.add_layer(Dropout(0.3))

        self.add_layer(Dense(1, activation="sigmoid"))

        dungeon = Input(shape=self.dungeon_shape)
        validity = self.model(dungeon)

        return Model(dungeon, validity)

    def add_layer(self, layer):
        self.model.add(layer)

        layer_info = str("Class_name: " + str(layer.__class__.__name__)
                         + "\tConfig: " + str(layer.get_config()))
        self.str_outputs.append(layer_info)

    def train(self, data):
        self.add_train_info("\nCombined training.")
        X_train = np.array(data)
        batch_size = self.batch_size

        valid = np.ones((batch_size, 1))

        fake = np.zeros((batch_size, 1))

        for epoch in range(1, self.epochs + 1):
            idx = np.random.randint(0, len(X_train), batch_size)
            sample = X_train[idx]
            print(sample.shape)

            noise = self.get_noise(batch_size)

            dungeon = self.generator.predict(noise)
            self.discriminator.train_on_batch(sample, valid)
            self.discriminator.train_on_batch(dungeon, fake)

            noise = self.get_noise(batch_size)

            g_loss = self.combined.train_on_batch(noise, valid)

            if epoch % self.sample_interval == 0:
                self.print_epoch_result(epoch, g_loss)
                self.sample_epoch(epoch, file_prefix="combined_")

    def train_generator(self, data):
        self.add_train_info("\nGenerator training.")
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
        self.add_train_info("\nDiscriminator training.")
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
                                 size=(number_of_samples, self.latent_size))

    # Sample functions
    def sample_epoch(self, epoch, file_prefix=""):
        self.sample_dungeon(epoch, file_prefix)
        if self.output_images:
            self.sample_images(epoch, file_prefix)

    def sample_dungeon(self, epoch, file_prefix=""):
        noise = self.get_noise(1)

        gen_data = self.generator.predict(noise)
        self.file_parser.write_to_csv(gen_data.flatten(),
                                      file_prefix + "_" + str(epoch) + "_")

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
