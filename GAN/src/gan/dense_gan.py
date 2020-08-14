import numpy as np

from keras.layers import Input, Dense, BatchNormalization, Dropout, Reshape, Flatten
from keras.models import Sequential, Model
from keras.layers.advanced_activations import LeakyReLU

# Local libraries
from gan.gan import GAN


class DENSE_GAN(GAN):
    def build_generator(self):
        self.str_outputs.append("\nGenerator model - Sequential")
        self.model = None
        self.model = Sequential()
        
        self.model.add(Input(shape=self.dungeon_shape))

        self.add_layer(Dense(units=128))
        self.add_layer(LeakyReLU(alpha=0.2))
        self.add_layer(BatchNormalization(momentum=0.8))

        self.add_layer(Dense(units=256))
        self.add_layer(LeakyReLU(alpha=0.2))
        self.add_layer(Dropout(rate=0.3))

        self.add_layer(Dense(units=1024))
        self.add_layer(LeakyReLU(alpha=0.2))
        self.add_layer(BatchNormalization(momentum=0.8))

        '''
        self.add_layer(Dense(units=1024))
        self.add_layer(LeakyReLU(alpha=0.2))
        self.add_layer(BatchNormalization(momentum=0.8))

        self.add_layer(Dense(units=1024))
        self.add_layer(LeakyReLU(alpha=0.2))
        self.add_layer(Dropout(rate=0.3))

        self.add_layer(Dense(units=1024))
        self.add_layer(LeakyReLU(alpha=0.2))
        self.add_layer(BatchNormalization(momentum=0.8))

        self.add_layer(Dense(units=1024))
        self.add_layer(LeakyReLU(alpha=0.2))
        self.add_layer(Dropout(rate=0.3))

        self.add_layer(Dense(units=256))
        self.add_layer(LeakyReLU(alpha=0.2))
        self.add_layer(BatchNormalization(momentum=0.8))
        '''

        #self.add_layer(Dense(np.prod(self.dungeon_shape), activation="softmax"))
        self.add_layer(Dense(units=3, activation='linear'))
        #self.add_layer(Reshape(target_shape=self.dungeon_shape))

        noise = Input(shape=self.dungeon_shape)
        dungeon = self.model(noise)
        
        print("Generator summary:")
        self.model.summary()

        return Model(noise, dungeon)

    def build_discriminator(self):
        self.str_outputs.append("\nDiscriminator model - Sequential")
        self.model = None
        self.model = Sequential()

        self.model.add(Input(shape=self.dungeon_shape))

        self.add_layer(Dense(units=512))

        self.add_layer(LeakyReLU(alpha=0.2))

        # model.add(Dropout(0.3))

        self.add_layer(Dense(units=256))

        self.add_layer(LeakyReLU(alpha=0.2))

        # model.add(Dropout(0.3))

        self.add_layer(Flatten())
        self.add_layer(Dense(1, activation="relu"))

        dungeon = Input(shape=self.dungeon_shape)
        validity = self.model(dungeon)

        return Model(dungeon, validity)