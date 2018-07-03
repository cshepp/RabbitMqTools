#!/bin/bash

#
# Enable clustering
#
sudo rabbitmqctl stop_app

# TODO - template the hostname so we can actually
#        use the correct when in this spot.
sudo rabbitmqctl join_cluster rabbit@rmq-11
sudo rabbitmqctl join_cluster rabbit@rmq-21

sudo rabbitmqctl start_app
