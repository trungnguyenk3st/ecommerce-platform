using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace ECommerce.Application.Common;

public static partial class Slugifier
{
    public static string Slugify(string input)
    {
        // Vietnamese đ/Đ does not decompose under NFD like other accented Latin letters.
        var withoutDStroke = input.Replace('đ', 'd').Replace('Đ', 'D');
        var normalized = withoutDStroke.Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder();
        foreach (var c in normalized)
        {
            var category = CharUnicodeInfo.GetUnicodeCategory(c);
            if (category != UnicodeCategory.NonSpacingMark)
                sb.Append(c);
        }

        var slug = sb.ToString().Normalize(NormalizationForm.FormC).ToLowerInvariant();
        slug = NonAlphaNumericRegex().Replace(slug, "-");
        slug = MultiDashRegex().Replace(slug, "-").Trim('-');
        return slug;
    }

    [GeneratedRegex(@"[^a-z0-9]+")]
    private static partial Regex NonAlphaNumericRegex();

    [GeneratedRegex(@"-{2,}")]
    private static partial Regex MultiDashRegex();
}
