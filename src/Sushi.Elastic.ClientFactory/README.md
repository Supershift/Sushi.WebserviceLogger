# Sushi.Elastic.ClientFactory
The ElasticClientFactory allows you to register a singleton instance of Nest.IElasticClient for different configurations. This allows you to connect to multiple ElasticSearch servers.
An instance is registered for each configuration.
## Registration
You can register an ElasticClient instance or use a callback function where you return an instance.
Register instance:
```csharp
var connection = new Elasticsearch.Net.InMemoryConnection();
var elasticClient = new Nest.ElasticClient(new Nest.ConnectionSettings(connection));
services.AddElasticClient("myName", elasticClient);
```
Register with callback:
```csharp
services.AddElasticClient("myName", () => 
{
	var connection = new Elasticsearch.Net.InMemoryConnection();
	var elasticClient = new Nest.ElasticClient(new Nest.ConnectionSettings(connection));
	return elasticClient;
});
```
The callback is called once, when the first instance is needed.
## Use client
You can inject the factory into your constructor and get an instance there:
```csharp
public class MyClass
{
	private Nest.IElasticClient _elasticClient;

	public MyClass(ElasticClientFactory elasticClientFactory)
	{		
		_elasticClient = elasticClientFactory.GetClient(Common.ElasticClientName);		
	}
}
```