import Foundation

struct NormalizationResult {
    let text: String
    let replaced: Int
    let removed: Int

    var totalAffected: Int { replaced + removed }
}

enum ClipboardNormalizer {
    static func normalizeWithStats(_ input: String) -> NormalizationResult {
        var out = String()
        out.reserveCapacity(input.utf16.count)

        var replaced = 0
        var removed = 0

        for scalar in input.unicodeScalars {
            let u = scalar.value

            // Preserve CR/LF (não faz nada) -> cai no default e mantém

            // Remove: soft hyphen
            if u == 0x00AD { removed += 1; continue }

            // Remove: format controls e ranges listados
            if isFormatControlToRemove(u) { removed += 1; continue }

            // Replace: família NBSP e espaços especiais
            if isSpaceLike(u) {
                replaced += 1
                out.append(" ")
                continue
            }

            switch u {
            // single quotes / primes
            case 0x2018, 0x2019, 0x201B, 0x2032:
                replaced += 1
                out.append("'")
                continue

            // double quotes
            case 0x201C, 0x201D, 0x201F, 0x2033:
                replaced += 1
                out.append("\"")
                continue

            // dashes/minus
            case 0x2010, 0x2011, 0x2012, 0x2013, 0x2014, 0x2015, 0x2212:
                replaced += 1
                out.append("-")
                continue

            // ellipsis
            case 0x2026:
                replaced += 1
                out.append("...")
                continue

            default:
                out.unicodeScalars.append(scalar)
            }
        }

        return NormalizationResult(text: out, replaced: replaced, removed: removed)
    }

    private static func isSpaceLike(_ u: UInt32) -> Bool {
        return u == 0x00A0 || u == 0x202F || u == 0x205F || u == 0x3000 ||
               (u >= 0x2000 && u <= 0x200A)
    }

    private static func isFormatControlToRemove(_ u: UInt32) -> Bool {
        return u == 0x200B || u == 0x200C || u == 0x200D || u == 0x2060 ||
               u == 0x200E || u == 0x200F || u == 0x061C ||
               (u >= 0x202A && u <= 0x202E) || u == 0x202C ||
               (u >= 0x2066 && u <= 0x2069) ||
               (u >= 0x2061 && u <= 0x2064) ||
               u == 0xFEFF
    }
}
