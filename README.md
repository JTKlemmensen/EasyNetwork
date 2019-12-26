# ObjectNetwork
ObjectNetwork abstracts most network code(event handlers, serialization, encryption, etc.) to using a few very simple attributes.

### Why
When working with Socket code directly or most other network libraries, code can quickly become messy and
 contain too much boilerplate code which makes it harder to read and therefore much harder to maintain
 
 
 ### Quick start
 Add a [Connect], [Command] or [Disconnect] attribute to a method to mark it to be called when an object has been received. The ObjectConnection parameter is always required and must always be the first argument. When the Command attribute is used you must declare which object to subscribe.
```csharp
public class ExampleCommandHandler
{
    //1. This method will be called when a connection has succesfully been established. 
    [Connect]
    public void OnConnect(ObjectConnection connection)
    {
        connection.SendObject(new MessageObject{Content="Hello World!"});
    }
    
    //2. This method will be called when the connection no longer listens for incomming messages.
    [Disconnect]
    public void OnDisconnect(ObjectConnection connection)
    {
        // some logic
    }
    
    //3. This method will be called when a MessageObject is received from the other part of the connection
    [Command]
    public void OnMessageReceived(ObjectConnection connection, MessageObject object)
    {
        connection.Stop();
    }
    
    //4. This method will be called when a PlayerJoinedObject is received from the other part of the connection
    [Command]
    public void OnPlayerJoined(ObjectConnection connection, PlayerJoinedObject object)
    {
        // some logic
    }
}
```
#### Client init example
```csharp
new ConnectionBuilder()
    .AddEventHandler(new ExampleCommandHandler())
    .CreateClient("127.0.0.1", 25000)
    .Start();
```

#### Server init example
```csharp
var listener = new ConnectionBuilder()
    .AddEventHandler(new ExampleCommandHandler())
    .CreateServer(25000);
listener.Start();
```
