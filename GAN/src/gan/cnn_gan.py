import numpy as np

from keras.models import Sequential
from keras.layers import Dense, Dropout, Flatten, BatchNormalization, Conv2D, Reshape
from keras.layers.advanced_activations import LeakyReLU


# Local libraries
from gan.gan import GAN


class CNN_GAN(GAN):
    def build_generator(self):
        self.str_outputs.append("\nGenerator model - Sequential")
        self.model = None
        self.model = Sequential()

        #self.add_layer(Dense(units=self.latent_size, input_shape=(self.latent_size,)))
        #self.add_layer(Reshape(target_shape=self.dungeon_shape))

        self.add_layer(Conv2D(32, 3, padding='same', strides=2,
                              input_shape=self.dungeon_shape, activation='relu'))
        self.add_layer(LeakyReLU(0.2))
        self.add_layer(BatchNormalization(momentum=0.8))
        self.add_layer(Dropout(0.3))

        self.add_layer(Conv2D(16, 3, padding='same', strides=1, activation='relu'))
        self.add_layer(LeakyReLU(0.2))
        self.add_layer(BatchNormalization(momentum=0.8))
        self.add_layer(Dropout(0.3))

        self.add_layer(Conv2D(16, 3, padding='same', strides=1, activation='relu'))
        self.add_layer(LeakyReLU(0.2))
        self.add_layer(BatchNormalization(momentum=0.8))
        self.add_layer(Dropout(0.5))

        '''

        self.add_layer(Conv2D(16, 3, padding='same', strides=1))
        self.add_layer(LeakyReLU(0.2))
        self.add_layer(BatchNormalization(momentum=0.8))
        self.add_layer(Dropout(0.5))

        self.add_layer(Conv2D(16, 3, padding='same', strides=2))
        self.add_layer(LeakyReLU(0.2))
        self.add_layer(BatchNormalization(momentum=0.8))
        self.add_layer(Dropout(0.3))

        self.add_layer(Flatten())
        self.add_layer(Dense(units=self.latent_size))
        self.add_layer(BatchNormalization(momentum=0.8))

        self.add_layer(Flatten())
        self.add_layer(Dense(units=self.latent_size))
        '''
        #self.add_layer(Dense(units=4, activation=self.gen_activation))
        
        self.add_layer(Flatten())
        self.add_layer(Dense(np.prod(self.dungeon_shape), activation=self.gen_activation))
        self.add_layer(Reshape(target_shape=self.dungeon_shape))

        return self.model

    def build_discriminator(self):
        self.str_outputs.append("\nDiscriminator model - Sequential")
        self.model = None
        self.model = Sequential()

        self.add_layer(Conv2D(16, 3, padding='same', strides=2,
                              input_shape=self.dungeon_shape))
        self.add_layer(LeakyReLU(0.2))
        self.add_layer(Dropout(0.3))

        self.add_layer(Conv2D(16, 3, padding='same', strides=1))
        self.add_layer(LeakyReLU(0.2))
        self.add_layer(Dropout(0.3))

        self.add_layer(Flatten())
        self.add_layer(Dense(1, activation='relu'))

        return self.model

