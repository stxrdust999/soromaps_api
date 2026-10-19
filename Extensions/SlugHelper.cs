using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace soromaps_api.Extensions;

public static partial class SlugHelper
{
    public static string GenerateSlug(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return string.Empty;

        var normalizedString = input.Trim().ToLowerInvariant().Normalize(NormalizationForm.FormD);
        var stringBuilder = new StringBuilder(normalizedString.Length);

        foreach (var character in normalizedString)
        {
            var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(character);
            if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                stringBuilder.Append(character);
        }

        var withoutDiacritics = stringBuilder.ToString().Normalize(NormalizationForm.FormC);
        var alphanumericAndHyphens = NonSlugCharsRegex().Replace(withoutDiacritics, "-");
        var collapsedHyphens = MultipleHyphensRegex().Replace(alphanumericAndHyphens, "-").Trim('-');

        return collapsedHyphens.Length <= 60 ? collapsedHyphens : collapsedHyphens[..60].TrimEnd('-');
    }

    [GeneratedRegex(@"[^a-z0-9]+")]
    private static partial Regex NonSlugCharsRegex();

    [GeneratedRegex(@"-{2,}")]
    private static partial Regex MultipleHyphensRegex();
}
