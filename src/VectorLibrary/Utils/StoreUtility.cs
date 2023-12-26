using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using CsvHelper;
using CsvHelper.Configuration;
using VectorLibrary.Models;
using VectorLibrary.Interfaces;
using VectorLibrary.Collections;
// using Azure; 
using Azure.AI.OpenAI;

namespace VectorLibrary.Utils
{
    public static class StoreUtility
    {
        public static async Task<List<FunctionCodePair>> LoadFunctionCodePairsFromCsvAsync(string csvFilePath, OpenAIClient openAIClient, string embeddingDeploymentName)
        {
            var rows = new List<FunctionCodePair>();
            var config = new CsvConfiguration(CultureInfo.InvariantCulture) { HasHeaderRecord = true };
            EmbeddingsOptions embeddingsOptions;
            using var reader = new StreamReader(csvFilePath);
            using var csv = new CsvReader(reader, config);
            csv.Read();
            csv.ReadHeader();
            while (csv.Read())
            {
                string function = csv.GetField<string>("FunctionDescription") ?? string.Empty;
                string code = csv.GetField<string>("FunctionCode") ?? string.Empty;
                var record = new FunctionCodePair(csv.GetField<int>("id"), function, code);
                embeddingsOptions = new EmbeddingsOptions(embeddingDeploymentName,new List<string> { record.Function });
                var embeddingsResponse = await openAIClient.GetEmbeddingsAsync(embeddingsOptions);
                record.FunctionVector = embeddingsResponse.Value.Data[0].Embedding.ToArray();
                rows.Add(record);
            }

            return rows;
        }

        public static async Task SaveVectorCollectionAsync<T>(VectorCollection<T> collection, string jsonFilePath) where T : IVector
        {
            await collection.SaveToDiskAsync(jsonFilePath);
        }

        public static async Task<VectorCollection<T>> LoadVectorCollectionAsync<T>(string jsonFilePath) where T : IVector
        {
            return await VectorCollection<T>.CreateFromDiskAsync(jsonFilePath);
        }
    }
}