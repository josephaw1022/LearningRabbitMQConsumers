using Amazon.SimpleEmail;
using Amazon.SimpleEmail.Model;
using Amazon;

namespace LearningQueues;

public class Emailer
{

    private string? fromEmail = Environment.GetEnvironmentVariable("AWS_FROM_EMAIL");


    /// <summary>
    /// Create an email message 
    /// </summary>
    /// <param name="message">
    ///  The message object for the email template.
    /// </param>
    /// <returns>
    /// The email message
    /// </returns>
    private string TemplateEmail(EmailMessage message)
    {
        return $@"
            <html>
                <head>
                    <title>{message.Subject}</title>
                </head>
                <body>
                    {message.Body} - {DateTime.Now}
                </body>
            </html>
        ";
    }



    /// <summary>
    /// Send an email using the Amazon Simple Email Service. 
    /// </summary>
    /// <param name="email">
    ///  The email message to send.
    /// </param>
    /// <returns>
    ///  A Task that completes when the email is sent.
    /// </returns>
    public Task SendEmail(EmailMessage email)
    {

        var destination = new Destination
        {
            ToAddresses = new List<string> { email.To }
        };

        var message = new Message
        {
            Subject = new Content(email.Subject),
            Body = new Body
            {
                Html = new Content(TemplateEmail(email))
            }
        };

        var request = new SendEmailRequest
        {
            Source = fromEmail,
            Destination = destination,
            Message = message
        };

        try
        {
            using (var client = new AmazonSimpleEmailServiceClient(new AmazonSimpleEmailServiceConfig { RegionEndpoint = RegionEndpoint.USEast1 }))
            {
                return client.SendEmailAsync(request);
            }
        }
        catch (Exception exception)
        {
            Console.WriteLine(exception.Message);
            return Task.CompletedTask;
        }

    }


}