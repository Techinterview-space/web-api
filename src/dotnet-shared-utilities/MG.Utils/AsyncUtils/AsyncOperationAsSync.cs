using System;
using System.Threading;
using System.Threading.Tasks;

namespace MG.Utils.AsyncUtils
{
    public class AsyncOperationAsSync
    {
        private static readonly TaskFactory _factory = new
            TaskFactory(
                default(CancellationToken),
                TaskCreationOptions.None,
                TaskContinuationOptions.None,
                TaskScheduler.Default);

        private readonly Func<Task> _task;

        public AsyncOperationAsSync(Func<Task> task)
        {
            _task = task ?? throw new ArgumentNullException(paramName: nameof(task));
        }

        public void Execute()
        {
            try
            {
                _factory.StartNew(_task).Unwrap().GetAwaiter().GetResult();
            }
            catch (Exception exception)
            {
                throw new InvalidOperationException(
                    $"Async operation has thrown the exception: {exception.Message}",
                    exception);
            }
        }
    }
}