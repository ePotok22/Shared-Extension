namespace FFF.Shared
{
    public static class DoubleExtensions
    {
        public static double ConvertSecoundToMilliSecound(this double value) =>
            value * 1000;

        public static double ConvertMilliSecoundToSecound(this double value) =>
            value / 1000;
    }
}
