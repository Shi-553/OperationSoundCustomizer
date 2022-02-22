using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace OperationSoundCustomizer
{

    public interface IValue<T>
    {
        public T GetNextValue();
    }
    public class CommonValue<T> : IValue<T>
    {
        public T Value { get; set; }
        public T GetNextValue()
        {
            return Value;
        }
    }

    public interface IValueList<T> : IValue<T>
    {
        public List<T> List { get; }
    }

    public class SequenceValues<T> : IValueList<T>
    {
        public List<T> List { get; init; }

        public SequenceValues(List<T> ts)
        {
            List = ts;
        }
        int index = -1;
        void Update()
        {
            index = (index + 1) % List.Count;
        }

        public T GetNextValue()
        {
            Update();
            return List[index];
        }
    }

    //途中の値のこともある
    public class RandomBetweenValues<T> : IValueList<T>
    {
        public List<T> List { get; init; }
        public RandomBetweenValues(List<T> ts)
        {
            List = ts;
        }
        public T GetNextValue()
        {
            return RandomHelper.GetRandomBetween(List.ToArray());
        }
    }
    //要素のどれかになる
    public class RandomValues<T> : IValueList<T>
    {
        public List<T> List { get; init; }
        public RandomValues(List<T> ts)
        {
            List = ts;
        }

        public T GetNextValue()
        {
            return RandomHelper.GetRandom(List.ToArray());
        }

    }


    public class LockValues<T>
    {
        public IValueList<T> Values { get; init; }

        [JsonIgnore]
        readonly Dictionary<int, T> lockValues = new();

        public LockValues(IValueList<T> list) { Values = list; }

        public T LockValue(int id, bool isOverride = false)
        {
            if (!isOverride && lockValues.TryGetValue(id, out var val))
            {
                return val;
            }
            var value = Values.GetNextValue();
            lockValues[id] = value;
            return value;
        }

        public bool UnlockValue(int id, out T val)
        {
            return lockValues.Remove(id, out val);
        }
    }
}
