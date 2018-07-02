#!/bin/bash

sudo apt-get update
sudo apt-get -y upgrade

#
# Install Erlang
#
cd ~
wget http://packages.erlang-solutions.com/site/esl/esl-erlang/FLAVOUR_1_general/esl-erlang_20.1-1~ubuntu~xenial_amd64.deb
sudo dpkg -i esl-erlang_20.1-1\~ubuntu\~xenial_amd64.deb

#
# Install RabbitMQ
#
echo "deb https://dl.bintray.com/rabbitmq/debian xenial main" | sudo tee /etc/apt/sources.list.d/bintray.rabbitmq.list
wget -O- https://www.rabbitmq.com/rabbitmq-release-signing-key.asc | sudo apt-key add -
sudo apt-get update
sudo apt-get install -f -y
sudo apt-get install rabbitmq-server -y

#
# Deploy the same Erlang Cookie to all nodes
#
sudo chmod 600 /var/lib/rabbitmq/.erlang.cookie
sudo chown root:root /var/lib/rabbitmq/.erlang.cookie
echo "COOKIEMONSTER" | sudo tee /var/lib/rabbitmq/.erlang.cookie
sudo chown rabbitmq:rabbitmq /var/lib/rabbitmq/.erlang.cookie

#
# Start RabbitMQ
#
sudo systemctl start rabbitmq-server.service
sudo systemctl enable rabbitmq-server.service
sudo systemctl restart rabbitmq-server.service
sudo rabbitmqctl status

#
# Enable Management interface
#
sudo rabbitmq-plugins enable rabbitmq_management

#
# Create admin account
#
sudo rabbitmqctl add_user <Username> <Password>
sudo rabbitmqctl set_user_tags <Username> administrator
sudo rabbitmqctl set_permissions -p / <Username> ".*" ".*" ".*"
