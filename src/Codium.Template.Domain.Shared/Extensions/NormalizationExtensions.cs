using System.Globalization;
using System.Text;

namespace Codium.Template.Domain.Shared.Extensions;

public static class NormalizationExtensions
{
    public static string NormalizeValue(this string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return value;
        }

        value = value.Normalize(NormalizationForm.FormD);
        var chars = value.Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark).ToArray();
        value = new string(chars).Normalize(NormalizationForm.FormC);
        
        return value.ToUpperInvariant();
    }
}