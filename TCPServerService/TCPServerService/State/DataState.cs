using System.Collections;
using System.Collections.Generic;

namespace TCPServerService.State
{
    public class DataState : IEnumerable<KeyValuePair<string, List<Dictionary<string, int>>>>
    {
        public readonly Dictionary<string, List<Dictionary<string, int>>> _data;

        // Constructor to initialize with default values
        public DataState()
        {
            _data = new Dictionary<string, List<Dictionary<string, int>>>
            {
                { "SetA", new List<Dictionary<string, int>> { new Dictionary<string, int> { { "One", 1 }, { "Two", 2 } } } },
                { "SetB", new List<Dictionary<string, int>> { new Dictionary<string, int> { { "Three", 3 }, { "Four", 4 } } } },
                { "SetC", new List<Dictionary<string, int>> { new Dictionary<string, int> { { "Five", 5 }, { "Six", 6 } } } },
                { "SetD", new List<Dictionary<string, int>> { new Dictionary<string, int> { { "Seven", 7 }, { "Eight", 8 } } } },
                { "SetE", new List<Dictionary<string, int>> { new Dictionary<string, int> { { "Nine", 9 }, { "Ten", 10 } } } },
            };
        }

        public IEnumerator<KeyValuePair<string, List<Dictionary<string, int>>>> GetEnumerator()
        {
            return _data.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _data.GetEnumerator();
        }

        public List<Dictionary<string, int>> this[string key]
        {
            get => _data[key];
            set => _data[key] = value;
        }
    }
}
