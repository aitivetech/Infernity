using Infernity.Framework.Json.Converters;

namespace Infernity.Inference.Abstractions.Models;

[FlagsEnumJsonOptions(true)]
public enum ModelQuantizationType
{
    F32 = 0,
    F16 = 1,
    Q40 = 2,
    Q41 = 3,

    // Q4_2 = 4, support has been removed
    // Q4_3 = 5, support has been removed
    Q50 = 6,
    Q51 = 7,
    Q80 = 8,
    Q81 = 9,
    Q2K = 10,
    Q3K = 11,
    Q4K = 12,
    Q5K = 13,
    Q6K = 14,
    Q8K = 15,
    Iq2Xxs = 16,
    Iq2Xs = 17,
    Iq3Xxs = 18,
    Iq1S = 19,
    Iq4Nl = 20,
    Iq3S = 21,
    Iq2S = 22,
    Iq4Xs = 23,
    I8 = 24,
    I16 = 25,
    I32 = 26,
    I64 = 27,
    F64 = 28,
    Iq1M = 29,
    Bf16 = 30,

    // Q4_0_4_4 = 31, support has been removed from gguf files
    // Q4_0_4_8 = 32,
    // Q4_0_8_8 = 33,
    Tq10 = 34,
    Tq20 = 35,

    // IQ4_NL_4_4 = 36,
    // IQ4_NL_4_8 = 37,
    // IQ4_NL_8_8 = 38,
    Mxfp4 = 39,
}