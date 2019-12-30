![Image of EasyNetwork Logo](https://i.imgur.com/kfECXD0.png)

EasyNetwork simplifies the process of making client and server applications by abstracting most network code(event handlers, serialization, encryption, etc.). It is very unit test friendly.

### Why
When working with Socket code directly or most other network libraries, code can quickly become messy and
 contain too much boilerplate code which makes it harder to read and therefore much harder to maintain.
 
 
 ### Quick start
 Add a [Connect], [Command] or [Disconnect] attribute to a method to mark it to be called when an object has been received. The ObjectConnection parameter is always required and must always be the first argument. When the Command attribute is used you must declare which object to subscribe.
```csharp
public class ExampleEventHandler
{
    //1. This method will be called when a connection has succesfully been established. 
    [Connect]
    public void OnConnect(IObjectConnection connection)
    {
        connection.SendObject(new MessageObject{Content="Hello World!"});
    }
    
    //2. This method will be called when the connection no longer listens for incomming messages.
    [Disconnect]
    public void OnDisconnect(IObjectConnection connection)
    {
        // some logic
    }
    
    //3. This method will be called when a MessageObject is received from the other part of the connection
    [Command]
    public void OnMessageReceived(IObjectConnection connection, MessageObject object)
    {
        connection.Stop();
    }
    
    //4. This method will be called when a PlayerJoinedObject is received from the other part of the connection
    [Command]
    public void OnPlayerJoined(IObjectConnection connection, PlayerJoinedObject object)
    {
        // some logic
    }
}
```
#### Client init example
```csharp
IConnection client = new ConnectionBuilder()
    .CreateClient("127.0.0.1", 25000);
    
client.OnConnect((c)=>Console.WriteLine("Lambda: Client connected"););
client.OnCommand<MessageObject>((c, m)=>Console.WriteLine("Lambda received message: "+m.Content););
client.AddEventHandler(new ExampleEventHandler());
client.Start();
```

#### Server init example
```csharp
IConnectionListener listener = new ConnectionBuilder()
    .CreateServer(25000);
    
listener.OnInboundConnection((c)=>
{
    c.OnConnect((c)=>Console.WriteLine("[Server]Lambda: Client Connected!"););
    c.AddEventHandler(new ExampleEventHandler());
    c.OnDisconnect((c)=>Console.WriteLine("[Server]Lambda: Client disconnected!"););
});

listener.Start();
```
#### Default serialization
EasyNetwork uses the easy built-in way of serializing objects.
```csharp
[Serializable]
public class MessageObject
{
    public string Content {get; set;}
}
```
