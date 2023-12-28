using Azure.Storage.Blobs;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using CsvHelper;
using CsvHelper.Configuration;
using Azure.AI.OpenAI;

namespace VectorLibrary
{
    public static class StoreUtility
    {

        public static async Task SaveFunctionCodePairsToAzureBlobAsync(List<FunctionCodePair> functionCodePairs, string jsonFileName)
        {
           // Retrieve connection string and container name from environment variables
            string connectionString = Environment.GetEnvironmentVariable("BLOB_STORAGE_CS")  ?? "BLOB_STORAGE_CS not found";
            string containerName = Environment.GetEnvironmentVariable("BLOB_CONTAINER_NAME") ?? "BLOB_CONTAINER_NAME not found";
            if (connectionString == "BLOB_STORAGE_CS not found" || containerName == "BLOB_CONTAINER_NAME not found" || string.IsNullOrEmpty(jsonFileName))
            {
                Console.WriteLine("One or more environment variables are not set. Please set BLOB_STORAGE_CS, BLOB_CONTAINER_NAME and jsonFileName");
                throw new Exception("One or more environment variables are not set. Please set BLOB_STORAGE_CS, BLOB_CONTAINER_NAME and jsonFileName");
            }

            // Serialize the List<FunctionCodePair> to JSON with indentation for human readability
            var json = JsonSerializer.Serialize(functionCodePairs, new JsonSerializerOptions { WriteIndented = true });


            // Create a BlobServiceClient to interact with Blob storage
            var blobServiceClient = new BlobServiceClient(connectionString);
            var blobContainerClient = blobServiceClient.GetBlobContainerClient(containerName);
            
            // Get a reference to the BlobClient
            var blobClient = blobContainerClient.GetBlobClient(jsonFileName);

            // Convert JSON data to a byte array
            byte[] byteArray = Encoding.UTF8.GetBytes(json);
            using var memoryStream = new MemoryStream(byteArray);

            // Upload the data to the blob
            await blobClient.UploadAsync(memoryStream, true);
            Console.WriteLine("Uploaded to Blob storage: " + jsonFileName);
        }
        public static async Task<List<FunctionCodePair>> LoadFunctionCodePairsFromAzureBlobAsync(string blobFileName, OpenAIClient openAIClient, string embeddingDeploymentName)
        {
            // Retrieve connection string and container name from environment variables
            string connectionString = Environment.GetEnvironmentVariable("BLOB_STORAGE_CS")  ?? "BLOB_STORAGE_CS not found";
            string containerName = Environment.GetEnvironmentVariable("BLOB_CONTAINER_NAME") ?? "BLOB_CONTAINER_NAME not found";
            if ( openAIClient==null || connectionString == "BLOB_STORAGE_CS not found" 
                || containerName == "BLOB_CONTAINER_NAME not found" || string.IsNullOrEmpty(blobFileName) 
                || string.IsNullOrEmpty(embeddingDeploymentName))
            {
                Console.WriteLine("One or more environment variables are not set, or the OpenAi client is null. Please set BLOB_STORAGE_CS, BLOB_CONTAINER_NAME and blobFileName");
                throw new Exception("One or more environment variables are not set. Please set BLOB_STORAGE_CS, BLOB_CONTAINER_NAME and blobFileName");
            }
            // Create a BlobServiceClient to interact with Blob storage
            var blobServiceClient = new BlobServiceClient(connectionString);
            var blobContainerClient = blobServiceClient.GetBlobContainerClient(containerName);
            var blobClient = blobContainerClient.GetBlobClient(blobFileName);

            // Check if the blob exists
            if (await blobClient.ExistsAsync())
            {
                // Download the blob's content to a MemoryStream
                using var memoryStream = new MemoryStream();
                await blobClient.DownloadToAsync(memoryStream);
                memoryStream.Position = 0; // Reset the memory stream position to the beginning

                // Create a StreamReader from the MemoryStream
                using var reader = new StreamReader(memoryStream);

                // Call the private method to process the data from the stream
                var functionCodePairs = await LoadFunctionCodePairsFromStreamAsync(reader, openAIClient, embeddingDeploymentName);

                return functionCodePairs;
            }
            else
            {
                throw new FileNotFoundException("Blob not found: " + blobFileName);
            }
        }

        public static async Task<List<FunctionCodePair>> LoadFunctionCodePairsFromCsvAsync(string csvFilePath, OpenAIClient openAIClient, string embeddingDeploymentName)
        {
            // Create a StreamReader from the file path
            using var fileStream = new FileStream(csvFilePath, FileMode.Open, FileAccess.Read);
            using var reader = new StreamReader(fileStream);

            // Call the private method to process the data from the stream
            var functionCodePairs = await LoadFunctionCodePairsFromStreamAsync(reader, openAIClient, embeddingDeploymentName);

            return functionCodePairs;
        }
        private static async Task<List<FunctionCodePair>> LoadFunctionCodePairsFromStreamAsync(StreamReader streamReader, OpenAIClient openAIClient, string embeddingDeploymentName)
        {
            var rows = new List<FunctionCodePair>();
            var config = new CsvConfiguration(CultureInfo.InvariantCulture) { HasHeaderRecord = true };
            EmbeddingsOptions embeddingsOptions;

            // Use the provided StreamReader
            using var csv = new CsvReader(streamReader, config);
            csv.Read();
            csv.ReadHeader();
            while (csv.Read())
            {
                string function = csv.GetField<string>("FunctionDescription") ?? string.Empty;
                string code = csv.GetField<string>("FunctionCode") ?? string.Empty;
                var record = new FunctionCodePair(csv.GetField<int>("id"), function, code);
                embeddingsOptions = new EmbeddingsOptions(embeddingDeploymentName, new List<string> { record.Function });
                var embeddingsResponse = await openAIClient.GetEmbeddingsAsync(embeddingsOptions);
                record.FunctionVector = embeddingsResponse.Value.Data[0].Embedding.ToArray();
                rows.Add(record);
            }

            return rows;
        }

        public static async Task SaveVectorCollectionAsync(VectorCollection collection, string jsonFilePath)
        {
            await collection.SaveToDiskAsync(jsonFilePath);
        }

        public static async Task<VectorCollection> LoadVectorCollectionAsync(string jsonFilePath)
        {
            return await VectorCollection.CreateFromDiskAsync(jsonFilePath);
        }
    }
}