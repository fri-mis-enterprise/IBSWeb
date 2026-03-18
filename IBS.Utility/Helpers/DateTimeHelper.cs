using System.Text.Json;
using IBS.DTOs;

namespace IBS.Utility.Helpers
{
    public static class DateTimeHelper
    {
        private static readonly TimeZoneInfo PhilippineTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Asia/Manila");

        private static readonly Random Randomizer = new Random();
        private static DateTime? _lastGeneratedTime;


        private static readonly HttpClient _httpClient = new();

        public static DateTime GetCurrentPhilippineTime()
        {
            return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, PhilippineTimeZone);
        }

        public static DateTime GetRandomPhilippineWorkTime()
        {
            var now = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, PhilippineTimeZone);

            var workStart = now.Date.AddHours(8).AddMinutes(30);
            var workEnd = now.Date.AddHours(17).AddMinutes(30);

            if (!_lastGeneratedTime.HasValue || _lastGeneratedTime.Value < workStart)
            {
                _lastGeneratedTime = workStart;
            }

            int minutesToAdd = Randomizer.Next(1, 6);

            int secondsToAdd = Randomizer.Next(0, 60);

            var nextTime = _lastGeneratedTime.Value
                .AddMinutes(minutesToAdd)
                .AddSeconds(secondsToAdd);

            if (nextTime > workEnd)
            {
                nextTime = workEnd;
            }

            _lastGeneratedTime = nextTime;

            return nextTime;
        }

        public static string GetCurrentPhilippineTimeFormatted(DateTime dateTime = default, string format = "MM/dd/yyyy hh:mm tt")
        {
            var philippineTime = dateTime != default ? dateTime : GetCurrentPhilippineTime();
            return philippineTime.ToString(format);
        }

        public static async Task<List<DateOnly>> GetNonWorkingDays(DateOnly startDate, DateOnly endDate, string countryCode)
        {
            var nonWorkingDays = new List<DateOnly>();

            var jsonSerializerOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            using var httpClient = new HttpClient();

            // Get holidays for all years in the range
            for (int year = startDate.Year; year <= endDate.Year; year++)
            {
                using var response = await httpClient.GetAsync($"https://date.nager.at/api/v3/publicholidays/{year}/{countryCode}");

                if (response.IsSuccessStatusCode)
                {
                    await using var jsonStream = await response.Content.ReadAsStreamAsync();
                    var items = JsonSerializer.Deserialize<List<PublicHolidayDto>>(jsonStream, jsonSerializerOptions);

                    if (items is not null)
                    {
                        nonWorkingDays.AddRange(
                            items.Select(h => DateOnly.FromDateTime(h.Date))
                        );
                    }
                }
            }

            // Filter holidays within range
            nonWorkingDays = nonWorkingDays.Where(d => d >= startDate && d <= endDate).ToList();

            // Add weekends that are not already holidays
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
