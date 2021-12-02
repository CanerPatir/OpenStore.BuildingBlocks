using System.Text.Json;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore.Metadata.Builders;

public static class ValueConversionExtensions
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        
    };

    public static PropertyBuilder<T> HasJsonConversion<T>(this PropertyBuilder<T> propertyBuilder)
    {
        var converter = new ValueConverter<T, string>(
            v => JsonSerializer.Serialize(v, SerializerOptions),
            v => JsonSerializer.Deserialize<T>(v, SerializerOptions)
        );

        var comparer = new ValueComparer<T>(
            (l, r) => JsonSerializer.Serialize(l, SerializerOptions) == JsonSerializer.Serialize(r, SerializerOptions),
            v => v == null ? 0 : JsonSerializer.Serialize(v, SerializerOptions).GetHashCode(),
            v => JsonSerializer.Deserialize<T>(JsonSerializer.Serialize(v, SerializerOptions), SerializerOptions)
        );

        propertyBuilder.HasConversion(converter);
        propertyBuilder.Metadata.SetValueConverter(converter);
        propertyBuilder.Metadata.SetValueComparer(comparer);

        return propertyBuilder;
    }
}