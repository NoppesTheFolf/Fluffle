using Noppes.Fluffle.Imaging.Tests.Models;
using Noppes.Fluffle.Utils;
using System.Runtime.Intrinsics.X86;

namespace Noppes.Fluffle.Imaging.Tests;

public interface IImagingTestsExecutor
{
    public void Execute();
}

internal class ImagingTestsExecutor : IImagingTestsExecutor
{
    private readonly Logger _logger;
    private readonly CompleteTestCaseProvider _testCaseProvider;

    public ImagingTestsExecutor(CompleteTestCaseProvider testCaseProvider, Logger logger)
    {
        _testCaseProvider = testCaseProvider;
        _logger = logger;
    }

    public void Execute()
    {
        _logger.Write("Starting imaging tests...");

        foreach (var testCase in _testCaseProvider.Provide())
            CompareTestCase(testCase);

        _logger.Write("Imaging tests finished without errors!");
    }

    private void CompareTestCase(TestCase testCase)
    {
        if (testCase.AllowedDeviationInBits != null)
        {
            var allowedDeviationInBits = (int)testCase.AllowedDeviationInBits;
            CompareHashes(testCase, testCase.ExpectedResult, testCase.ActualResult, allowedDeviationInBits, allowedDeviationInBits, allowedDeviationInBits);
            return;
        }

        var allowedDeviationAsPercentage = (double)testCase.AllowedDeviationAsPercentage!;
        var allowedDeviationInBits64 = (int)Math.Floor(allowedDeviationAsPercentage * 64);
        var allowedDeviationInBits256 = (int)Math.Floor(allowedDeviationAsPercentage * 256);
        var allowedDeviationInBits1024 = (int)Math.Floor(allowedDeviationAsPercentage * 1024);
        CompareHashes(testCase, testCase.ExpectedResult, testCase.ActualResult, allowedDeviationInBits64, allowedDeviationInBits256, allowedDeviationInBits1024);
    }

    private void CompareHashes(TestCase testCase, TestCaseHashes expected, TestCaseHashes actual, int allowedDeviationInBits64, int allowedDeviationInBits256, int allowedDeviationInBits1024)
    {
        CompareBytes(testCase, expected.PhashRed1024, actual.PhashRed1024, allowedDeviationInBits1024);
        CompareBytes(testCase, expected.PhashGreen1024, actual.PhashGreen1024, allowedDeviationInBits1024);
        CompareBytes(testCase, expected.PhashBlue1024, actual.PhashBlue1024, allowedDeviationInBits1024);
        CompareBytes(testCase, expected.PhashAverage1024, actual.PhashAverage1024, allowedDeviationInBits1024);

        CompareBytes(testCase, expected.PhashRed256, actual.PhashRed256, allowedDeviationInBits256);
        CompareBytes(testCase, expected.PhashGreen256, actual.PhashGreen256, allowedDeviationInBits256);
        CompareBytes(testCase, expected.PhashBlue256, actual.PhashBlue256, allowedDeviationInBits256);
        CompareBytes(testCase, expected.PhashAverage256, actual.PhashAverage256, allowedDeviationInBits256);

        CompareBytes(testCase, expected.PhashRed64, actual.PhashRed64, allowedDeviationInBits64);
        CompareBytes(testCase, expected.PhashGreen64, actual.PhashGreen64, allowedDeviationInBits64);
        CompareBytes(testCase, expected.PhashBlue64, actual.PhashBlue64, allowedDeviationInBits64);
        CompareBytes(testCase, expected.PhashAverage64, actual.PhashAverage64, allowedDeviationInBits64);
    }

    private void CompareBytes(TestCase testCase, byte[] expected, byte[] actual, int allowedDeviationInBits)
    {
        ulong mismatchCount = 0;
        foreach (var (actualHashPart, expectedHashPart) in ByteConvert.ToInt64(actual).Zip(ByteConvert.ToInt64(expected.ToArray())))
            mismatchCount += Popcnt.X64.PopCount(actualHashPart ^ expectedHashPart);

        var lengthInBits = 8 * sizeof(byte) * actual.Length;
        var percentageWrong = (int)mismatchCount / (double)lengthInBits;
        var percentageWrongAllowed = allowedDeviationInBits / (double)lengthInBits;

        _logger.Write($"{testCase.Description}@{lengthInBits} | mismatch: {percentageWrong}%, allowed: {Math.Round(percentageWrongAllowed, 5)}%");

        if (percentageWrong > percentageWrongAllowed)
            throw new InvalidOperationException("Hashing did not produce expected result.");
    }
}
