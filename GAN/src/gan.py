import numpy as np

from keras.layers import Input, Dense, BatchNormalization, Dropout
from keras.models import Sequential, Model
from tensorflow.keras.optimizers import Adam
from keras.layers.advanced_activations import LeakyReLU
from keras.utils import np_utils

from tensorflow import keras

import matplotlib.pyplot as plt

from data_io.file_writer import FileWriter

from data_info import NOISE, DUNGEON_DIMENSION


class DENSE_GAN():
    def __init__(self, epochs=50000, batch_size=128, sample_interval=1000,
                 output_folder=None, create_models=True,
                 d_trainable=True, output_images=False,
                 fuzzy=False, transform=True):
        # Define training parameters
        self.epochs = epochs
        self.batch_size = batch_size
        self.sample_interval = sample_interval

        # Define loss parameters
        self.dis_loss = "binary_crossentropy"
        self.gen_loss = "mean_squared_error"
        self.com_loss = "mean_squared_error"

        self.main_metric = 'accuracy'

        # Array to keep information to print in results file
        self.str_outputs = []

        # Define dungeon dimensions for dense model
        self.dungeon_dimension = DUNGEON_DIMENSION * DUNGEON_DIMENSION
        self.dungeon_shape = (self.dungeon_dimension)

        # Boolean value defining whether the discriminator will be trained
        self.d_trainable = d_trainable

        self.file_writer = FileWriter(transform=transform, fuzzy=fuzzy)
        self.file_writer.create_output_folder(folder_name="dense_gan-")

        tr = self.file_writer.data_transformation.transform_value_enabled
        fl = self.file_writer.data_transformation.fuzzy_logic_enabled
        self.str_outputs.append("Data transformation : " + str(tr))
        self.str_outputs.append("Fuzzy Logic         : " + str(fl))

        self.output_images = output_images

        if create_models:
            self.setup_new_models()

    def setup_new_models(self):
        # Define optimizer with parameters
        self.optimizer = Adam(0.0002, 0.5)

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
        self.str_outputs.append("Discriminator optimizer  : Adam(0.0002, 0.5)")

        # Create generator object
        self.generator = self.build_generator()
        # Parameterize Generator
        self.generator.compile(loss=self.gen_loss,
                               optimizer=self.optimizer,
                               metrics=[self.main_metric])
        self.str_outputs.append("\nGenerator loss      : " + str(self.dis_loss))
        self.str_outputs.append("Generator metrics     : " + str(self.main_metric))
        self.str_outputs.append("Generator optimizer   : Adam(0.0002, 0.5)")

        # Initialize noise input
        z = Input(shape=self.dungeon_shape)
        validity = self.discriminator(self.generator(z))

        self.combined = Model(z, validity)
        self.combined.compile(loss=self.com_loss,
                              optimizer=self.optimizer,
                              metrics=[self.main_metric])
        self.str_outputs.append("\nCombined loss    : " + str(self.dis_loss))
        self.str_outputs.append("Combined metrics   : " + str(self.main_metric))
        self.str_outputs.append("Combined optimizer : Adam(0.0002, 0.5)")

    def build_generator(self):
        self.str_outputs.append("\nGenerator model - Sequential")
        self.model = None
        self.model = Sequential()

        self.add_layer(Dense(units=256, input_dim=self.dungeon_dimension))
        self.add_layer(LeakyReLU(alpha=0.2))
        self.add_layer(BatchNormalization(momentum=0.8))

        self.add_layer(Dense(units=512))
        self.add_layer(LeakyReLU(alpha=0.2))
        self.add_layer(Dropout(rate=0.3))

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

        self.add_layer(Dense(units=1024))
        self.add_layer(LeakyReLU(alpha=0.2))
        self.add_layer(BatchNormalization(momentum=0.8))

        self.add_layer(Dense(np.prod(self.dungeon_shape), activation="tanh"))

        noise = Input(shape=self.dungeon_shape)
        dungeon = self.model(noise)

        return Model(noise, dungeon)

    def build_discriminator(self):
        self.str_outputs.append("\nDiscriminator model - Sequential")
        self.model = None
        self.model = Sequential()

        self.add_layer(Dense(units=512, input_dim=self.dungeon_dimension))

        self.add_layer(LeakyReLU(alpha=0.2))

        # model.add(Dropout(0.3))

        self.add_layer(Dense(units=256))

        self.add_layer(LeakyReLU(alpha=0.2))

        # model.add(Dropout(0.3))

        self.add_layer(Dense(1, input_dim=self.dungeon_dimension,
                             activation="sigmoid"))

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
        batch_size = self.batch_size

        valid = np.ones((batch_size, 1))

        fake = np.zeros((batch_size, 1))

        for epoch in range(1, self.epochs + 1):
            idx = np.random.randint(0, len(data), batch_size)
            sample = self.get_sample(data, idx)

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

        for epoch in range(1, self.epochs + 1):
            idx = np.random.randint(0, len(data), self.batch_size)
            
            sample = self.get_sample(data, idx)
            labels = sample

            noise = self.get_noise(self.batch_size)

            g_loss = self.generator.train_on_batch(noise, labels)

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
        return np.random.randint(NOISE["min"], NOISE["max"] + 1,
                                 size=(number_of_samples,
                                       self.dungeon_dimension))
    
    def get_sample(self, data, ids):
        sample_data = np.ndarray(shape=(len(ids), len(data[0])))
        index = 0
        for id in ids:
            sample_data[index] = data[id]
        return sample_data


    # Sample functions
    def sample_epoch(self, epoch, file_prefix=""):
        self.sample_dungeon(epoch, file_prefix)
        if self.output_images:
            self.sample_images(epoch, file_prefix)

    def sample_dungeon(self, epoch, file_prefix=""):
        noise = self.get_noise(1)

        gen_data = self.generator.predict(noise)
        prefix = str(file_prefix + str(epoch) + "_")
        self.file_writer.write_to_csv(gen_data.flatten(), file_prefix=prefix)

    def sample_images(self, epoch, file_prefix=""):
        r, c = 2, 2
        noise = self.get_noise(r * c)
        gen = self.generator.predict(noise)
        gen = 0.5 * gen + 1

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
        fig.savefig(self.file_writer.image_folder + file_prefix
                                                  + "%d.png" % epoch)
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
        self.generator = self.file_writer.load_model(path, "generator")
        self.discriminator = self.file_writer.load_model(path, "discriminator")
