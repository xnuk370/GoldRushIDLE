using System;
using YG;

public static class GameClock
{
    public static DateTime UtcNow
    {
        get
        {
            return DateTimeOffset
                .FromUnixTimeMilliseconds(YG2.ServerTime())
                .UtcDateTime;
        }
    }

    public static string Today
    {
        get
        {
            return UtcNow.ToString(
                "yyyy-MM-dd",
                System.Globalization.CultureInfo.InvariantCulture
            );
        }
    }
}