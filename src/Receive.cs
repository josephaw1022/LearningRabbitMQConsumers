
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using Newtonsoft.Json;
using dotenv.net;

namespace LearningQueues;

public class Receive
{

    public static void Main(string[] args)
    {
        DotEnv.Load();

        try
        {
            // Connect to RabbitMQ
            using (var connection = new ConnectionFactory
            {
                HostName = "localhost",
                Port = 5672,
                UserName = "guest",
                Password = "guest",
                Uri = new Uri("amqp://guest:guest@localhost:55010/")
            }
            .CreateConnection())
            {
                // Create a channel to communicate with the queue
                using (var channel = connection.CreateModel())
                {

                    // Declare the exchange
                    channel.QueueDeclare
                    (
                        queue: "hello",
                        durable: false,
                        exclusive: false,
                        autoDelete: false,
                        arguments: null
                    );

                    // Create a consumer
                    var consumer = new EventingBasicConsumer(channel);

                    // Email instance
                    Emailer email = new Emailer();

                    // Register a callback to handle the message
                    consumer.Received += async (model, ea) =>
                    {
                        // Parse the message
                        var message = ParseQueueMessage(ea);

                        // Send the email
                        await Task.Run(() => email.SendEmail(message));
                    };

                    // Start consuming messages
                    channel.BasicConsume
                    (
                        queue: "hello",
                        autoAck: true,
                        consumer: consumer
                    );

                    Console.WriteLine(" Press [enter] to exit.");
                    Console.ReadLine();

                }
            }

        }
        catch (Exception exception)
        {
            Console.WriteLine(exception.Message);
        }
    }




    /// <summary>
    /// Receive messages from the queue and parse them into EmailMessage objects. 
    /// </summary>
    /// <param name="eventArguments">
    ///   The arguments passed to the callback. This is the same as the arguments passed to the delegate.
    /// </param>
    /// <returns>
    ///  The EmailMessage object.
    /// </returns>
    private static EmailMessage ParseQueueMessage(BasicDeliverEventArgs eventArguments)
    {

        // Get the message body in the form of a byte array
        var body = eventArguments.Body.ToArray();

        // Deserialize the message
        var message = Encoding.UTF8.GetString(body);

        // Deserialize the message
        var email = JsonConvert.DeserializeObject<EmailMessage>(message);

        // Ensure the message is valid
        if (email is null)
        {
            throw new Exception("Could not parse message");
        }

        // Print the message
        Console.WriteLine("Received message: " + message);

        return email;
    }

}