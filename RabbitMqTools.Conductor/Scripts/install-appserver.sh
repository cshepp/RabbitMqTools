#!/bin/bash

# Download the consumer and producer
mkdir Consumer
mkdir Producer
wget https://github.com/cshepp/RabbitMqTools/releases/download/v0.0.1/RabbitMqTools.Consumer.tar.gz -O ./Consumer/RabbitMqTools.Consumer.tar.gz
wget https://github.com/cshepp/RabbitMqTools/releases/download/v0.0.1/RabbitMqTools.Producer.tar.gz -O ./Producer/RabbitMqTools.Producer.tar.gz

cd ~/Consumer
tar -xzf RabbitMqTools.Consumer.tar.gz
cd ~/Producer
tar -xzf RabbitMqTools.Producer.tar.gz
cd ~

chmod +x ./Consumer/RabbitMqTools.Consumer
chmod +x ./Producer/RabbitMqTools.Producer
