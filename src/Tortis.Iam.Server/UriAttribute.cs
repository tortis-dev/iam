// Licensed under GPL-3 (https://www.gnu.org/licenses/gpl-3.0.en.html)

using System.ComponentModel.DataAnnotations;

namespace Tortis.Iam.Server;

public class UriAttribute : DataTypeAttribute
{
    public UriAttribute() : base("uri")
    {
    }

    public override bool IsValid(object? value)
    {
        if (value is null) return true;
        if (!(value is string str)) return false;

        return Uri.TryCreate(str, UriKind.Absolute, out _);
    }
}