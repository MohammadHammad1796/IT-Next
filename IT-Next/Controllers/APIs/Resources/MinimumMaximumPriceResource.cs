namespace IT_Next.Controllers.APIs.Resources
{
    public class MinimumMaximumPriceResource
    {
        public float Minimum { get; set; }

        public float Maximum { get; set; }

        public MinimumMaximumPriceResource()
        {
            Minimum = 0;
            Maximum = 0;
        }

        public MinimumMaximumPriceResource(float minimum, float maximum)
        {
            Minimum = minimum;
            Maximum = maximum;
        }
    }
}