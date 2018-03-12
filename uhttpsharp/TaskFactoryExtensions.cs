using System.Threading.Tasks;

namespace uhttpsharp
{
    public static class TaskFactoryExtensions
    {
        private static readonly Task CompletedTask = Task.FromResult<object>(null);

        public static Task GetCompleted(this TaskFactory factory)
        {
            return CompletedTask;
        }

    }
}
