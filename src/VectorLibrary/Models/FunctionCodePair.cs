using VectorLibrary.Interfaces;

namespace VectorLibrary.Models
{
    public class FunctionCodePair : IVector
    {
        public string Function { get; set; }
        public string Code { get; set; }
        public float[] ? FunctionVector { get; set; }     
        public int Id { get; set; }
        public FunctionCodePair(int id, string function, string code)
        {
            Id = id;
            Function = function;
            Code = code;  
        }
        public float[] GetVector()
        {
            return FunctionVector ?? throw new InvalidOperationException("FunctionVector is not set.");
        }

    }
}