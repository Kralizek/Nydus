﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Nybus;

namespace Tests
{
    public class FirstTestCommand : ICommand
    {
        public string Message { get; set; }
    }

    public class FirstTestCommandHandler : ICommandHandler<FirstTestCommand>
    {
        public virtual Task HandleAsync(IDispatcher dispatcher, ICommandContext<FirstTestCommand> incomingCommand)
        {
            throw new NotImplementedException();
        }
    }

    public class SecondTestCommand : ICommand { }

    public class SecondTestCommandHandler : ICommandHandler<SecondTestCommand>
    {
        private readonly CommandReceived<SecondTestCommand> _commandReceived;

        public SecondTestCommandHandler(CommandReceived<SecondTestCommand> commandReceived)
        {
            _commandReceived = commandReceived ?? throw new ArgumentNullException(nameof(commandReceived));
        }

        public virtual Task HandleAsync(IDispatcher dispatcher, ICommandContext<SecondTestCommand> incomingCommand)
        {
            return _commandReceived(dispatcher, incomingCommand);
        }
    }

    
    public class ThirdTestCommand : ICommand
    {
        public string Message { get; set; }
    }
}

namespace Samples
{
    [Message("ThirdTestCommand", "Tests")]
    public class AttributeTestCommand : ICommand
    {
        public string Message { get; set; }
    }
}
