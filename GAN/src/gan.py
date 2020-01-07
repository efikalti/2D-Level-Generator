import numpy as np
from keras.layers import Input, Dense, Reshape, Flatten
from keras.layers import BatchNormalization
from keras.layers.advanced_activations import LeakyReLU
from keras.models import Sequential, Model
from keras.optimizers import Adam


class GAN():
    def __init__(self):
        # Define dungeon dimensions
        self.dungeon_dimension = 30
        self.dungeon_shape = (self.dungeon_dimension, self.dungeon_dimension)
        self.latent_dim = 100

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
        z = Input(shape=(self.latent_dim,))

        dungeon_generator = self.generator(z)
        self.discriminator.trainable = False
        validity = self.discriminator(dungeon_generator)
        self.combined = Model(z, validity)
        self.combined.compile(loss='binary_crossentropy',
                              optimizer=self.optimizer)


def build_generator(self):
    model = Sequential()

    model.add(Dense(256, input_dim=self.latent_dim))
    model.add(LeakyReLU(alpha=0.2))
    model.add(BatchNormalization(momentum=0.8))
    model.add(Dense(512))
    model.add(LeakyReLU(alpha=0.2))
    model.add(BatchNormalization(momentum=0.8))
    model.add(Dense(1024))
    model.add(LeakyReLU(alpha=0.2))
    model.add(BatchNormalization(momentum=0.8))
    model.add(Dense(np.prod(self.img_shape), activation='tanh'))
    model.add(Reshape(self.img_shape))

    model.summary()

    noise = Input(shape=(self.latent_dim,))
    dungeon = model(noise)

    return Model(noise, dungeon)


def build_discriminator(self):
    model = Sequential()
    model.add(Flatten(input_shape=self.dungeon_shape))
    model.add(Dense(512))
    model.add(LeakyReLU(alpha=0.2))
    model.add(Dense(256))
    model.add(LeakyReLU(alpha=0.2))
    model.add(Dense(1, activation='sigmoid'))

    model.summary()

    dungeon = Input(shape=self.dungeon_shape)
    validity = model(dungeon)

    return Model(dungeon, validity)


def train(self, data, epochs, batch_size=128, sample_interval=50):
    # (X_train, _), (_, _) = mnist.load_data()
    # X_train = X_train / 127.5 - 1.
    # X_train = np.expand_dims(X_train, axis=3)
    X_train = data
    valid = np.ones((batch_size, 1))

    fake = np.zeros((batch_size, 1))

    for epoch in range(epochs):
        idx = np.random.randint(0, X_train.shape[0], batch_size)
        imgs = X_train[idx]
        noise = np.random.normal(0, 1, (batch_size, self.latent_dim))
        gen_imgs = self.generator.predict(noise)
        d_loss_real = self.discriminator.train_on_batch(imgs, valid)
        d_loss_fake = self.discriminator.train_on_batch(gen_imgs, fake)
        d_loss = 0.5 * np.add(d_loss_real, d_loss_fake)

        noise = np.random.normal(0, 1, (batch_size, self.latent_dim))
        g_loss = self.combined.train_on_batch(noise, valid)

        print("%d [D loss: %f, acc.: %.2f%%] [G loss: %f]" % (epoch, d_loss[0],
              100*d_loss[1], g_loss))
        if epoch % sample_interval == 0:
            self.sample_images(epoch)


def sample_images(self, epoch):
    pass
