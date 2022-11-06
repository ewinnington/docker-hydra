<!-- Taken from https://www.rabbitmq.com/tutorials/tutorial-one-php.html -->
<!-- Requires PHP installation of php-amqplib/php-amqplib --> 

<?php

require_once __DIR__ . '/vendor/autoload.php';
use PhpAmqpLib\Connection\AMQPStreamConnection;
use PhpAmqpLib\Message\AMQPMessage;

$connection = new AMQPStreamConnection('rabbitmq', 5672, 'guest', 'guest');
$channel = $connection->channel();

$channel->queue_declare('events', false, false, false, false);

$msg = new AMQPMessage('{"uuid":"","type":"nginxphp","body":"{"message":"rendered page"}"}');
$channel->basic_publish($msg, '', 'events');

echo "<h2> [PHP] Sent 'page rendered message to rabbit'\n </h2>";

$channel->close();
$connection->close();
?>