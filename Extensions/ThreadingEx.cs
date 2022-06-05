namespace Parser.Extensions
{
    internal static class ThreadingEx
    {
        /// <summary>
        /// Возвращает значения всех завершенных задач за определнный промежуток времени
        /// </summary>
        /// <typeparam name="TResult">Возвращаемое значение</typeparam>
        /// <param name="tasks">Коллекция <see cref="Task"/></param>
        /// <param name="timeout">Время ожидания выполнения задач</param>
        /// <returns>Результат выполнения операции</returns>
        public static async Task<TResult[]> GetResults<TResult>(IEnumerable<Task<TResult>> tasks, int timeout)
        {
            var timeoutTask = Task.Delay(timeout)
                .ContinueWith(_ => default(TResult));

            var completedTasks =
                (await Task.WhenAll(tasks.Select(task => Task.WhenAny(task, timeoutTask!))))
                .Where(task => task != timeoutTask);

            return await Task.WhenAll(completedTasks);
        }
    }
}
