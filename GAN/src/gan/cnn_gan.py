from keras.layers import Input, Dense, Dropout, Flatten, BatchNormalization, Embedding
from keras.layers import Conv2D, Reshape, Activation, Conv2DTranspose, Concatenate
from keras.models import Sequential, Model
from keras.initializers import RandomNormal
from keras import backend as K
from keras.layers.advanced_activations import LeakyReLU

# Local libraries
from gan.gan import GAN

def relu_advanced(x):
    return K.relu(x, max_value=2)


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
        #self.add_layer(Dropout(0.3))

        self.add_layer(Conv2D(32, 3, padding='same', strides=1, activation='relu'))
        self.add_layer(LeakyReLU(0.2))
        self.add_layer(BatchNormalization(momentum=0.8))
        #self.add_layer(Dropout(0.3))

        self.add_layer(Conv2D(32, 3, padding='same', strides=1, activation='relu'))
        self.add_layer(LeakyReLU(0.2))
        self.add_layer(BatchNormalization(momentum=0.8))
        #self.add_layer(Dropout(0.5))

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
        
        self.add_layer(Dense(np.prod(self.dungeon_shape), activation=self.gen_activation))
        self.add_layer(Reshape(target_shape=self.dungeon_shape))

        # this is the z space commonly referred to in GAN papers
        noise = Input(shape=self.dungeon_shape)
        dungeon = self.model(noise)

        return Model(noise, dungeon)

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

        dungeon = Input(shape=self.dungeon_shape)
        validity = self.model(dungeon)

        return Model(dungeon, validity)

