using System;

namespace Noppes.Fluffle.Bot.Utils
{
    public static class FluffleIdDateTime
    {
        private const string Alphabet = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz23456789";
        private const int YearOffset = 2020;
        private const int MinuteMultiplier = 2;

        public static string ToString(DateTime value)
        {
            var year = Alphabet[value.Year - YearOffset];
            var month = Alphabet[value.Month];
            var day = Alphabet[value.Day];
            var hour = Alphabet[value.Hour];
            var minute = Alphabet[value.Minute / MinuteMultiplier];

            return $"{year}{month}{day}{hour}{minute}";
        }
    }
}
