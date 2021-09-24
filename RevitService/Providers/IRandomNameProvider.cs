namespace RevitService.Providers
{
    public interface IRandomNameProvider
    {
        (string name, string fullpath) Next(string extension);
    }
}
