namespace IronDB.Core.Json.Parsing;

// should never be visible externally
public enum JsonParserTokenContinuation
{
    None = 0,
    PartialNaN = 1 << 23,
    PartialPositiveInfinity = 1 << 24,
    PartialNegativeInfinity = 1 << 25,
    PartialNull = 1 << 26,
    PartialTrue = 1 << 27,
    PartialString = 1 << 28,
    PartialNumber = 1 << 29,
    PartialPreamble = 1 << 30,
    PartialFalse = 1 << 31,
    PleaseRefillBuffer = 1 << 32,
}