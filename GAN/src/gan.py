import numpy as np
from keras.layers import Input, Dense
from keras.layers import BatchNormalization, ReLU
from keras.models import Sequential, Model
from tensorflow.keras.optimizers import Adam
from keras import backend as K
from keras.layers.advanced_activations import LeakyReLU

import matplotlib.pyplot as plt

from file_parser import FileParser
from evaluate import Evaluator


def relu_advanced(x):
    return K.relu(x, max_value=2)


class GAN():
    def __init__(self):
        # Define dungeon dimensions
        self.dungeon_dimension = 900
        self.dungeon_shape = (900, )

        # Define optimizer with parameters
        self.optimizer = Adam(0.0002, 0.5)

        # Create descriminator object
        self.discriminator = self.build_discriminator()
        self.discriminator.trainable = False
        # Parameterize descriminator
        self.discriminator.compile(loss='binary_crossentropy',
                                   optimizer=self.optimizer,
                                   metrics=['accuracy'])

        # Create generator object
        self.generator = self.build_generator()

        # Initialize noise input
        z = Input(shape=self.dungeon_shape)

        dungeon_generator = self.generator(z)
        validity = self.discriminator(dungeon_generator)
        self.combined = Model(z, validity)
        self.combined.compile(loss='binary_crossentropy',
                              optimizer=self.optimizer)

        self.file_parser = FileParser()

        self.evaluator = Evaluator()

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

    def train(self, data, epochs, batch_size=128, sample_interval=100):
        X_train = np.array(data)
        valid = np.ones((batch_size, 1))

        fake = np.zeros((batch_size, 1))

        for epoch in range(epochs):
            idx = np.random.randint(0, len(X_train), batch_size)
            sample = X_train[idx]

            noise = np.random.normal(-1, 1, size=(batch_size,
                                                  self.dungeon_dimension))

            gen_imgs = self.generator.predict(noise)
            d_loss_real = self.discriminator.train_on_batch(sample, valid)
            d_loss_fake = self.discriminator.train_on_batch(gen_imgs, fake)
            d_loss = 0.5 * np.add(d_loss_real, d_loss_fake)

            noise = np.random.normal(-1, 1, size=(batch_size,
                                                  self.dungeon_dimension))

            g_loss = self.combined.train_on_batch(noise, valid)
            print("%d [D loss: %f, acc.: %.2f%%] [G loss: %f]" % (
                  epoch, d_loss[0],
                  100*d_loss[1], g_loss), end='\r')
            if epoch % sample_interval == 0:
                self.sample_epoch(epoch)
                self.sample_images(epoch)
        self.sample_epoch(epoch)
        self.sample_images(epoch)

    def sample_epoch(self, epoch):
        noise = np.random.normal(-1, 1, size=(1, self.dungeon_dimension))

        gen_data = self.generator.predict(noise)
        self.file_parser.write_to_csv(gen_data.flatten())

    def sample_images(self, epoch):
        r, c = 5, 5
        noise = np.random.normal(-1, 1, (r * c, self.dungeon_dimension))
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
