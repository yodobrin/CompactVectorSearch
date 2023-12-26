using VectorLibrary.Interfaces;

namespace VectorLibrary.Models
{
    public class SearchResult
    {
        public IVector Item { get; set; }
        public float Value { get; set; }
        public float ms {get; set;}

        public SearchResult(IVector item, float value, float ms)
        {
            Item = item;
            Value = value;
            this.ms = ms;
        }
    }
}