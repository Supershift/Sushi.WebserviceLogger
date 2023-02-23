# Sushi.WebserviceLogger.Persisters
A webservice logger persister allows you to save a LogItem to a backingstore, like ElasticSearch. 
It is a required dependency for Logger instances.
## Using a persister
You can request an ILogItemPersister in the constructor of your class and then use it in your methods.
```csharp
public class MyClass
{
    private ILogItemPersister _logItemPersister;

    public MyClass(ILogItemPersister logItemPersister)
    {
        _logItemPersister = logItemPersister;
    }
    
    public async Task DoSomeWork()
    {
        // create a log item
        var logItem = new LogItem();

        // set your values here

        // save it
        await _logItemPersister.StoreLogItemAsync(logItem, "webservicelogs-mylogs");
    }
}
```

## In process persister
The InProcessPersister directly inserts a LogItem instance to ElasticSearch when it is called. It is a simple solution, but it does not scale very well when you have a lot of traffic.
Registration:
```csharp
// using an elastic client instance
var client = new Nest.ElasticClient();
services.AddInProcessPersister(client);

// using a callback to create client instance
services.AddInProcessPersister( () => {
    var client = new Nest.ElasticClient();
    return client;
})
```
## Queue persister
The QueuePersister inserts LogItems into a queue. Another process can read from the queue and insert multiple items in batches. This allows for a much more scalable solution. It requires a seperate process to read from the queue.

### Register queue persister and queue processor
```csharp
var client = new Nest.ElasticClient();
services.AddQueuePersister(client);
```
This registers a QueuePersister and a QueueProcessorHostedService. The hosted service reads from the queue and inserts the items in batches into elastic. The hosted service can be configured with a function:
```csharp
var client = new Nest.ElasticClient();
services.AddQueuePersister(client, (o) => {
    // insert in batches of maximum 200 items
    o.MaxBatchSize = 200;
    // if the queue is empty, the hosted service sleeps for this amount of time before checking again
    o.WaitTime = TimeSpan.FromSeconds(15);
    // this is called when an exception occurs when trying to insert items into elastic. 
    // inserting of items is NOT retried
    o.ExceptionDelegate = (e) => { // do something with exception  }
});
```
