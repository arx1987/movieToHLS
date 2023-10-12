internal class MyTaskCompletionSource<T>
{
    bool isResultNotNull;
    private T result;
    public MyTaskCompletionSource()
    {
        isResultNotNull = false;
    }

    internal async Task<T> getResult()//Task<T>
    {
        while (isResultNotNull == false)
            ;
        //isResultNotNull = false;
        return result;
    }

    internal void setResult(T something)
    {
        result = something;
        if (result != null) isResultNotNull = true;
    }
}