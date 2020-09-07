import numpy as np

from keras.models import Sequential, Model
from keras.layers import Dense, Dropout, Flatten, BatchNormalization, Conv2D, Reshape, Conv2DTranspose, Input, UpSampling2D
from keras.layers.advanced_activations import LeakyReLU


# Local libraries
from gan.gan import GAN


class CNN_GAN(GAN):
    def build_generator(self):
        self.str_outputs.append("\nGenerator model - Sequential")
        self.model = None
        self.model = Sequential()
        
        depth = 128
        
        units = 25 * 25 * 128
        self.add_layer(Dense(units, input_dim=self.latent_dim))
        self.add_layer(BatchNormalization(momentum=0.8))
        self.add_layer(LeakyReLU(0.2))

        # Reshape to 25 x 25 x 128
        self.add_layer(Reshape(target_shape=(25, 25, 128)))
        self.add_layer(Dropout(0.3))

        # Upsample to 50 x 50 x 128
        self.add_layer(UpSampling2D())

        # Change to 50 x 50 x 64
        self.add_layer(Conv2DTranspose(int(depth/2), 5, padding='same'))
        self.add_layer(BatchNormalization(momentum=0.8))
        self.add_layer(LeakyReLU(0.2))

        # Upsample to 100 x 100 x 64
        self.add_layer(UpSampling2D())

        # Change to 100 x 100 x 32
        self.add_layer(Conv2DTranspose(int(depth/4), 5, padding='same'))
        self.add_layer(BatchNormalization(momentum=0.8))
        self.add_layer(LeakyReLU(0.2))

        # Change to 100 x 100 x 2
        
        self.add_layer(Conv2DTranspose(2, 5, padding='same'))
        self.add_layer(BatchNormalization(momentum=0.8))
        self.add_layer(LeakyReLU(0.2))

        '''
        self.add_layer(Conv2D(32, 3, padding='same', strides=2))
        self.add_layer(LeakyReLU(0.2))
        self.add_layer(BatchNormalization(momentum=0.8))
        self.add_layer(Dropout(0.3))

        self.add_layer(Conv2D(16, 3, padding='same', strides=1))
        self.add_layer(LeakyReLU(0.2))
        self.add_layer(BatchNormalization(momentum=0.8))
        self.add_layer(Dropout(0.3))

        self.add_layer(Conv2D(16, 3, padding='same', strides=1))
        self.add_layer(LeakyReLU(0.2))
        self.add_layer(BatchNormalization(momentum=0.8))
        self.add_layer(Dropout(0.5))
        
        self.add_layer(Flatten())
        self.add_layer(Dense(np.prod(self.dungeon_shape), activation='sigmoid'))
        self.add_layer(Reshape(target_shape=self.dungeon_shape))
        '''
        

        '''
        # foundation for 25 x 25 image
        n_nodes = 10 * 10 * 100
        self.add_layer(Dense(n_nodes, input_dim=self.latent_dim))
        self.add_layer(LeakyReLU(alpha=0.2))
        self.add_layer(Reshape((10, 10, 200)))

        # upsample to 50x50
        self.add_layer(Conv2DTranspose(200, (4,4), strides=(2,2), padding='same'))
        self.add_layer(LeakyReLU(alpha=0.2))

        # upsample to 100x100
        self.add_layer(Conv2DTranspose(200, (4,4), strides=(2,2), padding='same'))
        self.add_layer(LeakyReLU(alpha=0.2))

        self.add_layer(Conv2D(2, (100,100), activation='sigmoid', padding='same'))
        
        self.add_layer(Flatten())
        self.add_layer(Dense(np.prod(self.dungeon_shape), activation="sigmoid"))
        self.add_layer(Reshape(self.dungeon_shape))
        '''

        '''
        noise = Input(shape=(self.latent_dim))
        dungeon = self.model(noise)
        return Model(noise, dungeon)
        '''

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
        self.add_layer(Dense(1, activation='sigmoid'))

        '''
        dungeon = Input(shape=self.dungeon_shape)
        validity = self.model(dungeon)
        return Model(dungeon, validity)
        '''
        return self.model

