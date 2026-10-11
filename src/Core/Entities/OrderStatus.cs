using System.Text.Json.Serialization;

namespace Core.Entities;

// serialized as a string in JSON ("Pending") so the API reads nicely
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum OrderStatus
{
    Pending,
    Paid,
    Shipped,
    Cancelled,
}
