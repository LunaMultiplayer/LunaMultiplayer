namespace uhttpsharp
{
    public interface IHttpMethodProvider
    {
        HttpMethods Provide(string name);
    }
}