# ObjectNetwork
ObjectNetwork abstracts most network code(event handlers, serialization, encryption, etc.) to using a few very simple attributes.

### Why
When working with Socket code directly or most other network libraries, code can quickly become messy and
 contain too much boilerplate code which makes it harder to read and therefore much harder to maintain
 
 
 ### Quick start
 Add a [Connect], [Command] or [Disconnect] attribute to a method to mark it to be called when an object has been received. The ObjectConnection parameter is optional but if it is present it must be the first argument. When the Command attribute is used you must define which object is sent over the network as the last parameter.
```csharp
public class ExampleCommandHandler
{
    [Connect]
    public void OnConnect(ObjectConnection connection)
    {
        connection.SendObject(new MessageObject{Content="Hello World!"});
    }
    
    [Disconnect]
    public void OnDisconnect(ObjectConnection connection)
    {
    
    }
    
    [Command]
    public void OnMessageReceived(ObjectConnection connection, MessageObject object)
    {
    
    }
    
    [Command]
    public void OnPlayerJoined(PlayerJoinedObject object)
    {
        // some logic
    }
}
```
#### Client init example
```csharp
ObjectConnection connection = ConnectionBuilder.CreateClient("127.0.0.1",25000);
connection.AddCommandHandler(new ExampleCommandHandler());
connection.Start();
```

#### Server init example
```csharp
IConnectionListener listener = ConnectionBuilder.CreateServer(25000);
listener.OnInboundConnection += (connection) => 
{
    connection.AddCommandHandler(new ExampleCommandHandler);
    connection.Start();
};
```
