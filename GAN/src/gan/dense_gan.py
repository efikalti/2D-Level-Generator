import numpy as np

from keras.models import Sequential
from keras.layers import BatchNormalization
from keras.layers.core import Dense, Dropout, Reshape, Flatten
from keras.layers.advanced_activations import LeakyReLU

# Local libraries
from gan.gan import GAN


class DENSE_GAN(GAN):
    def build_generator(self):
        self.str_outputs.append("\nGenerator model - Sequential")
        self.model = None
        self.model = Sequential()
        
        self.add_layer(Dense(units=1024, input_dim=self.latent_dim))
        self.add_layer(BatchNormalization(momentum=0.8))
        self.add_layer(LeakyReLU(alpha=0.2))
        #self.add_layer(Dropout(rate=0.3))

        self.add_layer(Dense(units=612))
        self.add_layer(BatchNormalization(momentum=0.8))
        self.add_layer(LeakyReLU(alpha=0.2))
        self.add_layer(Dropout(rate=0.3))

        self.add_layer(Dense(units=612))
        self.add_layer(BatchNormalization(momentum=0.8))
        self.add_layer(LeakyReLU(alpha=0.2))
        #self.add_layer(Dropout(rate=0.3))

        self.add_layer(Dense(units=256))
        self.add_layer(BatchNormalization(momentum=0.8))
        self.add_layer(LeakyReLU(alpha=0.2))
        self.add_layer(Dropout(rate=0.3))

        self.add_layer(Dense(units=612))
        self.add_layer(BatchNormalization(momentum=0.8))
        self.add_layer(LeakyReLU(alpha=0.2))
        #self.add_layer(Dropout(rate=0.3))

        self.add_layer(Dense(units=1024, input_dim=self.latent_dim))
        self.add_layer(BatchNormalization(momentum=0.8))
        self.add_layer(LeakyReLU(alpha=0.2))
        #self.add_layer(Dropout(rate=0.3))

        self.add_layer(Dense(np.prod(self.dungeon_shape), activation="sigmoid"))
        self.add_layer(Reshape(self.dungeon_shape))
        
        return self.model

    def build_discriminator(self):
        self.str_outputs.append("\nDiscriminator model - Sequential")
        self.model = None
        self.model = Sequential()

        self.add_layer(Flatten(input_shape=self.dungeon_shape))

        self.add_layer(Dense(units=128))
        self.add_layer(BatchNormalization(momentum=0.8))
        self.add_layer(LeakyReLU(alpha=0.2))

        #self.add_layer(Dropout(rate=0.3))

        self.add_layer(Dense(units=128))
        self.add_layer(BatchNormalization(momentum=0.8))
        self.add_layer(LeakyReLU(alpha=0.2))

        #self.add_layer(Dropout(rate=0.3))

        self.add_layer(Dense(units=128))
        self.add_layer(BatchNormalization(momentum=0.8))
        self.add_layer(LeakyReLU(alpha=0.2))

        self.add_layer(Flatten())
        self.add_layer(Dense(1, activation="sigmoid"))
        
        return self.model