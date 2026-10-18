using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace MadrasaSiddeeqiyahAPI.Services
{
    public static class AadhaarValidator
    {
        // Multiplication table (d)
        private static readonly int[,] d = new int[,]
        {
            {0, 1, 2, 3, 4, 5, 6, 7, 8, 9},
            {1, 2, 3, 4, 0, 6, 7, 8, 9, 5},
            {2, 3, 4, 0, 1, 7, 8, 9, 5, 6},
            {3, 4, 0, 1, 2, 8, 9, 5, 6, 7},
            {4, 0, 1, 2, 3, 9, 5, 6, 7, 8},
            {5, 9, 8, 7, 6, 0, 4, 3, 2, 1},
            {6, 5, 9, 8, 7, 1, 0, 4, 3, 2},
            {7, 6, 5, 9, 8, 2, 1, 0, 4, 3},
            {8, 7, 6, 5, 9, 3, 2, 1, 0, 4},
            {9, 8, 7, 6, 5, 4, 3, 2, 1, 0}
        };

        // Permutation table (p)
        private static readonly int[,] p = new int[,]
        {
            {0, 1, 2, 3, 4, 5, 6, 7, 8, 9},
            {1, 5, 7, 6, 2, 8, 3, 0, 9, 4},
            {5, 8, 0, 3, 7, 9, 6, 1, 4, 2},
            {8, 9, 1, 6, 0, 4, 3, 5, 2, 7},
            {9, 4, 5, 3, 1, 2, 6, 8, 7, 0},
            {4, 2, 8, 6, 5, 7, 3, 9, 0, 1},
            {2, 7, 9, 3, 8, 0, 6, 4, 1, 5},
            {7, 0, 4, 6, 9, 1, 3, 2, 5, 8}
        };

        /// <summary>
        /// Validates an Aadhaar number using UIDAI format rules and the Verhoeff algorithm.
        /// </summary>
        public static (bool IsValid, string Message, string Masked) Validate(string? rawAadhaar)
        {
            if (string.IsNullOrWhiteSpace(rawAadhaar))
            {
                return (false, "Aadhaar number is required.", string.Empty);
            }

            // Strip spaces and hyphens
            var cleaned = Regex.Replace(rawAadhaar, @"[\s\-]", "");

            if (!Regex.IsMatch(cleaned, @"^\d{12}$"))
            {
                return (false, "Aadhaar number must be exactly 12 numeric digits.", string.Empty);
            }

            // Aadhaar cannot start with 0 or 1 according to UIDAI specifications
            if (cleaned.StartsWith("0") || cleaned.StartsWith("1"))
            {
                return (false, "Aadhaar number cannot start with 0 or 1 according to UIDAI specifications.", string.Empty);
            }

            // Verify Verhoeff checksum
            int c = 0;
            int[] digits = cleaned.Select(ch => ch - '0').ToArray();
            Array.Reverse(digits);

            for (int i = 0; i < digits.Length; i++)
            {
                c = d[c, p[i % 8, digits[i]]];
            }

            var masked = $"XXXX-XXXX-{cleaned.Substring(8, 4)}";

            if (c != 0)
            {
                // Check if demo bypass is requested or provide checksum error
                return (false, "Invalid Aadhaar number (Verhoeff checksum verification failed).", masked);
            }

            return (true, "Aadhaar number verified successfully with UIDAI standard checksum.", masked);
        }

        public static string Mask(string cleaned)
        {
            if (string.IsNullOrEmpty(cleaned) || cleaned.Length < 4) return cleaned;
            var cleanDigits = Regex.Replace(cleaned, @"[\s\-]", "");
            if (cleanDigits.Length == 12)
            {
                return $"XXXX-XXXX-{cleanDigits.Substring(8, 4)}";
            }
            return cleaned;
        }
    }
}
