using System;

namespace CrossWordTest
{
    public class ReadInput
    {
        readonly CommandStore _commandStore;
        bool _shouldStop;

        public ReadInput(CommandStore aCommandStore)
        {
            _commandStore = aCommandStore;
        }

        public bool ShouldStop
        {
            set { _shouldStop = value; }
        }

        public void Run()
        {
            while (! _shouldStop)
            {
                String command = Console.ReadLine();
                _commandStore.AddCommand(command);
            }
        }
    }
}