using System.Text.Json;
using System.IO;
using VectorLibrary.Models;
using VectorLibrary.Utils;
using VectorLibrary.Interfaces;

namespace VectorLibrary.Collections
{
    public class VectorCollection<T> where T : IVector
    {
        private readonly int dimensions;
        private List<T> objects = new List<T>();


        public async Task SaveToDiskAsync(string path)
        {
            string json = JsonSerializer.Serialize(objects, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(path, json);
        }


        public static async Task<VectorCollection<T>> CreateFromDiskAsync(string path)
        {
            string jsonFromFile = await File.ReadAllTextAsync(path);            
            List<T> loadedObjects = JsonSerializer.Deserialize<List<T>>(jsonFromFile) ?? new List<T>();
            var collection = new VectorCollection<T>(1536);
            collection.AddRange(loadedObjects);
            return collection;
        }


        public VectorCollection(int dimensions)
        {
            this.dimensions = dimensions;
        }

        public int Dimensions => dimensions;

        public void Add(T obj)
        {
            objects.Add(obj);
        }

        public void AddRange(IEnumerable<T> _objects
        )
        {
            objects.AddRange(_objects);
        }

        public IVector GetItem(int index)
        {
            return objects[index];
        }

        private delegate float ComparisonStrategy(float[] vectorA, float[] vectorB);

        /*
        * This method is used to find the best match for a given query vector.
        * The strategy parameter is used to determine which comparison strategy to use.
        * The isMaxBetter parameter is used to determine whether the best match is the highest or lowest value. (future use)
        * The vectorSelector parameter is used to select the vector to compare against the query vector.
        */
        private SearchResult FindBestMatch(float[] query, Func<T, float[]> vectorSelector, ComparisonStrategy strategy)
        {
            // measure the time it take for Search
            long start = DateTime.Now.Ticks; 
            float bestValue = float.MinValue;
            int bestIndex = 0;

            for (int i = 0; i < objects.Count; i++)
            {
                float currentValue = strategy(vectorSelector(objects[i]), query);
                if (currentValue > bestValue)
                {
                    bestValue = currentValue;
                    bestIndex = i;
                }
            }
            long endtime = DateTime.Now.Ticks;

            return new SearchResult(objects[bestIndex], bestValue, (float)(endtime - start) / TimeSpan.TicksPerMillisecond);
        }
    
        public SearchResult FindByDotProduct(float[] query, Func<T, float[]> vectorSelector)
        {
            return FindBestMatch(query, vectorSelector, VectorMath.DotProduct);
        }

        public SearchResult FindByCosineSimilarity(float[] query, Func<T, float[]> vectorSelector)
        {
            return FindBestMatch(query, vectorSelector, VectorMath.CosineSimilarity);
        }

        public SearchResult FindByEuclideanDistance(float[] query, Func<T, float[]> vectorSelector)
        {
            // we negate the distance to use the isMaxBetter logic
            return FindBestMatch(query, vectorSelector, (a, b) => -VectorMath.EuclideanDistance(a, b)); 
        }      
    }
}