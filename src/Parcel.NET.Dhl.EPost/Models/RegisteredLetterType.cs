namespace Parcel.NET.Dhl.EPost.Models;

/// <summary>Registered-mail (Einschreiben) options supported by E-POST.</summary>
public enum RegisteredLetterType
{
    /// <summary>No registered mail (standard letter).</summary>
    None = 0,

    /// <summary>Einwurf Einschreiben (delivery into the mailbox, with proof of delivery).</summary>
    Einwurf,

    /// <summary>Einschreiben (handed over against signature).</summary>
    Standard,

    /// <summary>Einschreiben Rückschein (with returned acknowledgement of receipt).</summary>
    Rueckschein
}

/// <summary>Maps <see cref="RegisteredLetterType"/> to the API's string values.</summary>
public static class RegisteredLetterTypeExtensions
{
    /// <summary>Returns the E-POST API wire value, or <c>null</c> for <see cref="RegisteredLetterType.None"/>.</summary>
    /// <param name="type">The registered-letter type.</param>
    /// <returns>The API string value or <c>null</c>.</returns>
    public static string? ToApiValue(this RegisteredLetterType type) => type switch
    {
        RegisteredLetterType.None => null,
        RegisteredLetterType.Einwurf => "Einwurf Einschreiben",
        RegisteredLetterType.Standard => "Einschreiben",
        RegisteredLetterType.Rueckschein => "Einschreiben Rückschein",
        _ => throw new ArgumentOutOfRangeException(nameof(type), type, "Unknown registered-letter type.")
    };
}
