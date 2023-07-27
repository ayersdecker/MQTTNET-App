using MQTTnet.Client;
using MQTTnet;
using MQTTnet.LowLevelClient;
using System.Xml.Linq;
using System.Reflection.Emit;
using Newtonsoft.Json;

namespace MQTTApp;

public partial class MainPage : ContentPage
{

	public MainPage()
	{
		InitializeComponent();
        //Connect_Client();
        Connect_With_Amazon_AWS();
        Publish_Application_Message();
        Handle_Received_Application_Message();
	}
    public async Task Connect_With_Amazon_AWS()
    {
        /*
         * This sample creates a simple MQTT client and connects to an Amazon Web Services broker.
         *
         * The broker requires special settings which are set here.
         */

        var mqttFactory = new MqttFactory();

        using (var mqttClient = mqttFactory.CreateMqttClient())
        {
            var mqttClientOptions = new MqttClientOptionsBuilder()
                .WithTcpServer("a3qo6u2n7mm4fw-ats.iot.us-east-1.amazonaws.com")
                .WithClientId("iotconsole-c5d9beb4-d1a2-47ce-9e87-50121a0a063a")
                // Disabling packet fragmentation is very important!  
                .WithoutPacketFragmentation()
                .Build();

            await mqttClient.ConnectAsync(mqttClientOptions, CancellationToken.None);

            Console.WriteLine("The MQTT client is connected.");

            ServerLabel.Text += mqttClient.ToString();

            await mqttClient.DisconnectAsync();
        }
    }
    public async Task Connect_Client()
    {
        /*
         * This sample creates a simple MQTT client and connects to a public broker.
         *
         * Always dispose the client when it is no longer used.
         * The default version of MQTT is 3.1.1.
         */

        var mqttFactory = new MqttFactory();

        using (var mqttClient = mqttFactory.CreateMqttClient())
        {
            // Use builder classes where possible in this project.
            var mqttClientOptions = new MqttClientOptionsBuilder().WithTcpServer("broker.hivemq.com").Build();

            // This will throw an exception if the server is not available.
            // The result from this message returns additional data which was sent 
            // from the server. Please refer to the MQTT protocol specification for details.
            var response = await mqttClient.ConnectAsync(mqttClientOptions, CancellationToken.None);

            Console.WriteLine("The MQTT client is connected.");


            ServerLabel.Text += response.ResultCode;

            // Send a clean disconnect to the server by calling _DisconnectAsync_. Without this the TCP connection
            // gets dropped and the server will handle this as a non clean disconnect (see MQTT spec for details).
            var mqttClientDisconnectOptions = mqttFactory.CreateClientDisconnectOptionsBuilder().Build();

            await mqttClient.DisconnectAsync(mqttClientDisconnectOptions, CancellationToken.None);
        }
    }
    public async Task Publish_Application_Message()
    {
        /*
         * This sample pushes a simple application message including a topic and a payload.
         *
         * Always use builders where they exist. Builders (in this project) are designed to be
         * backward compatible. Creating an _MqttApplicationMessage_ via its constructor is also
         * supported but the class might change often in future releases where the builder does not
         * or at least provides backward compatibility where possible.
         */

        var mqttFactory = new MqttFactory();

        using (var mqttClient = mqttFactory.CreateMqttClient())
        {
            var mqttClientOptions = new MqttClientOptionsBuilder()
                .WithTcpServer("a3qo6u2n7mm4fw-ats.iot.us-east-1.amazonaws.com")
                .Build();

            await mqttClient.ConnectAsync(mqttClientOptions, CancellationToken.None);

            var applicationMessage = new MqttApplicationMessageBuilder()
                .WithTopic("message")
                .WithPayload("19.5")
                .Build();

            await mqttClient.PublishAsync(applicationMessage, CancellationToken.None);

            await mqttClient.DisconnectAsync();

            Console.WriteLine("MQTT application message is published.");
            ServerLabel.Text += "\n" + Entry.Text;
        }
    }
     public async Task Handle_Received_Application_Message()
    {
        string en = "";
        /*
         * This sample subscribes to a topic and processes the received message.
         */

        var mqttFactory = new MqttFactory();

        using (var mqttClient = mqttFactory.CreateMqttClient())
        {
            var mqttClientOptions = new MqttClientOptionsBuilder().WithTcpServer("a3qo6u2n7mm4fw-ats.iot.us-east-1.amazonaws.com").WithClientId("iotconsole-c5d9beb4-d1a2-47ce-9e87-50121a0a063a").Build();

            // Setup message handling before connecting so that queued messages
            // are also handled properly. When there is no event handler attached all
            // received messages get lost.
            mqttClient.ApplicationMessageReceivedAsync += e =>
            {
                Console.WriteLine("Received application message.");
                en = e.ReasonCode.ToString();

                return Task.CompletedTask;
            };

            await mqttClient.ConnectAsync(mqttClientOptions, CancellationToken.None);

            var mqttSubscribeOptions = mqttFactory.CreateSubscribeOptionsBuilder()
                .WithTopicFilter(
                    f =>
                    {
                        f.WithTopic("message");
                    })
                .Build();

            await mqttClient.SubscribeAsync(mqttSubscribeOptions, CancellationToken.None);

            Console.WriteLine("MQTT client subscribed to topic.");

            Console.WriteLine("Press enter to exit.");
            
            ServerLabel.Text += en;
        }
    }

    private async void Button_Clicked(object sender, EventArgs e)
    {
        Publish_Application_Message();
    }
}

