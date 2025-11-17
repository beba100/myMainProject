using System;

namespace zaaerIntegration.Utilities
{
    /// <summary>
    /// Provides current date/time values aligned with the Saudi Arabia timezone.
    /// </summary>
    public static class KsaTime
    {
        private static readonly TimeZoneInfo SaudiTimeZone = ResolveSaudiTimeZone();

        /// <summary>
        /// Current date/time in Saudi Arabia (Arab Standard Time / Asia/Riyadh).
        /// </summary>
        public static DateTime Now => TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, SaudiTimeZone);

        /// <summary>
        /// Exposes UTC now in case it is still needed by the application.
        /// </summary>
        public static DateTime UtcNow => DateTime.UtcNow;

        /// <summary>
        /// Converts an arbitrary UTC timestamp into the Saudi timezone.
        /// </summary>
        public static DateTime ConvertFromUtc(DateTime utcDateTime) => TimeZoneInfo.ConvertTimeFromUtc(utcDateTime, SaudiTimeZone);

        private static TimeZoneInfo ResolveSaudiTimeZone()
        {
            var candidateIds = new[] { "Arab Standard Time", "Asia/Riyadh" };

            foreach (var id in candidateIds)
            {
                try
                {
                    return TimeZoneInfo.FindSystemTimeZoneById(id);
                }
                catch (TimeZoneNotFoundException)
                {
                }
                catch (InvalidTimeZoneException)
                {
                }
            }

            return TimeZoneInfo.Utc;
        }
    }
}
