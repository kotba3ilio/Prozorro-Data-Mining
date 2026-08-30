using System.Globalization;
using System.Text;

namespace ProzorroDataMining.Application.Tenders;

public sealed record TenderPageCursor(DateTimeOffset DateCreated, Guid Id)
{
    public string Encode()
    {
        var payload = string.Create(
            CultureInfo.InvariantCulture,
            $"{DateCreated.ToUniversalTime():O}|{Id:N}");
        var base64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(payload));

        return base64.TrimEnd('=').Replace('+', '-').Replace('/', '_');
    }

    public static bool TryDecode(string? cursor, out TenderPageCursor? value)
    {
        value = null;

        if (string.IsNullOrWhiteSpace(cursor))
        {
            return true;
        }

        try
        {
            var normalized = cursor.Replace('-', '+').Replace('_', '/');
            var padding = normalized.Length % 4;

            if (padding > 0)
            {
                normalized = normalized.PadRight(normalized.Length + 4 - padding, '=');
            }

            var payload = Encoding.UTF8.GetString(Convert.FromBase64String(normalized));
            var parts = payload.Split('|', 2);

            if (parts.Length != 2 ||
                !DateTimeOffset.TryParse(parts[0], CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var dateCreated) ||
                !Guid.TryParseExact(parts[1], "N", out var id))
            {
                return false;
            }

            value = new TenderPageCursor(dateCreated.ToUniversalTime(), id);
            return true;
        }
        catch (FormatException)
        {
            return false;
        }
    }
}