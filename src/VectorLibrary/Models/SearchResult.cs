
namespace VectorLibrary
{
    public class SearchResult
    {
        public FunctionCodePair Item { get; set; }
        public float Value { get; set; }
        public float Ms { get; set; } 

        public SearchResult(FunctionCodePair item, float value, float ms)
        {
            Item = item;
            Value = value;
            Ms = ms;
        }
    }
}
