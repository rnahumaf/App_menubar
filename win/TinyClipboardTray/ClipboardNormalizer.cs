using System.Text;
using System.Text.Unicode;

namespace TinyClipboardTray;

readonly record struct NormalizationResult(string Text, int Replaced, int Removed)
{
    public int TotalAffected => Replaced + Removed;
}

static class ClipboardNormalizer
{
    public static NormalizationResult NormalizeWithStats(string input)
    {
        var sb = new StringBuilder(input.Length);
        int replaced = 0, removed = 0;

        foreach (var rune in input.EnumerateRunes())
        {
            int u = rune.Value;

            // Preserve CR/LF (não faz nada)

            // Remove: soft hyphen
            if (u == 0x00AD) { removed++; continue; }

            // Remove: format controls e ranges listados
            if (IsFormatControlToRemove(u)) { removed++; continue; }

            // Replace: família NBSP e espaços especiais
            if (IsSpaceLike(u))
            {
                replaced++;
                sb.Append(' ');
                continue;
            }

            switch (u)
            {
                // single quotes / primes
                case 0x2018: case 0x2019: case 0x201B: case 0x2032:
                    replaced++; sb.Append('\''); continue;

                // double quotes
                case 0x201C: case 0x201D: case 0x201F: case 0x2033:
                    replaced++; sb.Append('\"'); continue;

                // dashes/minus
                case 0x2010: case 0x2011: case 0x2012:
                case 0x2013: case 0x2014: case 0x2015:
                case 0x2212:
                    replaced++; sb.Append('-'); continue;

                // ellipsis
                case 0x2026:
                    replaced++; sb.Append("..."); continue;
            }

            sb.Append(rune.ToString());
        }

        return new NormalizationResult(sb.ToString(), replaced, removed);
    }

    static bool IsSpaceLike(int u) =>
        u == 0x00A0 || u == 0x202F || u == 0x205F || u == 0x3000 ||
        (u >= 0x2000 && u <= 0x200A);

    static bool IsFormatControlToRemove(int u) =>
        u == 0x200B || u == 0x200C || u == 0x200D || u == 0x2060 ||
        u == 0x200E || u == 0x200F || u == 0x061C ||
        (u >= 0x202A && u <= 0x202E) || u == 0x202C ||
        (u >= 0x2066 && u <= 0x2069) ||
        (u >= 0x2061 && u <= 0x2064) ||
        u == 0xFEFF;
}
