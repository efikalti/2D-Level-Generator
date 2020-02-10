import numpy as np
from keras.layers import Input, Dense
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
    def __init__(self, epochs=10000, batch_size=128, sample_interval=1000):
        # Define training parameters
        self.epochs = epochs
        self.batch_size = batch_size
        self.sample_interval = sample_interval

        # Define dungeon dimensions
        self.dungeon_dimension = DUNGEON_DIMENSION * DUNGEON_DIMENSION
        self.dungeon_shape = (self.dungeon_dimension, )

        # Define optimizer with parameters
        self.optimizer = Adam(0.0002, 0.5)

        # Create descriminator object
        self.discriminator = self.build_discriminator()
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

        self.file_parser = FileParser()

        self.evaluator = Evaluator()

        self.output_images = False

    def build_generator(self):
        model = Sequential()

        model.add(Dense(256, input_dim=self.dungeon_dimension))
        model.add(LeakyReLU(alpha=0.2))
        model.add(BatchNormalization(momentum=0.8))
        model.add(Dense(512))
        model.add(LeakyReLU(alpha=0.2))
        model.add(BatchNormalization(momentum=0.8))
        model.add(Dense(1024))
        model.add(LeakyReLU(alpha=0.2))
        model.add(BatchNormalization(momentum=0.8))
        model.add(Dense(np.prod(self.dungeon_shape), activation="tanh"))

        model.summary()

        noise = Input(shape=self.dungeon_shape)
        dungeon = model(noise)

        return Model(noise, dungeon)

    def build_discriminator(self):
        model = Sequential()
        model.add(Dense(512, input_dim=self.dungeon_dimension))
        model.add(LeakyReLU(alpha=0.2))
        model.add(Dense(256))
        model.add(LeakyReLU(alpha=0.2))
        model.add(Dense(1, activation="sigmoid"))

        model.summary()

        dungeon = Input(shape=self.dungeon_shape)
        validity = model(dungeon)

        return Model(dungeon, validity)

    def train(self, data):
        X_train = np.array(data)
        batch_size = self.batch_size

        valid = np.ones((batch_size, 1))

        fake = np.zeros((batch_size, 1))

        self.discriminator.trainable = False

        for epoch in range(self.epochs):
            idx = np.random.randint(0, len(X_train), batch_size)
            sample = X_train[idx]

            noise = self.get_noise(batch_size)

            gen_imgs = self.generator.predict(noise)
            self.discriminator.train_on_batch(sample, valid)
            self.discriminator.train_on_batch(gen_imgs, fake)

            noise = self.get_noise(batch_size)

            g_loss = self.combined.train_on_batch(noise, valid)

            self.print_epoch_result(epoch, g_loss)

            if epoch % self.sample_interval == 0:
                self.sample_epoch(epoch)

    def train_generator(self, data):
        X_train = np.array(data)

        for epoch in range(self.epochs):
            idx = np.random.randint(0, len(X_train), self.batch_size)
            sample = X_train[idx]

            noise = self.get_noise(self.batch_size)

            g_loss = self.generator.train_on_batch(noise, sample)

            self.print_epoch_result(epoch, g_loss)

            if epoch % self.sample_interval == 0:
                self.sample_epoch(epoch)

    def train_discriminator(self, data):
        X_train = np.array(data)
        valid = np.ones((self.batch_size, 1))
        fake = np.zeros((self.batch_size, 1))

        for epoch in range(self.epochs):
            idx = np.random.randint(0, len(X_train), self.batch_size)
            sample = X_train[idx]
            noise = self.get_noise(self.batch_size)

            gen_output = self.generator.predict(noise)
            d_loss_real = self.discriminator.train_on_batch(sample, valid)
            d_loss_fake = self.discriminator.train_on_batch(gen_output, fake)
            d_loss = 0.5 * np.add(d_loss_real, d_loss_fake)

            self.print_epoch_result(epoch, d_loss)

            if epoch % self.sample_interval == 0:
                self.sample_epoch(epoch)

    def get_noise(self, number_of_samples):
        return np.random.normal(NOISE["min"], NOISE["max"],
                                size=(number_of_samples,
                                      self.dungeon_dimension))

    # Sample functions
    def sample_epoch(self, epoch):
        self.sample_dungeon(epoch)
        if self.output_images:
            self.sample_images(epoch)

    def sample_dungeon(self, epoch):
        noise = self.get_noise(1)

        gen_data = self.generator.predict(noise)
        self.file_parser.write_to_csv(gen_data.flatten())

    def sample_images(self, epoch):
        r, c = 5, 5
        noise = self.get_noise(r * c)
        gen = self.generator.predict(noise)
        gen = 0.5 * gen + 0.5

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
        fig.savefig("images/%d.png" % epoch)
        plt.close()

    def print_epoch_result(self, epoch, result):
        print("%d [Loss: %f, Acc.: %.2f%%]" %
              (epoch, result[0], result[1]),
              end='\r')
