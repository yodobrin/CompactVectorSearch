using VectorLibrary;
using Azure.Storage.Blobs;
using System.IO;
using Azure.AI.OpenAI;
using Azure;

namespace VectorApi
{
    public class VectorDbService
    {
        public VectorCollection ? VectorCollection { get; private set; }
        private OpenAIClient ? _openAIClient;
        private string ? _embeddingDeploymentName;

        public VectorDbService()
        {
            Console.WriteLine("VectorDbService constructor called");            
        }
        public async Task<SearchResult> SearchByDotProduct(string query)
        {
            // check the vector collection is not null throw exception
            if (VectorCollection == null)
            {
                throw new Exception("VectorCollection is null");
            }
            var queryVector = await GetEmbeddings(query);
            return VectorCollection.FindByDotProduct(queryVector, item => item.GetVector());
        }            
        public async Task<SearchResult> SearchByCosineSimilarity(string query)
        {
            // check the vector collection is not null throw exception
            if (VectorCollection == null)
            {
                throw new Exception("VectorCollection is null");
            }
            var queryVector = await GetEmbeddings(query);
            return VectorCollection.FindByCosineSimilarity(queryVector, item => item.GetVector());
        }
        public async Task<SearchResult> SearchByEuclideanDistance(string query)
        {
            // check the vector collection is not null throw exception
            if (VectorCollection == null)
            {
                throw new Exception("VectorCollection is null");
            }
            var queryVector = await GetEmbeddings(query);
            return VectorCollection.FindByEuclideanDistance(queryVector, item => item.GetVector());
        }
        public async Task CreateDatabaseAsync(string inputCsvFileName, string outputJsonFileName)
        {
            // check for null on embeddingDeploymentName & openAIClient throw exception
            if (string.IsNullOrEmpty(_embeddingDeploymentName)  || _openAIClient == null)
            {
                throw new Exception("OpenAI Client or Embedding Deployment Name is null");
            }
            List<FunctionCodePair> functionCodePairs = await StoreUtility.LoadFunctionCodePairsFromAzureBlobAsync(inputCsvFileName,_openAIClient,_embeddingDeploymentName);
            await StoreUtility.SaveFunctionCodePairsToAzureBlobAsync(functionCodePairs, outputJsonFileName);

        }

        public async Task SaveFunctionCodePairsToBlobAsync(string jsonFileName)
        {
            if (VectorCollection == null)
            {
                throw new InvalidOperationException("VectorCollection is not initialized.");
            }
            // Call the method to save the function code pairs to Azure Blob
            await StoreUtility.SaveFunctionCodePairsToAzureBlobAsync(VectorCollection.GetFunctionCodePairs(), jsonFileName);
        }

        private async Task<float[]> GetEmbeddings(string query)
        {
            // null check for embeddingDeploymentName & openAIClient throw exception
            if (_embeddingDeploymentName == null || _openAIClient == null)
            {
                throw new Exception("OpenAI Client or Embedding Deployment Name is null");
            }

            EmbeddingsOptions embeddingsOptions = new EmbeddingsOptions(_embeddingDeploymentName,new List<string> { query });
            var embeddingsResponse = await _openAIClient.GetEmbeddingsAsync(embeddingsOptions);
            return embeddingsResponse.Value.Data[0].Embedding.ToArray();
        }
        private async Task LoadDataFromBlobStorage(string jsonFileName)
        {
            string accountConnectionString = Environment.GetEnvironmentVariable("BLOB_STORAGE_CS")  ?? "BLOB_STORAGE_CS not found";
            string containerName = Environment.GetEnvironmentVariable("BLOB_CONTAINER_NAME") ?? "BLOB_CONTAINER_NAME not found";
            
            // check if any of the above are null
            if (accountConnectionString == "BLOB_STORAGE_CS not found" || containerName == "BLOB_CONTAINER_NAME not found" || string.IsNullOrEmpty(jsonFileName))
            {
                Console.WriteLine("One or more environment variables are not set. Please set BLOB_STORAGE_CS, BLOB_CONTAINER_NAME and BLOB_FILE_NAME");
                return;
            }
            BlobServiceClient blobServiceClient = new BlobServiceClient(accountConnectionString);

            var containerClient = blobServiceClient.GetBlobContainerClient(containerName);
            BlobClient blobClient = containerClient.GetBlobClient(jsonFileName);

            if (blobClient.Exists())
            {
                var response = await blobClient.DownloadContentAsync();
                using var stream = response.Value.Content.ToStream();

                VectorCollection = await VectorCollection.CreateFromMemoryAsync(stream);
            }
            else
            {
                Console.WriteLine("DB Blob does not exist");
            }
        }


        public async Task InitializeAsync(string jsonFileName)
        {
            Console.WriteLine("Initializing VectorDbService & OpenAI Client");
            await LoadDataFromBlobStorage(jsonFileName);
            string oAiApiKey = Environment.GetEnvironmentVariable("SKIT_AOAI_APIKEY") ?? "SKIT_AOAI_APIKEY not found";
            string oAiEndpoint = Environment.GetEnvironmentVariable("SKIT_AOAI_ENDPOINT") ?? "SKIT_AOAI_ENDPOINT not found";
            _embeddingDeploymentName = Environment.GetEnvironmentVariable("SKIT_EMBEDDING_DEPLOYMENTNAME") ?? "SKIT_EMBEDDING_DEPLOYMENTNAME not found";

            AzureKeyCredential azureKeyCredential = new AzureKeyCredential(oAiApiKey);
            _openAIClient = new OpenAIClient(new Uri(oAiEndpoint), azureKeyCredential);
            Console.WriteLine("... Initialized VectorDbService & OpenAI Client !");
        }
    }
}