import numpy as np
from keras.layers import Input, Dense, Reshape
from keras.layers import BatchNormalization, ReLU
from keras.layers.advanced_activations import LeakyReLU
from keras.models import Sequential, Model
from tensorflow.keras.optimizers import Adam

from file_parser import FileParser


class GAN():
    def __init__(self):
        # Define dungeon dimensions
        self.dungeon_dimension = 900
        # self.dungeon_shape = (12, 900)
        self.dungeon_shape = (900, )
        self.latent_dim = 900

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

        # Initialize noise input
        z = Input(shape=self.dungeon_shape)

        dungeon_generator = self.generator(z)
        self.discriminator.trainable = False
        validity = self.discriminator(dungeon_generator)
        self.combined = Model(z, validity)
        self.combined.compile(loss='binary_crossentropy',
                              optimizer=self.optimizer)

        self.file_parser = FileParser()

    def build_generator(self):
        model = Sequential()

        model.add(Dense(256, input_dim=self.dungeon_dimension))
        model.add(ReLU())
        model.add(BatchNormalization(momentum=0.8))
        model.add(Dense(512))
        model.add(ReLU())
        model.add(BatchNormalization(momentum=0.8))
        model.add(Dense(1024))
        model.add(ReLU())
        model.add(BatchNormalization(momentum=0.8))
        model.add(Dense(np.prod(self.dungeon_shape), activation='tanh'))
        model.add(Reshape(self.dungeon_shape))

        model.summary()

        noise = Input(shape=self.dungeon_shape)
        dungeon = model(noise)

        return Model(noise, dungeon)

    def build_discriminator(self):
        model = Sequential()
        # model.add(Flatten(input_shape=self.dungeon_shape))
        model.add(Dense(512, input_dim=self.dungeon_dimension))
        model.add(ReLU())
        model.add(Dense(256))
        model.add(ReLU())
        model.add(Dense(1, activation='sigmoid'))

        model.summary()

        dungeon = Input(shape=self.dungeon_shape)
        validity = model(dungeon)

        return Model(dungeon, validity)

    def train(self, data, epochs, batch_size=128, sample_interval=50):
        X_train = np.array(data)
        valid = np.ones((batch_size, 1))

        fake = np.zeros((batch_size, 1))

        for epoch in range(epochs):
            idx = np.random.randint(0, len(X_train), batch_size)
            sample = X_train[idx]
            noise = np.random.randint(low=0, high=3, size=(batch_size,
                                                           self.latent_dim))
            gen_imgs = self.generator.predict(noise)
            d_loss_real = self.discriminator.train_on_batch(sample, valid)
            d_loss_fake = self.discriminator.train_on_batch(gen_imgs, fake)
            d_loss = 0.5 * np.add(d_loss_real, d_loss_fake)

            noise = np.random.normal(0, 1, (batch_size, self.latent_dim))
            g_loss = self.combined.train_on_batch(noise, valid)

            print("%d [D loss: %f, acc.: %.2f%%] [G loss: %f]" % (
                  epoch, d_loss[0],
                  100*d_loss[1], g_loss))
            if epoch % sample_interval == 0:
                self.sample_epoch(epoch)
        self.sample_epoch(epoch)

    def sample_epoch(self, epoch):
        noise = np.random.randint(low=0, high=3, size=(1,
                                                       self.dungeon_dimension))

        gen_data = self.generator.predict(noise)
        self.file_parser.write_to_csv(self.round_data(gen_data.flatten()))

    def round_data(self, data):
        for i in range(0, len(data)):
            new_value = round(data[i]) + 1
            data[i] = new_value
        return data
