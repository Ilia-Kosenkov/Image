namespace ImageCore
{
    public interface ISubImage
    {
        long Size { get; }
        double Min();
        double Max();
        double Percentile(double lvl);
       
    }
}
