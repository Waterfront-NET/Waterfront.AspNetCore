using System.Text.Json;
using System.Text.Json.Serialization;
using Waterfront.Common.Contracts.Tokens.Response;

namespace Waterfront.AspNetCore.Json.Converters;

public class TokenResponseJsonConverter : JsonConverter<TokenResponse>
{
    public static readonly TokenResponseJsonConverter Instance = new TokenResponseJsonConverter();

    public override TokenResponse Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options
    ) => throw new NotImplementedException();

    public override void Write(
        Utf8JsonWriter writer,
        TokenResponse value,
        JsonSerializerOptions options
    )
    {
        writer.WriteStartObject();
        writer.WritePropertyName("token");
        writer.WriteStringValue(value.Token);
        writer.WritePropertyName("access_token");
        writer.WriteStringValue(value.Token);

        if ( value.IssuedAt != null )
        {
            writer.WritePropertyName("issued_at");
            writer.WriteStringValue(value.IssuedAt);
        }

        if ( value.ExpiresIn.HasValue )
        {
            writer.WritePropertyName("expires_in");
            writer.WriteNumberValue(value.ExpiresIn.Value);
        }

        if ( value.RefreshToken != null )
        {
            writer.WritePropertyName("refresh_token");
            writer.WriteStringValue(value.RefreshToken);
        }

        writer.WriteEndObject();
    }
}
