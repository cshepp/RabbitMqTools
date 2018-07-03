#!/bin/bash

sudo rabbitmq-plugins enable rabbitmq_federation
sudo rabbitmq-plugins enable rabbitmq_federation_management

sudo rabbitmqctl set_parameter federation-upstream upstream '{"uri": [ <FQDN_LIST> ]}'
sudo rabbitmqctl set_policy --apply-to exchanges federate-me "^test\." '{"federation-upstream-set": "all"}'
