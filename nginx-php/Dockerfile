FROM php:7-fpm

COPY --from=composer/composer:latest-bin /composer /usr/bin/composer

RUN apt-get update && apt-get install -y zip git 

RUN  /usr/bin/composer require php-amqplib/php-amqplib

RUN docker-php-ext-install bcmath

WORKDIR /code