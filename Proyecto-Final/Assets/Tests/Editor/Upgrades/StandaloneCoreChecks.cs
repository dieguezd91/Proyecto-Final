#if MAGIC_GARDEN_STANDALONE
using System;
using System.Reflection;
using NUnit.Framework;

// Safe alternate runner for the same pure-core NUnit cases while the user's Editor holds its lock.
// This is NOT Unity integration or play-mode proof.
public static class StandaloneCoreChecks
{
    public static int Main()
    {
        int passed = 0, failed = 0;
        var suite = new UpgradeCoreTests();
        foreach (var method in typeof(UpgradeCoreTests).GetMethods())
        {
            var cases = method.GetCustomAttributes(typeof(TestCaseAttribute), false);
            if (cases.Length > 0)
                foreach (TestCaseAttribute test in cases) Run(method, suite, test.Arguments, ref passed, ref failed);
            else if (method.IsDefined(typeof(TestAttribute), false)) Run(method, suite, null, ref passed, ref failed);
        }
        Console.WriteLine("Pure core only: " + passed + " passed, " + failed + " failed.");
        return failed == 0 ? 0 : 1;
    }
    private static void Run(MethodInfo method, object suite, object[] args, ref int passed, ref int failed)
    {
        try { method.Invoke(suite, args); passed++; Console.WriteLine("PASS " + method.Name); }
        catch (Exception exception)
        {
            failed++;
            Console.WriteLine("FAIL " + method.Name + ": " + (exception.InnerException ?? exception));
        }
    }
}
#endif
