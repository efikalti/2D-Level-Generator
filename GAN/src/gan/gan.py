import numpy as np
import tensorflow as tf
import matplotlib.pyplot as plt

import tensorboard
from tensorflow import keras

from keras.models import Sequential, Model
from keras.layers import Input

# Local libraries
import data_models.data_info as di
from data_io.file_writer import FileWriter
from data_models.data_transform import DataTransformation
from data_models.data_info import DUNGEON_DIMENSION, DUNGEON_LABELS

class GAN():
    def __init__(self, gan_type, 
                 epochs=10000, batch_size=64, sample_interval=1000,
                 output_folder=None, create_models=True,
                 output_images=True, one_hot_enabled=True,
                 discriminator_intervals=100):
        # Define data variables
        self.one_hot_enabled = one_hot_enabled

        self.discriminator_intervals = discriminator_intervals

        # Define training parameters
        self.epochs = epochs
        self.batch_size = batch_size
        self.sample_interval = sample_interval
        self.loss = di.LOSS
        self.metric = di.METRIC
        self.latent_dim = di.LATENT_DIM
        self.optimizer = di.optimizers[di.OPTIMIZER]

        self.str_outputs = []
        self.str_outputs.append("\nLoss     : " + str(self.loss))
        self.str_outputs.append("Metrics    : " + str(self.metric))
        self.str_outputs.append("Optimizer: " + str(self.optimizer))

        # Define dungeon dimensions
        self.dungeon_dimension = DUNGEON_DIMENSION
        self.dungeon_labels = DUNGEON_LABELS
        self.dungeon_shape = (DUNGEON_DIMENSION, DUNGEON_DIMENSION, DUNGEON_LABELS)
        self.dungeon_units = DUNGEON_DIMENSION * DUNGEON_DIMENSION * DUNGEON_LABELS

        self.gan_name = gan_type + "_gan-"

        self.file_writer = FileWriter()
        self.file_writer.create_output_folder(folder_name=self.gan_name)

        self.output_images = output_images
        self.data_transformation = DataTransformation(one_hot_enabled=one_hot_enabled)

        self.model = None
        if create_models:
            self.setup_new_models()

    def setup_new_models(self):
        # Create generator
        self.generator = self.build_generator()
        #self.generator.compile(loss=self.loss, optimizer=self.optimizer, metrics=[self.metric])

        # Create descriminator
        self.discriminator = self.build_discriminator()

        # Parameterize descriminator
        self.discriminator.compile(loss=self.loss,
                                   optimizer=self.optimizer,
                                   metrics=[self.metric])

        # Create combined model
        
        self.combined = Sequential()
        self.discriminator.trainable = False
        self.combined.add(self.generator)
        self.combined.add(self.discriminator)
        

        '''
        z = Input(shape=(self.latent_dim))
        self.discriminator.trainable = False
        validity = self.discriminator(self.generator(z))
        self.combined = Model(z, validity)
        '''

        self.combined.compile(loss=self.loss,
                              optimizer=self.optimizer,
                              metrics=[self.metric])


    def train(self, data):
        tensorboard_callback = keras.callbacks.TensorBoard(log_dir=self.file_writer.com_logs_dir)

        valid = np.ones((self.batch_size, 1))

        # Train the combined model
        completed_epochs = 0
        while(completed_epochs < self.epochs):

            noise = self.get_noise(self.batch_size)

            # Train discriminator separetely
            self.train_discriminator(data, self.discriminator_intervals, noise)

            c_loss = self.combined.fit(noise, valid,
                epochs=completed_epochs + self.sample_interval,
                batch_size=self.batch_size,
                initial_epoch=completed_epochs,
                callbacks=[tensorboard_callback],
                verbose=1)
            
            completed_epochs += self.sample_interval
            
            self.add_train_info("\nCombined training.")
            self.print_epoch_result(completed_epochs, c_loss, self.combined.metrics_names, )
            self.sample_epoch(completed_epochs, file_prefix="combined_")

    def train_generator(self, data):
        X_train = np.array(data)

        completed_epochs = 0
        while(completed_epochs < self.epochs):
            idx = np.random.randint(0, len(X_train), self.batch_size)
            sample = X_train[idx]
            noise = self.get_noise(self.batch_size)
            
            g_loss = self.generator.fit(noise, sample, 
                batch_size=self.batch_size, 
                epochs=completed_epochs + self.sample_interval, 
                initial_epoch=completed_epochs)
            
            completed_epochs += self.sample_interval

            self.add_train_info("\nGenerator training.")
            self.print_epoch_result(completed_epochs, g_loss, self.generator.metrics_names)
            self.sample_epoch(completed_epochs, file_prefix="generator_")

    def train_discriminator(self, data, epochs=None, noise=None):
        if epochs is None:
            epochs = self.epochs
        if noise is None:
            noise = self.get_noise(self.batch_size)

        self.add_train_info("\nDiscriminator training.")
        X_train = np.array(data)
        
        valid = np.ones((self.batch_size, 1))
        fake = np.zeros((self.batch_size, 1))

        idx = np.random.randint(0, len(X_train), self.batch_size)
        sample = X_train[idx]
        dungeon = self.generator.predict(noise)

        X, y = np.vstack((sample, dungeon)), np.vstack((valid, fake))
        
        d_loss = self.discriminator.fit(X, y, epochs=epochs, verbose=1, batch_size=self.batch_size)
        self.print_epoch_result(epochs, d_loss, self.discriminator.metrics_names, epochs)
    
    def get_noise(self, number_of_samples):
        # generate points in the latent space
        x_input = np.random.randn(self.latent_dim * number_of_samples)
        # reshape into a batch of inputs for the network
        x_input = x_input.reshape(number_of_samples, self.latent_dim)
        return x_input

    def add_layer(self, layer):
        self.model.add(layer)

        layer_info = str("Class_name: " + str(layer.__class__.__name__)
                       + "\tConfig: " + str(layer.get_config()))
        self.str_outputs.append(layer_info)

    # Sample functions
    def sample_epoch(self, epoch, file_prefix=""):
        self.sample_dungeon(epoch, file_prefix)

    def sample_dungeon(self, epoch, file_prefix=""):
        #noise = self.get_noise(1)
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
                value = dungeon[i][j][0]
                if value == 0:
                    value = di.colors['brown']['float']
                elif value == 1:
                    value = di.colors['white']['float']
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


    def print_epoch_result(self, epoch, result, metrics_names, epochs=None):
        if epochs is None:
            epochs = self.sample_interval

        metric_0 = np.sum(result.history[metrics_names[0]]) / epochs
        metric_1 = np.sum(result.history[metrics_names[1]]) / epochs
        str_results = "%d [%s: %f, %s: %f]" % (epoch, 
            metrics_names[0], metric_0,
            metrics_names[1], metric_1)
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