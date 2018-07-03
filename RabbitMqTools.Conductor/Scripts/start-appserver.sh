#!/bin/bash

# Start the consumer and producer as background tasks
./Consumer/RabbitMqTools.Consumer --config ./Consumer/consumer-config.toml & 
./Producer/RabbitMqTools.Producer --config ./Producer/producer-config.toml &
