using System;

namespace UZSG.Timebase
{
    public readonly struct Second
    {
        public static Second Minute { get => new(60); }
        public static Second Hour { get => new(3600); }
        private readonly float value;

        public Second(float value)
        {
            if (value < 0) throw new ArgumentOutOfRangeException("Seconds cannot be negative.");
            this.value = value;
        }

        public static Second operator +(Second a) => a;
        public static Second operator -(Second a) => -a;

        public static Second operator +(Second a, Second b)
            => a + b;

        public static Second operator -(Second a, Second b)
            => a + (-b);

        public static Second operator *(Second a, Second b)
            => a * b;
        }
}
