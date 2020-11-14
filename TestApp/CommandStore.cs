using System.Collections.Generic;

namespace CrossWord.TestApp
{
    public class CommandStore
    {
        readonly IList<string> _commandQueue;
        readonly object _lockObject = new object();
        int _count;

        public CommandStore()
        {
            _commandQueue = new List<string>();
            _count = 0;
        }

        public int Count => _count;

        public void AddCommand(string aCommand)
        {
            lock (_lockObject)
            {
                _commandQueue.Add(aCommand);
                _count++;
            }
        }

        public string? PopCommand()
        {
            string? result = null;
            lock (_lockObject)
            {
                if (_commandQueue.Count > 0)
                {
                    result = _commandQueue[0];
                    _commandQueue.RemoveAt(0);
                    _count--;
                }
            }
            return result;
        }

        public object Lock => _lockObject;
    }
}