﻿using System;
using System.Buffers;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Refit.Buffers;

namespace Refit
{
    /// <summary>
    /// A <see langword="class"/> implementing <see cref="IContentSerializer"/> using the System.Text.Json APIs
    /// </summary>
    public sealed class SystemTextJsonContentSerializer : IContentSerializer
    {
        /// <summary>
        /// The JSON serialization options to use
        /// </summary>
        readonly JsonSerializerOptions jsonSerializerOptions;

        /// <summary>
        /// Creates a new <see cref="SystemTextJsonContentSerializer"/> instance
        /// </summary>
        public SystemTextJsonContentSerializer() : this(new JsonSerializerOptions())
        {

            // Set some defaults
            // Default to case insensitive property name matching as that's likely the behavior most users expect
            jsonSerializerOptions.PropertyNameCaseInsensitive = true;
            jsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            jsonSerializerOptions.Converters.Add(new ObjectToInferredTypesConverter());
            jsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
        }

        /// <summary>
        /// Creates a new <see cref="SystemTextJsonContentSerializer"/> instance with the specified parameters
        /// </summary>
        /// <param name="jsonSerializerOptions">The serialization options to use for the current instance</param>
        public SystemTextJsonContentSerializer(JsonSerializerOptions jsonSerializerOptions)
        {
            this.jsonSerializerOptions = jsonSerializerOptions;
        }

        /// <inheritdoc/>
        public Task<HttpContent> SerializeAsync<T>(T item)
        {
            using var utf8BufferWriter = new PooledBufferWriter();

            var utf8JsonWriter = new Utf8JsonWriter(utf8BufferWriter);

            JsonSerializer.Serialize(utf8JsonWriter, item, jsonSerializerOptions);

            var stream = utf8BufferWriter.DetachStream();

            var content = new StreamContent(stream)
            {
                Headers =
                {
                    ContentLength = stream.Length,
                    ContentType = new MediaTypeHeaderValue("application/json") { CharSet = Encoding.UTF8.WebName }
                }
            };

            return Task.FromResult<HttpContent>(content);
        }

        /// <inheritdoc/>
        public async Task<T?> DeserializeAsync<T>(HttpContent content, CancellationToken cancellationToken = default)
        {

            using var stream = await content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
            int streamLength;

            try
            {
                streamLength = (int)stream.Length;
            }
            catch (NotSupportedException)
            {
                /* If the stream doesn't support seeking, the Stream.Length property
                 * cannot be used, which means we can't retrieve the size of a buffer
                 * to rent from the pool. In this case, we just deserialize directly
                 * from the input stream, with the Async equivalent API.
                 * We're using a try/catch here instead of just checking Stream.CanSeek
                 * because some streams can report that property as false even though
                 * they actually let users access the Length property just fine. */
                return await JsonSerializer.DeserializeAsync<T>(stream, jsonSerializerOptions, cancellationToken).ConfigureAwait(false);
            }

            var buffer = ArrayPool<byte>.Shared.Rent(streamLength);

            try
            {
#if NET5_0_OR_GREATER
                var utf8Length = await stream.ReadAsync(buffer, cancellationToken).ConfigureAwait(false);
#else
                var utf8Length = await stream.ReadAsync(buffer, 0, buffer.Length, cancellationToken).ConfigureAwait(false);
#endif

                return Deserialize<T>(buffer, utf8Length, jsonSerializerOptions);
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buffer);
            }
        }

        /// <summary>
        /// Deserializes an item of a specific type from a given UTF8 buffer
        /// </summary>
        /// <typeparam name="T">The type of item to deserialize</typeparam>
        /// <param name="buffer">The input buffer of UTF8 bytes to read</param>
        /// <param name="length">The length of the usable data within <paramref name="buffer"/></param>
        /// <param name="jsonSerializerOptions">The JSON serialization settings to use</param>
        /// <returns>A <typeparamref name="T"/> item deserialized from the UTF8 bytes within <paramref name="buffer"/></returns>
        [Pure]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static T? Deserialize<T>(byte[] buffer, int length, JsonSerializerOptions jsonSerializerOptions)
        {
            var span = new ReadOnlySpan<byte>(buffer, 0, length);
            var utf8JsonReader = new Utf8JsonReader(span);

            return JsonSerializer.Deserialize<T>(ref utf8JsonReader, jsonSerializerOptions);
        }

        public string? GetFieldNameForProperty(PropertyInfo propertyInfo)
        {
            if (propertyInfo is null)
                throw new ArgumentNullException(nameof(propertyInfo));

            return propertyInfo.GetCustomAttributes<JsonPropertyNameAttribute>(true)
                       .Select(a => a.Name)
                       .FirstOrDefault();
        }
    }

    // From https://docs.microsoft.com/en-us/dotnet/standard/serialization/system-text-json-converters-how-to?pivots=dotnet-5-0#deserialize-inferred-types-to-object-properties
    public class ObjectToInferredTypesConverter
       : JsonConverter<object>
    {
        public override object? Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.True)
            {
                return true;
            }

            if (reader.TokenType == JsonTokenType.False)
            {
                return false;
            }

            if (reader.TokenType == JsonTokenType.Number)
            {
                if (reader.TryGetInt64(out var l))
                {
                    return l;
                }

                return reader.GetDouble();
            }

            if (reader.TokenType == JsonTokenType.String)
            {
                if (reader.TryGetDateTime(out var datetime))
                {
                    return datetime;
                }

                return reader.GetString();
            }

            // Use JsonElement as fallback.
            // Newtonsoft uses JArray or JObject.
            using var document = JsonDocument.ParseValue(ref reader);
            return document.RootElement.Clone();
        }

        public override void Write(
            Utf8JsonWriter writer,
            object objectToWrite,
            JsonSerializerOptions options) =>
                throw new InvalidOperationException("Should not get here.");
    }

}

