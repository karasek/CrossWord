using System;

namespace CrossWord.TestApp;

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
        set => _shouldStop = value;
    }

    public void Run()
    {
        while (!_shouldStop)
        {
            var command = Console.ReadLine();
            if (command != null)
                _commandStore.AddCommand(command);
        }
    }
}
