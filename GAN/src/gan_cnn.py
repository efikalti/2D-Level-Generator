import numpy as np
import tensorflow as tf
import matplotlib.pyplot as plt

from keras.layers import Input, Dense, Dropout, Flatten, BatchNormalization
from keras.layers import Conv2D, Reshape
from keras.models import Sequential, Model
from keras import backend as K
from keras.layers.advanced_activations import LeakyReLU
from tensorflow.keras.optimizers import Adam

# Local libraries
import data_info as di
from data_transform import DataTransformation
from data_io.file_writer import FileWriter
from data_info import NOISE, DUNGEON_DIMENSION, DUNGEON_LABELS


def relu_advanced(x):
    return K.relu(x, max_value=2)


class GAN_CNN():
    def __init__(self, epochs=10000, batch_size=64, sample_interval=1000,
                 output_folder=None, create_models=True,
                 d_trainable=False, output_images=True, 
                 transform=True, one_hot_enabled=True):
        # Define training parameters
        self.epochs = epochs
        self.batch_size = batch_size
        self.sample_interval = sample_interval

        self.dis_loss = di.DIS_LOSS
        self.gen_loss = di.GEN_LOSS
        self.com_loss = di.COM_LOSS

        self.main_metric = di.METRIC

        self.latent_size = DUNGEON_DIMENSION * DUNGEON_DIMENSION * DUNGEON_LABELS

        self.str_outputs = []

        # Define dungeon dimensions
        self.dungeon_dimension = DUNGEON_DIMENSION
        self.dungeon_shape = (DUNGEON_DIMENSION, DUNGEON_DIMENSION, DUNGEON_LABELS)

        self.d_trainable = d_trainable

        self.file_writer = FileWriter(transform=transform)
        self.file_writer.create_output_folder(folder_name="cnn_gan-")
        tr = self.file_writer.data_transformation.transform_value_enabled
        self.str_outputs.append("Data transformation : " + str(tr))

        self.output_images = output_images
        self.data_transformation = DataTransformation(transform=transform, one_hot_enabled=one_hot_enabled)

        if create_models:
            self.setup_new_models()

    def setup_new_models(self):
        # Define optimizer with parameters
        self.optimizer = di.optimizers[di.OPTIMIZER]

        # Create descriminator object
        self.discriminator = self.build_discriminator()

        self.discriminator.trainable = self.d_trainable
        self.str_outputs.append("\nDiscriminator trainable = "
                                + str(self.d_trainable))

        # Parameterize descriminator
        self.discriminator.compile(loss=self.dis_loss,
                                   optimizer=self.optimizer,
                                   metrics=[self.main_metric])
        self.str_outputs.append("\nDiscriminator loss     : " + str(self.dis_loss))
        self.str_outputs.append("Discriminator metrics    : " + str(self.main_metric))
        self.str_outputs.append("Discriminator optimizer: " + str(self.optimizer))

        # Create generator object
        self.generator = self.build_generator()
        # Parameterize Generator
        self.generator.compile(loss=self.gen_loss,
                               optimizer=self.optimizer,
                               metrics=[self.main_metric])
        self.str_outputs.append("\nGenerator loss      : " + self.gen_loss)
        self.str_outputs.append("Generator metrics   : " + str(self.main_metric))
        self.str_outputs.append("Generator optimizer : " + str(self.optimizer))

        # Initialize noise input
        z = Input(shape=self.dungeon_shape)

        validity = self.discriminator(self.generator(z))
        self.combined = Model(z, validity)
        self.combined.compile(loss=self.com_loss,
                              optimizer=self.optimizer,
                              metrics=[self.main_metric])
        self.str_outputs.append("\nCombined loss    : " + self.com_loss)
        self.str_outputs.append("Combined metrics   : " + str(self.main_metric))
        self.str_outputs.append("Combined optimizer : " + str(self.optimizer))

    def build_generator(self):
        self.str_outputs.append("\nGenerator model - Sequential")
        self.model = None
        self.model = Sequential()

        #self.add_layer(Dense(units=self.latent_size, input_shape=(self.latent_size,)))
        #self.add_layer(Reshape(target_shape=self.dungeon_shape))

        self.add_layer(Conv2D(16, 3, padding='same', strides=2,
                              input_shape=self.dungeon_shape))
        self.add_layer(LeakyReLU(0.2))
        #self.add_layer(BatchNormalization(momentum=0.8))
        self.add_layer(Dropout(0.3))

        self.add_layer(Conv2D(16, 3, padding='same', strides=1))
        self.add_layer(LeakyReLU(0.2))
        #self.add_layer(BatchNormalization(momentum=0.8))
        self.add_layer(Dropout(0.3))

        self.add_layer(Conv2D(32, 3, padding='same', strides=2))
        self.add_layer(LeakyReLU(0.2))
        #self.add_layer(BatchNormalization(momentum=0.8))
        self.add_layer(Dropout(0.3))

        '''
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
        self.add_layer(Dense(units=48, activation='softmax'))
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
        self.add_layer(Dense(1, activation="sigmoid"))

        dungeon = Input(shape=self.dungeon_shape)
        validity = self.model(dungeon)

        return Model(dungeon, validity)

    def add_layer(self, layer):
        self.model.add(layer)

        layer_info = str("Class_name: " + str(layer.__class__.__name__)
                         + "\tConfig: " + str(layer.get_config()))
        self.str_outputs.append(layer_info)

    def train(self, data):
        self.add_train_info("\nCombined training.")
        X_train = np.array(data)
        batch_size = self.batch_size

        valid = np.ones((batch_size, 1))

        fake = np.zeros((batch_size, 1))

        for epoch in range(1, self.epochs + 1):
            idx = np.random.randint(0, len(X_train), batch_size)
            sample = X_train[idx]

            noise = self.get_noise(batch_size)

            dungeon = self.generator.predict(noise)
            self.discriminator.train_on_batch(sample, valid)
            self.discriminator.train_on_batch(dungeon, fake)

            noise = self.get_noise(batch_size)

            g_loss = self.combined.train_on_batch(noise, valid)

            if epoch % self.sample_interval == 0:
                self.print_epoch_result(epoch, g_loss)
                self.sample_epoch(epoch, file_prefix="combined_")

    def train_generator(self, data):
        self.add_train_info("\nGenerator training.")
        X_train = np.array(data)

        for epoch in range(1, self.epochs + 1):
            idx = np.random.randint(0, len(X_train), self.batch_size)
            sample = X_train[idx]

            noise = self.get_noise(self.batch_size)

            g_loss = self.generator.train_on_batch(noise, sample)

            if epoch % self.sample_interval == 0:
                self.print_epoch_result(epoch, g_loss)
                self.sample_epoch(epoch, file_prefix="generator_")

    def train_discriminator(self, data):
        self.add_train_info("\nDiscriminator training.")
        X_train = np.array(data)
        valid = np.ones((self.batch_size, 1))
        fake = np.zeros((self.batch_size, 1))

        for epoch in range(1, self.epochs + 1):
            idx = np.random.randint(0, len(X_train), self.batch_size)
            sample = X_train[idx]
            noise = self.get_noise(self.batch_size)

            gen_output = self.generator.predict(noise)
            d_loss_real = self.discriminator.train_on_batch(sample, valid)
            d_loss_fake = self.discriminator.train_on_batch(gen_output, fake)
            d_loss = 0.5 * np.add(d_loss_real, d_loss_fake)

            if epoch % self.sample_interval == 0:
                self.print_epoch_result(epoch, d_loss)
                self.sample_epoch(epoch, file_prefix="discriminator_")

    def get_noise(self, number_of_samples):

        noise_data = np.empty(shape=(number_of_samples, DUNGEON_DIMENSION, DUNGEON_DIMENSION, DUNGEON_LABELS))
        for s in range(0, number_of_samples):
            noise_data[s] = np.eye(DUNGEON_LABELS)[np.random.choice(DUNGEON_LABELS, DUNGEON_DIMENSION)]
            '''
            for i in range(0, DUNGEON_DIMENSION):
                for j in range(0, DUNGEON_DIMENSION):
                    noise_position = np.random.randint(NOISE["min"], NOISE["max"])
                    noise_labels = np.zeros(shape=(3))
                    noise_labels[noise_position] = 1
                    noise_data[s][i][j] = noise_labels
            '''
        return noise_data

    # Sample functions
    def sample_epoch(self, epoch, file_prefix=""):
        self.sample_dungeon(epoch, file_prefix)

    def sample_dungeon(self, epoch, file_prefix=""):
        noise = self.get_noise(1)

        gen_data = self.generator.predict(noise)
        dungeon = gen_data[0]
        prefix = str(file_prefix + str(epoch) + "_")
        self.file_writer.write_to_csv(dungeon, file_prefix=prefix)

        if self.output_images:
            self.sample_image(dungeon, epoch, file_prefix=file_prefix)

    def sample_image(self, data, epoch, file_prefix=""):
        dungeon = self.data_transformation.transform_single_to_original(data)
        #dungeon = self.data_transformation.transform_to_matrix(gen)

        img = np.empty(shape=(DUNGEON_DIMENSION, DUNGEON_DIMENSION, 3))
        for i in range(DUNGEON_DIMENSION):
            for j in range(DUNGEON_DIMENSION):
                value = dungeon[i][j]
                if value == 0:
                    value = di.colors['white']['float']
                elif value == 1:
                    value = di.colors['brown']['float']
                elif value == 2:
                    value = di.colors['orange']['float']
                else:
                    value = di.colors['black']['float']
                img[i][j] = value
    
        image_name = self.file_writer.image_folder + file_prefix + "%d.png" % epoch

        plt.imshow(img)
        plt.xticks([]), plt.yticks([])  # to hide tick values on X and Y axis
        plt.savefig(image_name, dpi=100)
        plt.close()


    def print_epoch_result(self, epoch, result):
        str_results = "%d [Loss: %f, Acc.: %.2f%%]" % (epoch,
                                                       result[0], result[1])
        print(str_results, end='\r')
        self.str_outputs.append(str_results)

    def add_train_info(self, train_info):
        self.str_outputs.append(
         train_info + "  Epochs: " + str(self.epochs)
         + "  Batch size: " + str(self.batch_size)
         + "  Sample interval: " + str(self.sample_interval))

    def write_results(self):
        self.file_writer.write_results(self.str_outputs)

    def save_models(self):
        self.file_writer.save_model(self.generator, "generator")
        self.file_writer.save_model(self.discriminator, "discriminator")

    def load_models(self, path):
        pass
