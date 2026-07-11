using System.Text;
using CsCheck;
using HostFxrSharp.Interop;
using Shouldly;
using Xunit;

namespace HostFxrSharp.Tests;

/// <summary>
/// Tests for the internal <see cref="NativeString"/> marshaller, which encodes and decodes managed
/// strings to the native <c>char_t</c> type: UTF-16 (<c>wchar_t</c>) on Windows and UTF-8 (<c>char</c>)
/// on Linux and macOS. Coverage spans the full text spectrum (ASCII, Latin-1, Greek/Cyrillic, CJK,
/// astral-plane emoji), the null and whitespace edge cases, the documented null-terminator truncation,
/// the platform contract (<see cref="NativeString.IsUtf16"/> / <see cref="NativeString.UnitSize"/>),
/// the raw pointer APIs (<c>Allocate</c> / <c>Read</c> / <c>Decode</c> / <c>Free</c>), and a CsCheck
/// property asserting round-trip identity over many generated Unicode strings.
/// </summary>
public sealed unsafe class NativeStringTests
{
    // A CsCheck generator for strings built only from valid Unicode scalar values: no interior U+0000
    // and no lone surrogates. Each element is drawn from a random Unicode block so the produced strings
    // deliberately mix ASCII with non-ASCII (Latin, Greek, Cyrillic, CJK, and supplementary emoji).
    private static readonly Gen<string> ValidScalarStrings =
        Gen.OneOf(
            Gen.Int[0x0020, 0x007E],    // printable ASCII
            Gen.Int[0x00A0, 0x024F],    // Latin-1 supplement + Latin Extended-A/B
            Gen.Int[0x0370, 0x03FF],    // Greek and Coptic
            Gen.Int[0x0400, 0x04FF],    // Cyrillic
            Gen.Int[0x4E00, 0x9FFF],    // CJK Unified Ideographs
            Gen.Int[0x1F300, 0x1FAFF])  // supplementary symbols and emoji (surrogate pairs)
        .Select(char.ConvertFromUtf32)
        .Array
        .Select(string.Concat);

    // ---- Platform contract ----

    [Fact]
    public void IsUtf16MatchesOperatingSystem()
    {
        NativeString.IsUtf16.ShouldBe(OperatingSystem.IsWindows());
    }

    [Fact]
    public void UnitSizeIsTwoOnWindowsOtherwiseOne()
    {
        var expected = OperatingSystem.IsWindows() ? 2 : 1;
        NativeString.UnitSize.ShouldBe(expected);
    }

    [Fact]
    public void UnitSizeAgreesWithIsUtf16()
    {
        NativeString.UnitSize.ShouldBe(NativeString.IsUtf16 ? 2 : 1);
    }

    // ---- RoundTrip: text preservation across scripts ----

    [Theory]
    [InlineData("Hello, World!")]
    [InlineData("The quick brown fox jumps over the lazy dog 0123456789")]
    [InlineData("~!@#$%^&*()_+-=[]{}|;':\",./<>?`")]
    public void RoundTripPreservesAscii(string value)
    {
        NativeString.RoundTrip(value).ShouldBe(value);
    }

    [Theory]
    [InlineData("café résumé naïve")]
    [InlineData("Zürich straße ñoño çedilla")]
    [InlineData("ÀÈÌÒÙ àèìòù")]
    public void RoundTripPreservesLatin1Accents(string value)
    {
        NativeString.RoundTrip(value).ShouldBe(value);
    }

    [Theory]
    [InlineData("Ελληνικά")]
    [InlineData("Привет, мир!")]
    [InlineData("αβγδε абвгд")]
    public void RoundTripPreservesGreekAndCyrillic(string value)
    {
        NativeString.RoundTrip(value).ShouldBe(value);
    }

    [Theory]
    [InlineData("日本語")]
    [InlineData("中文测试")]
    [InlineData("한국어 안녕")]
    public void RoundTripPreservesCjk(string value)
    {
        NativeString.RoundTrip(value).ShouldBe(value);
    }

    [Theory]
    [InlineData("\U0001F600\U0001F389\U0001F30D")]          // three surrogate pairs
    [InlineData("café \U0001F44D and 中文")]                  // mixed BMP + astral
    [InlineData("\U0001F469‍\U0001F4BB")]              // ZWJ sequence: woman + ZWJ + laptop
    public void RoundTripPreservesEmojiAndSurrogatePairs(string value)
    {
        NativeString.RoundTrip(value).ShouldBe(value);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    [InlineData("\t")]
    [InlineData("\r\n")]
    [InlineData(" \t \n \r \f \v ")]
    public void RoundTripPreservesEmptyAndWhitespace(string value)
    {
        NativeString.RoundTrip(value).ShouldBe(value);
    }

    [Fact]
    public void RoundTripNullReturnsNull()
    {
        NativeString.RoundTrip(null).ShouldBeNull();
    }

    [Theory]
    [InlineData("abc\0def", "abc")]
    [InlineData("trailing\0", "trailing")]
    [InlineData("\0leading", "")]
    [InlineData("a\0b\0c", "a")]
    [InlineData("no interior null", "no interior null")]
    public void RoundTripTruncatesAtFirstEmbeddedNull(string value, string expected)
    {
        // Documented behaviour: char_t strings are null-terminated. Allocate copies the ENTIRE managed
        // string (interior U+0000 included) plus a terminator, but Read decodes only up to the first
        // null unit. Everything at or after an embedded '\0' is therefore silently dropped on the way
        // back out. This is a lossy, one-directional truncation and is asserted here on purpose.
        NativeString.RoundTrip(value).ShouldBe(expected);
    }

    // ---- Raw pointer APIs: Allocate / Read / Free ----

    [Fact]
    public void AllocateNullReturnsNullPointer()
    {
        var buffer = NativeString.Allocate(null);

        ((nint)buffer).ShouldBe(nint.Zero);
    }

    [Fact]
    public void AllocateNonNullReturnsNonNullPointer()
    {
        var buffer = NativeString.Allocate("value");

        try
        {
            ((nint)buffer).ShouldNotBe(nint.Zero);
        }
        finally
        {
            NativeString.Free(buffer);
        }
    }

    [Fact]
    public void ReadNullPointerReturnsNull()
    {
        NativeString.Read(null).ShouldBeNull();
    }

    [Fact]
    public void FreeNullPointerDoesNotThrow()
    {
        Should.NotThrow(static () => NativeString.Free(null));
    }

    [Theory]
    [InlineData("Hello, native world")]
    [InlineData("café \U0001F600 中文 Привет")]
    public void AllocateThenReadRoundTripsThroughPointer(string value)
    {
        var buffer = NativeString.Allocate(value);

        try
        {
            NativeString.Read(buffer).ShouldBe(value);
        }
        finally
        {
            NativeString.Free(buffer);
        }
    }

    [Fact]
    public void ReadStopsAtEmbeddedNullTerminatorThroughPointer()
    {
        var buffer = NativeString.Allocate("visible\0hidden");

        try
        {
            NativeString.Read(buffer).ShouldBe("visible");
        }
        finally
        {
            NativeString.Free(buffer);
        }
    }

    [Fact]
    public void AllocateReturnsIndependentBuffers()
    {
        var first = NativeString.Allocate("shared");
        var second = NativeString.Allocate("shared");

        try
        {
            ((nint)first).ShouldNotBe((nint)second);
            NativeString.Read(first).ShouldBe(NativeString.Read(second));
        }
        finally
        {
            NativeString.Free(first);
            NativeString.Free(second);
        }
    }

    // ---- Raw pointer APIs: Decode ----

    [Fact]
    public void DecodeNullBufferReturnsEmpty()
    {
        NativeString.Decode(null, 5).ShouldBe(string.Empty);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(int.MinValue)]
    public void DecodeNonPositiveUnitCountReturnsEmpty(int units)
    {
        var buffer = NativeString.Allocate("populated");

        try
        {
            NativeString.Decode(buffer, units).ShouldBe(string.Empty);
        }
        finally
        {
            NativeString.Free(buffer);
        }
    }

    [Theory]
    [InlineData("ascii text")]
    [InlineData("café naïve")]
    [InlineData("Ελληνικά")]
    [InlineData("日本語")]
    [InlineData("mix \U0001F600 中文")]
    public void DecodeReconstructsExactUnitCount(string value)
    {
        DecodeThroughPointer(value).ShouldBe(value);
    }

    // ---- Property-based coverage ----

    [Fact]
    public void RoundTripIsIdentityForAllValidScalarStrings()
    {
        // Any string composed solely of valid Unicode scalar values (no lone surrogates, no interior
        // U+0000) must survive the native char_t round trip exactly, on every supported platform.
        ValidScalarStrings.Sample(value =>
        {
            NativeString.RoundTrip(value).ShouldBe(value);
        });
    }

    /// <summary>
    /// Allocates <paramref name="value"/> as native <c>char_t</c>, then decodes exactly the number of
    /// <c>char_t</c> units it occupies on this platform (UTF-16 code units on Windows, UTF-8 bytes on
    /// Unix) and returns the result. Used to exercise <see cref="NativeString.Decode"/> directly.
    /// </summary>
    private static string DecodeThroughPointer(string value)
    {
        var buffer = NativeString.Allocate(value);

        try
        {
            var units = NativeString.IsUtf16
                ? value.Length
                : Encoding.UTF8.GetByteCount(value);

            return NativeString.Decode(buffer, units);
        }
        finally
        {
            NativeString.Free(buffer);
        }
    }
}
