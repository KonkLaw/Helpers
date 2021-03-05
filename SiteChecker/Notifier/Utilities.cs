using System;

namespace Notifier
{
    static class Utilities
    {
        public static string ToShortString(this TimeSpan timeSpan) => timeSpan.ToString(@"hh\:mm");
    }
}
