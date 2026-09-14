namespace IAT.Core.ResultData;

/// <summary>
/// One decrypted administration ready for the workbook.
/// Build this after RSA/AES unwrap — the exporter never sees ciphertext.
/// </summary>
public sealed class AdministrationExport
{
    public int ResultId { get; init; }

    public DateTimeOffset? AdminTime { get; init; }

    /// <summary>Optional subject token. Omitted from the sheet when every row is blank.</summary>
    public string? Token { get; init; }

    public required ResultSet Results { get; init; }
}
