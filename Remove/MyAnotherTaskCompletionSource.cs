internal class MyAnotherTaskCompletionSource<T>
{
    private T result;
    private readonly SemaphoreSlim sem;
    public MyAnotherTaskCompletionSource()
    {
        sem = new SemaphoreSlim(1);
    }
    internal void setResult(T some)
    {
        result = some;
        sem.Release();
    }
    internal async Task<T> getResult()
    {
        await sem.WaitAsync();
        return result;
    }
}