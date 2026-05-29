using System.Text.Json;
using IBS.DTOs;
using Microsoft.Extensions.Configuration;

namespace IBS.Utility.Helpers
{
    public static class DateTimeHelper
    {
        private static readonly TimeZoneInfo _philippineTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Asia/Manila");

        private static readonly object _lock = new();
        private static DateTime? _lastGeneratedTime;
        private static readonly HttpClient _httpClient = new();
        private static string _calendarificApiKey = string.Empty;
        private static readonly Dictionary<(string country, int year), List<DateOnly>> _holidayCache = new();

        public static void Initialize(IConfiguration configuration)
        {
            _calendarificApiKey = configuration["Calendarific:ApiKey"]
                                  ?? throw new InvalidOperationException("Calendarific API key is not configured.");
        }

        public static DateTime GetCurrentPhilippineTime()
        {
            return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, _philippineTimeZone);
        }

        public static DateOnly GetFirstDayOfCurrentPhilippineMonth()
        {
            var currentDate = GetCurrentPhilippineTime();
            return new DateOnly(currentDate.Year, currentDate.Month, 1);
        }

        public static DateTime GenerateRandomTransactionDateTime(DateOnly date)
        {
            lock (_lock)
            {
                var baseDate = date.ToDateTime(TimeOnly.MinValue);

                var workStart = baseDate.AddHours(8).AddMinutes(30);
                var workEnd = baseDate.AddHours(17).AddMinutes(30);

                var random = Random.Shared;

                if (_lastGeneratedTime == null || _lastGeneratedTime.Value.Date != baseDate.Date)
                {
                    var initial = workStart
                        .AddMinutes(random.Next(2, 6))
                        .AddSeconds(random.Next(0, 60));

                    _lastGeneratedTime = initial;
                    return initial;
                }

                var next = _lastGeneratedTime.Value
                    .AddMinutes(random.Next(2, 6))
                    .AddSeconds(random.Next(0, 60));

                if (next > workEnd)
                    next = workEnd;

                _lastGeneratedTime = next;
                return next;
            }
        }

        public static string GetCurrentPhilippineTimeFormatted(DateTime dateTime = default, string format = "MM/dd/yyyy hh:mm tt")
        {
            var philippineTime = dateTime != default ? dateTime : GetCurrentPhilippineTime();
            return philippineTime.ToString(format);
        }

        public static async Task<List<DateOnly>> GetNonWorkingDays(DateOnly startDate, DateOnly endDate, string countryCode = "PH")
        {
            var nonWorkingDays = new List<DateOnly>();

            var jsonSerializerOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

            for (int year = startDate.Year; year <= endDate.Year; year++)
            {
                if (_holidayCache.TryGetValue((countryCode, year), out var cached))
                {
                    nonWorkingDays.AddRange(cached);
                    continue;
                }

                var url = $"https://calendarific.com/api/v2/holidays?api_key={_calendarificApiKey}&country={countryCode}&year={year}";

                using var response = await _httpClient.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    continue;
                }

                await using var jsonStream = await response.Content.ReadAsStreamAsync();
                var result = JsonSerializer.Deserialize<CalendarificResponse>(jsonStream, jsonSerializerOptions);

                if (result?.Response.Holidays is not null)
                {
                    var holidays = result.Response.Holidays
                        .Where(h =>
                            h.Type.Contains("National holiday") ||
                            h.Type.Contains("Special holiday"))
                        .Select(h => DateOnly.Parse(h.Date.Iso))
                        .ToList();

                    _holidayCache[(countryCode, year)] = holidays;

                    nonWorkingDays.AddRange(holidays);
                }
            }

            // Filter holidays within range and remove duplicates
            nonWorkingDays = nonWorkingDays
                .Where(d => d >= startDate && d <= endDate)
                .Distinct()
                .ToList();

            // Add weekends
            for (var date = startDate; date <= endDate; date = date.AddDays(1))
            {
                if ((date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday)
                    && !nonWorkingDays.Contains(date))
                {
                    nonWorkingDays.Add(date);
                }
            }

            nonWorkingDays.Sort();

            return nonWorkingDays;
        }
    }
}
