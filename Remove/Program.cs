const int threadCount = 10;
const int amountOfLogins = 10000;
const int amountOfPasswords = 10000;
var cts = new CancellationTokenSource();
var token = cts.Token;
/*---------- IEnumerable -----------*/
IEnumerable<string> allLogins = EGenerateLogOrPass(amountOfLogins);
IEnumerable<string> allPasswords = EGenerateLogOrPass(amountOfPasswords);
IEnumerable<(string?, string?)> allData = EGenerateLogPassCombinationFromScratch(amountOfLogins, amountOfPasswords);//EGenerateLogPassCombination(allLogins, amountOfLogins, allPasswords, amountOfPasswords);
 Task<(string?, string?)> task1 = EFindLogPass(allData, amountOfLogins * amountOfPasswords, threadCount);
(var l, var p) = task1.Result;
/*---------- Arrays -----------*/
/*var allLogins = GenerateLogOrPass(amountOfLogins);
var allPasswords = GenerateLogOrPass(amountOfPasswords);
var allData = GenerateLogPassCombination(allLogins, allPasswords);
Task<(string?, string?)> task1 = FindLogPass(allData, threadCount);
(var l, var p) = task1.Result;*/


//(var l, var p) = FindLogPass();
//Console.WriteLine(l + " " + p);
Console.WriteLine($"{l} {p}");


async Task<(string?, string?)> FindLogPass((string, string)[] ar, int threadCount)
{
    string resultLog = null!;
    string resultPass = null!;
    int operationsPerThread = ar.Length / threadCount;
    for (int i = 0; i < threadCount; i++)
    {
        if (token.IsCancellationRequested) break;
        await Task.Run(() =>
        {
            Console.WriteLine(Environment.CurrentManagedThreadId);
            for (int j = i * operationsPerThread; j < i * operationsPerThread + operationsPerThread && j < ar.Length; j++)
            {
                if (token.IsCancellationRequested) break;
                if (CheckLogin(ar[j].Item1, ar[j].Item2))
                {
                    resultLog = ar[j].Item1;
                    resultPass = ar[j].Item2;
                    cts.Cancel();
                    break;
                }
            }
        });
    }
    return (resultLog, resultPass); //resultLog, resultPass);
}
(string, string)[] GenerateLogPassCombination(string[] logins, string[] passwords)
{
    (string, string)[] ar = new (string, string)[logins.Length * passwords.Length];
    int l = 0;
    for (int i = 0; i < logins.Length; i++)
    {
        for (int j = 0; j < passwords.Length; j++)
        {
            ar[l] = (logins[i], passwords[j]);
            l++;
        }
    }
    return ar;
}

string[] GenerateLogOrPass(int amount)
{
    string[] ar = new string[amount];
    for (int i = 0; i < amount; i++)
    {
        var some = i.ToString();
        var count = 4;
        var prefixLogin = new string('0', count - some.Length);
        ar[i] = prefixLogin + some;
    }
    return ar;
}

bool CheckLogin(string login, string password)
{
    return login == "9222" && password == "9001";
}

async Task<(string?, string?)> EFindLogPass(IEnumerable<(string?, string?)> genLogPass, int opAmount, int threadCount)
{
    string resultLog = null!;
    string resultPass = null!;
    int amountPerThread = opAmount / threadCount;

    for (int i = 0; i < threadCount; i++)
    {
        if (token.IsCancellationRequested) break;
        _ = Task.Run(async() =>//подчеркивает, когда нет авейта, но когда он есть, код выполняется синхронно?
        {
            Console.WriteLine(Environment.CurrentManagedThreadId);
            var dataTuple = genLogPass.Skip(i * amountPerThread).Take(amountPerThread);
            foreach (var lpTuple in dataTuple)
            {
                if (token.IsCancellationRequested) break;
                if (lpTuple.Item1 != null && lpTuple.Item2 != null)
                {
                    if (CheckLogin(lpTuple.Item1, lpTuple.Item2))
                    {
                        resultLog = lpTuple.Item1;
                        resultPass = lpTuple.Item2;
                        cts.Cancel();
                        break;
                    }
                }
            }
        }, token);
    }
    //await await Task.WhenAny()
    //return await Task.;
    return (resultLog, resultPass);
}


IEnumerable<string> EGenerateLogOrPass(int operationsNumber)
{

    for (var i = 0; i < operationsNumber; i++)
    {
        var some = i.ToString();
        var count = 4;
        var prefixLogin = new string('0', count - some.Length);
        yield return prefixLogin + some;//вернуть IEnumerable
    }
}

IEnumerable<(string?, string?)> EGenerateLogPassCombination(IEnumerable<string> log, int logCount, IEnumerable<string> pass, int passCount)
{

    for (int i = 0; i < logCount; i++)
    {
        for (int j = 0; j < passCount; j++)
        {
            yield return (log.Skip(i).Take(1).FirstOrDefault(), pass.Skip(j).Take(1).FirstOrDefault());
        }
    }
}

IEnumerable<(string?, string?)> EGenerateLogPassCombinationFromScratch(int logCount, int passCount)
{
    var ranks = (logCount-1).ToString().Length;//количество разрядов
    for(int i = 0; i < logCount; i++)
    {
        for(int j = 0; j < passCount; j++)
        {
            string log = convertIntToString(ranks, i);
            string pass = convertIntToString(ranks, j);
            yield return (log, pass);
        }
    }
}

string convertIntToString(int ranks, int number)
{
    return new String('0', ranks - number.ToString().Length) + number;
}















//Parallel.For(0, 10000, (l) =>
//{
//    Console.WriteLine(Thread.CurrentThread.ManagedThreadId);
//    for (var p = 0; p < 10000; p++)
//    {
//        var log = l.ToString();
//        var pass = p.ToString();
//        var count = 4;
//        var prefixLogin = new string('0', count - log.Length);
//        var prefixPass = new string('0', count - pass.Length);
//        log = prefixLogin + log;
//        pass = prefixPass + pass;
//        if (CheckLogin(log, pass))
//        {
//            resultLog = log;
//            resultPass = pass;
//            return;
//        }
//    }
//});