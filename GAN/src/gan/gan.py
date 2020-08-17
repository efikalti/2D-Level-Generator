import numpy as np
import tensorflow as tf
import matplotlib.pyplot as plt

from keras.layers import Input
from keras.models import Model

import data_models.data_info as di
from data_io.file_writer import FileWriter
from data_models.data_info import NOISE, DUNGEON_DIMENSION, DUNGEON_LABELS
from data_models.data_transform import DataTransformation

class GAN():
    def __init__(self, gan_type, 
                 epochs=10000, batch_size=64, sample_interval=1000,
                 output_folder=None, create_models=True,
                 d_trainable=True, output_images=True, 
                 transform=True, one_hot_enabled=True):
        # Define data variables
        self.one_hot_enabled = one_hot_enabled

        # Define training parameters
        self.epochs = epochs
        self.batch_size = batch_size
        self.sample_interval = sample_interval

        self.dis_loss = di.DIS_LOSS
        self.gen_loss = di.GEN_LOSS
        self.com_loss = di.COM_LOSS

        self.dis_metric = di.DIS_METRIC
        self.gen_metric = di.GEN_METRIC
        self.com_metric = di.COM_METRIC

        self.gen_activation = di.GEN_ACTIVATION

        self.latent_size = DUNGEON_DIMENSION * DUNGEON_DIMENSION * DUNGEON_LABELS

        self.str_outputs = []

        # Define dungeon dimensions
        self.dungeon_dimension = DUNGEON_DIMENSION
        self.dungeon_labels = DUNGEON_LABELS
        self.dungeon_shape = (DUNGEON_DIMENSION, DUNGEON_DIMENSION, DUNGEON_LABELS)
        self.dungeon_units = DUNGEON_DIMENSION * DUNGEON_DIMENSION * DUNGEON_LABELS

        self.d_trainable = d_trainable

        self.gan_name = gan_type + "_gan-"

        self.file_writer = FileWriter(transform=transform)
        self.file_writer.create_output_folder(folder_name=self.gan_name)
        tr = self.file_writer.data_transformation.transform_value_enabled
        self.str_outputs.append("Data transformation : " + str(tr))

        self.output_images = output_images
        self.data_transformation = DataTransformation(transform=transform, one_hot_enabled=one_hot_enabled)

        self.model = None
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
                                   metrics=[self.dis_metric])
        self.str_outputs.append("\nDiscriminator loss     : " + str(self.dis_loss))
        self.str_outputs.append("Discriminator metrics    : " + str(self.dis_metric))
        self.str_outputs.append("Discriminator optimizer: " + str(self.optimizer))

        # Create generator object
        self.generator = self.build_generator()
        # Parameterize Generator
        self.generator.compile(loss=self.gen_loss, optimizer=self.optimizer, metrics=[self.gen_metric])
        self.str_outputs.append("\nGenerator loss      : " + self.gen_loss)
        self.str_outputs.append("Generator metrics   : " + str(self.gen_metric))
        self.str_outputs.append("Generator optimizer : " + str(self.optimizer))

        # Initialize noise input
        z = Input(shape=self.dungeon_shape)

        validity = self.discriminator(self.generator(z))
        self.combined = Model(z, validity)
        self.combined.compile(loss=self.com_loss,
                              optimizer=self.optimizer,
                              metrics=[self.com_metric])
        self.str_outputs.append("\nCombined loss    : " + self.com_loss)
        self.str_outputs.append("Combined metrics   : " + str(self.com_metric))
        self.str_outputs.append("Combined optimizer : " + str(self.optimizer))

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
                self.print_epoch_result(epoch, g_loss, self.combined.metrics_names)
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
                self.print_epoch_result(epoch, g_loss, self.generator.metrics_names)
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
                self.print_epoch_result(epoch, d_loss, self.discriminator.metrics_names)
                self.sample_epoch(epoch, file_prefix="discriminator_")

    def get_noise(self, number_of_samples):
        if self.one_hot_enabled:
            noise_data = np.empty(shape=(number_of_samples, DUNGEON_DIMENSION, DUNGEON_DIMENSION, DUNGEON_LABELS))
            for s in range(0, number_of_samples):
                noise_data[s] = np.eye(DUNGEON_LABELS)[np.random.choice(DUNGEON_LABELS, DUNGEON_DIMENSION)]
        else:
            noise_data = np.random.randint(NOISE["min"], NOISE["max"], size=(number_of_samples, DUNGEON_DIMENSION, DUNGEON_DIMENSION, DUNGEON_LABELS))
        #self.sample_image(noise_data[0], np.random.randint(0,1000), "noise-" + str(np.random.randint(0,1000)))
        return noise_data

    # Sample functions
    def sample_epoch(self, epoch, file_prefix=""):
        self.sample_dungeon(epoch, file_prefix)

    def sample_dungeon(self, epoch, file_prefix=""):
        noise = self.get_noise(1)

        gen_data = self.generator.predict(noise)
        dungeon = gen_data[0]
        print(dungeon)
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
                value = dungeon[i][j][0]
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


    def print_epoch_result(self, epoch, result, metrics_names):
        str_results = "%d [%s: %f, %s: %.2f%%]" % (epoch, 
            metrics_names[0], result[0], 
            metrics_names[1], result[1])
        print(str_results)
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