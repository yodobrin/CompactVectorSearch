using System.IO;
using VectorLibrary.Models;
using VectorLibrary.Utils;
using VectorLibrary.Interfaces;
using VectorLibrary.Collections;
using Azure.AI.OpenAI;
using Azure;
using DotNetEnv;



Console.WriteLine("Hello, World!");
string configurationFile = Path.Combine(Directory.GetCurrentDirectory(),  "application.env");
Env.Load(configurationFile);

string oAiApiKey = Environment.GetEnvironmentVariable("SKIT_AOAI_APIKEY") ?? "SKIT_AOAI_APIKEY not found";
string oAiEndpoint = Environment.GetEnvironmentVariable("SKIT_AOAI_ENDPOINT") ?? "SKIT_AOAI_ENDPOINT not found";
string embeddingDeploymentName = Environment.GetEnvironmentVariable("SKIT_EMBEDDING_DEPLOYMENTNAME") ?? "SKIT_EMBEDDING_DEPLOYMENTNAME not found";

AzureKeyCredential azureKeyCredential = new AzureKeyCredential(oAiApiKey);
OpenAIClient openAIClient = new OpenAIClient(new Uri(oAiEndpoint), azureKeyCredential);
Console.WriteLine("OpenAI client created.");

// load from file


string assetsFolder = Directory.GetCurrentDirectory();
string VectorDBPath = Path.Combine(assetsFolder,  "FunctionCollection.json");
// add timing for the next operation
long start = DateTime.Now.Ticks;
VectorCollection<FunctionCodePair> myCollection = await VectorCollection<FunctionCodePair>.CreateFromDiskAsync(VectorDBPath);
long end = DateTime.Now.Ticks;
Console.WriteLine($"Loading the VectorDB took {(end - start) / TimeSpan.TicksPerMillisecond} ms");

string question = "i am seeking to find my team commision data";
var embeddingsOptions = new EmbeddingsOptions(embeddingDeploymentName,new List<string> {question});
var embeddingsResponse = await openAIClient.GetEmbeddingsAsync( embeddingsOptions);
float[] query = embeddingsResponse.Value.Data[0].Embedding.ToArray();
SearchResult nearestByDotProduct = myCollection.FindByDotProduct(query, q => q.GetVector());
// Console.WriteLine($"Near Question (Dot Product): {nearestByDotProduct.Item.Function}");
Console.WriteLine($"Value (Dot Product): {nearestByDotProduct.Value}");
Console.WriteLine($"Time (Dot Product): {nearestByDotProduct.ms} ms");