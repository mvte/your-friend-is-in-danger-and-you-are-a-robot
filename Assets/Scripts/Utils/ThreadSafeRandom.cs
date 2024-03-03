// https://andrewlock.net/building-a-thread-safe-random-implementation-for-dotnet-framework/
#nullable enable
namespace Utils {
    using System;
    using System.Collections.Concurrent;
    using System.Threading.Tasks;

    // Generates a ship independent of any UnityEngine classes, so it can be used in a parallel context
    public class ThreadSafeRandom {
        [ThreadStatic]
        private static Random? _local;
        private static readonly Random Global = new(); // 👈 Global instance used to generate seeds

        private static Random Instance
        {
            get
            {
                if (_local is null)
                {
                    int seed;
                    lock (Global) // 👈 Ensure no concurrent access to Global
                    {
                        seed = Global.Next();
                    }

                    _local = new Random(seed); // 👈 Create [ThreadStatic] instance with specific seed
                }

                return _local;
            }
        }

        public static int Next() => Instance.Next();

        public static int Next(int maxValue) => Instance.Next(maxValue);

        public static float NextFloat() => (float)Instance.NextDouble();
    }
}