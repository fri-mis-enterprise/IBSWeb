using System.Text.Json.Serialization;

namespace IBS.DTOs
{
    // DTOs for Calendarific API
    public record CalendarificResponse(
        [property: JsonPropertyName("response")] CalendarificData Response
    );

    public record CalendarificData(
        [property: JsonPropertyName("holidays")] List<CalendarificHoliday> Holidays
    );

    public record CalendarificHoliday(
        [property: JsonPropertyName("name")] string Name,
        [property: JsonPropertyName("date")] CalendarificDate Date,
        [property: JsonPropertyName("type")] List<string> Type
    );

    public record CalendarificDate(
        [property: JsonPropertyName("iso")] string Iso  // Format: "2025-01-01"
    );
}
