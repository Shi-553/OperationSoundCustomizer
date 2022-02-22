using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OperationSoundCustomizer
{
    public class RandomHelper
    {
        public static Random Random { get; } = new Random();


        internal static int GetRandomIndex(int max)
        {
            return Random.Next(max);
        }

        internal static T GetRandom<T>(params T[] list) => GetRandom((IReadOnlyList<T>)list);

        internal static T GetRandom<T>(IReadOnlyList<T> list)
        {
            return list[GetRandomIndex(list.Count)];
        }

        internal static double GetRandomBetweenIndex(int max)
        {
            return Random.NextDouble() * max;
        }

        internal static T GetRandomBetween<T>(params T[] list) => GetRandomBetween((IReadOnlyList<T>)list);

        internal static T GetRandomBetween<T>(IReadOnlyList<T> list)
        {
            var rand = Random.NextDouble();
            var randLen = rand * list.Count;
            var floor = Convert.ToInt32(Math.Floor(randLen));
            var ceiling = Convert.ToInt32(Math.Ceiling(randLen));

            return ((dynamic)list[floor] * rand) + ((dynamic)list[ceiling] * (1 - rand));
        }
    }
}
