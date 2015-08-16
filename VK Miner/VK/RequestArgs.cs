using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VK_Miner.VK
{
    public class RequestArgs : IDictionary<string, object>
    {
        private readonly List<KeyValuePair<string, object>> _values = new List<KeyValuePair<string, object>>();

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator() => _values.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        public void Add(KeyValuePair<string, object> item) => _values.Add(item);
        public void Clear() => _values.Clear();
        public bool Contains(KeyValuePair<string, object> item) => _values.Contains(item);
        public void CopyTo(KeyValuePair<string, object>[] array, int arrayIndex) => _values.CopyTo(array, arrayIndex);
        public bool Remove(KeyValuePair<string, object> item) => _values.Remove(item);
        public int Count => _values.Count;
        public bool IsReadOnly => false;
        public void Add(string key, object value) => _values.Add(new KeyValuePair<string, object>(key, value));
        public bool ContainsKey(string key) => _values.Exists(i => i.Key == key);
        public bool Remove(string key) => _values.RemoveAll(i => i.Key == key) > 0;
        public bool TryGetValue(string key, out object value)
        {
            var index = _values.FindIndex(i => i.Key == key);
            if (index >= 0)
            {
                value = _values[index];
                return true;
            }
            else
            {
                value = null;
                return false;
            }
        }

        public object this[string key]
        {
            get { return _values.FirstOrDefault(i => i.Key == key); }
            set
            {
                var index = _values.FindIndex(i => i.Key == key);
                if (index >= 0)
                    _values[index] = new KeyValuePair<string, object>(_values[index].Key, value);
                else
                    _values.Add(new KeyValuePair<string, object>(key, value));
            }
        }

        public ICollection<string> Keys => _values.Select(i => i.Key).ToList();
        public ICollection<object> Values => _values.Select(i => i.Value).ToList();

        public static readonly RequestArgs Empty = new RequestArgs();
    }
}
