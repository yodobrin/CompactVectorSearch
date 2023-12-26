namespace VectorLibrary.Utils
{
    public class VectorMath
    {
        private const int VectorDimension = 1536;

        public static float Length(float[] vector)
        {
            float sum = 0;
            for (int i = 0; i < VectorDimension ; i++)
            {
                sum += vector[i] * vector[i];
            }
            return (float)Math.Sqrt(sum);
        }
        public static float DotProduct(float[] a, float[] b)
        {
            float sum = 0;
            for (int i = 0; i < VectorDimension; i++)
            {
                sum += a[i] * b[i];
            }

            return sum;
        }
        public static float CosineSimilarity(float[] a, float[] b)
        {
            float dotProduct = DotProduct(a, b);        
            return dotProduct / (float)Math.Pow(VectorDimension,2);
        }

        public static float EuclideanDistance(float[] a, float[] b)
        {
            float sum = 0;
            for (int i = 0; i < VectorDimension; i++)
            {
                sum += (a[i] - b[i]) * (a[i] - b[i]);
            }
            return (float)Math.Sqrt(sum);
        }
    }
}