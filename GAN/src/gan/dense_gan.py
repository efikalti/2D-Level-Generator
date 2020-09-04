import numpy as np

#from keras.layers import Input, Dense, BatchNormalization, Dropout, Reshape, Flatten

from keras.layers import Input, BatchNormalization
from keras.layers.core import Dense, Dropout, Reshape, Flatten
#from keras.layers.convolutional import Conv2D, MaxPooling2D, SeparableConv2D

from keras.models import Sequential, Model
from keras.layers.advanced_activations import LeakyReLU

# Local libraries
from gan.gan import GAN

from data_models.data_info import DUNGEON_LABELS


class DENSE_GAN(GAN):
    def build_generator(self):
        self.str_outputs.append("\nGenerator model - Sequential")
        self.model = None
        self.model = Sequential()
        
        #self.add_layer(Flatten(input_shape=self.dungeon_shape))
        self.add_layer(Dense(units=128, input_dim=self.latent_dim))
        self.add_layer(BatchNormalization(momentum=0.8))
        self.add_layer(LeakyReLU(alpha=0.2))
        self.add_layer(Dropout(rate=0.3))

        self.add_layer(Dense(units=128))
        self.add_layer(BatchNormalization(momentum=0.8))
        self.add_layer(LeakyReLU(alpha=0.2))
        self.add_layer(Dropout(rate=0.3))

        self.add_layer(Dense(units=128))
        self.add_layer(BatchNormalization(momentum=0.8))
        self.add_layer(LeakyReLU(alpha=0.2))
        self.add_layer(Dropout(rate=0.3))

        self.add_layer(Dense(np.prod(self.dungeon_shape), activation=self.gen_activation))
        self.add_layer(Reshape(self.dungeon_shape))

        
        noise = Input(shape=(self.latent_dim))
        dungeon = self.model(noise)
        
        print("Generator summary:")
        self.model.summary()

        return Model(noise, dungeon)
        
        #return self.model

    def build_discriminator(self):
        self.str_outputs.append("\nDiscriminator model - Sequential")
        self.model = None
        self.model = Sequential()

        self.add_layer(Flatten(input_shape=self.dungeon_shape))

        self.add_layer(Dense(units=64))

        self.add_layer(LeakyReLU(alpha=0.2))

        self.add_layer(Dropout(rate=0.3))

        self.add_layer(Dense(units=64))

        self.add_layer(LeakyReLU(alpha=0.2))

        self.add_layer(Dropout(rate=0.3))

        self.add_layer(Flatten())
        self.add_layer(Dense(1, activation="sigmoid"))

        
        dungeon = Input(shape=self.dungeon_shape)
        validity = self.model(dungeon)

        return Model(dungeon, validity)
        
        #return self.model