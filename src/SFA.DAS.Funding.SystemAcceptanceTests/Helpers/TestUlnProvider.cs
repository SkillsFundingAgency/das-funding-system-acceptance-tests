using System.Collections.Concurrent;

namespace SFA.DAS.Funding.SystemAcceptanceTests.Helpers;

public static class TestUlnProvider
{
    private static bool _isInitialised = false;
    private static ConcurrentQueue<string> _values = new ConcurrentQueue<string>();
    private static readonly object _lock = new();

    public static List<string> Initialise(int numberOfUlns)
    {
        lock (_lock)
        {
            if (_isInitialised)
                throw new Exception("TestUlnProvider has already been initialised.");

            var ulns = new List<string>();

            for (int i = 0; i < numberOfUlns; i++)
            {
                var uln = GenerateRandomUln();
                _values.Enqueue(uln);
                ulns.Add(uln);
            }

            _isInitialised = true;

            return ulns;
        }
    }

    internal static string GetNext()
    {
        string? uln;

        if (_values.TryDequeue(out uln))
        {
            return uln;
        }

        throw new Exception("No more ULNs available. Please generate more.");
    }

    private static String GenerateRandomUln()
    {
        String randomUln = GenerateRandomNumberBetweenTwoValues(10, 99).ToString()
            + DateTime.Now.ToString("ssffffff");

        for (int i = 1; i < 30; i++)
        {
            if (IsValidCheckSum(randomUln))
            {
                return randomUln;
            }
            randomUln = (long.Parse(randomUln) + 1).ToString();
        }
        throw new Exception("Unable to generate ULN");
    }

    private static int GenerateRandomNumberBetweenTwoValues(int min, int max) => new Random().Next(min, max);

    private static bool IsValidCheckSum(string uln)
    {
        var ulnCheckArray = uln.ToCharArray()
                                .Select(c => long.Parse(c.ToString()))
                                .ToList();

        var multiplier = 10;
        long checkSumValue = 0;
        for (var i = 0; i < 10; i++)
        {
            checkSumValue += ulnCheckArray[i] * multiplier;
            multiplier--;
        }

        return checkSumValue % 11 == 10;
    }

}
