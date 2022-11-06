using System;
using System.Text; 
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks; 
using RabbitMQ.Client; 
using RabbitMQ.Client.Events;
using Npgsql;

internal class Program
{
    private static async Task Main(string[] args)
    {
        Console.WriteLine("Startup sleep 20s...");
        Thread.Sleep(20000); 
        Console.WriteLine("Sleeping finished... initializing");


        var connectionStringBuilder = new NpgsqlConnectionStringBuilder()
        {
            ConnectionString = "User ID=postgres;Password=postgres;Server=pgdb;Port=5432;Database=postgres;Integrated Security=true;Pooling=false;CommandTimeout=300"
        };
        using (var Db = new NpgsqlConnection(connectionStringBuilder.ConnectionString))
        {
            if (Db != null) 
                Db.Open();
            else 
                Console.WriteLine("Unable to connect to DB");

            var sqlInsert = "INSERT INTO events(uuid, type, body) VALUES (@uuid, @type, CAST(@body as json))";
            var InsertCmd = new NpgsqlCommand(sqlInsert, Db);

            string rabbitConnection = "amqp://guest:guest@rabbitmq:5672/";
            string rabbit_channel = "events";

            ConnectionFactory factory = new ConnectionFactory();
            factory.Uri = factory.Uri = new Uri(rabbitConnection);

            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.QueueDeclare(queue: rabbit_channel,
                                        durable: false,
                                        exclusive: false,
                                        autoDelete: false,
                                        arguments: null);

                var consumer = new EventingBasicConsumer(channel);
                consumer.Received += (model, ea) =>
                {
                    var body = ea.Body.ToArray();
                    var messageContents = Encoding.UTF8.GetString(body);
                    Console.WriteLine(" [x] Received {0}", messageContents);

                    var decoded = JsonSerializer.Deserialize<EventMessage>(messageContents);
                    if (decoded != null)
                    {
                        InsertCmd.Parameters.Clear();
                        InsertCmd.Parameters.AddWithValue("uuid", decoded.uuid);
                        InsertCmd.Parameters.AddWithValue("type", decoded.type != null ? decoded.type : "");
                        InsertCmd.Parameters.AddWithValue("body", decoded.body != null ? decoded.body : "{}");
                        int inserted = InsertCmd.ExecuteNonQuery();
                        if (inserted != 1) Console.WriteLine("Error Inserting {0}", messageContents);
                    }
                    else
                    {
                        Console.WriteLine("Error Deserializing {0}", messageContents);
                    }
                };
                channel.BasicConsume(queue: rabbit_channel,
                                        autoAck: true,
                                        consumer: consumer);
           
                Console.WriteLine("Ready to receive messages");

                Publish(rabbitConnection, rabbit_channel, new EventMessage() { uuid = Guid.NewGuid(), type = "dotnet_receiver", body = JsonSerializer.Serialize( new { message = "ready to receive message" } ) });

                CancellationTokenSource Cts = new();
                await Task.Delay(Timeout.Infinite, Cts.Token).ConfigureAwait(false);
            }
        }
    }

    public static void Publish(string amqpConnectString, string rabbit_channel, EventMessage eventmessage) 
    {
        ConnectionFactory factory = new ConnectionFactory(); 
        factory.Uri = new Uri(amqpConnectString);

        using(var connection = factory.CreateConnection())
        using(var channel = connection.CreateModel())
        {
            channel.QueueDeclare(queue: rabbit_channel,
                                durable: false,
                                exclusive: false,
                                autoDelete: false,
                                arguments: null);

            string message = JsonSerializer.Serialize(eventmessage);
            Console.WriteLine("Publishing: {0}", message); 
            var body = Encoding.UTF8.GetBytes(message);

            channel.BasicPublish(exchange: "",
                                routingKey: rabbit_channel,
                                basicProperties: null,
                                body: body);
        }
    }
}

public class EventMessage
{
    public Guid uuid {get; set;}
    public string? type {get;set;}
    public string? body {get;set;}

}


