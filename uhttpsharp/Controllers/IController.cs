namespace uhttpsharp.Controllers
{
    public interface IController
    {
        IPipeline Pipeline { get; }
    }
}