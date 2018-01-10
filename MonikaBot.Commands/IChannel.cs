using System;

namespace MonikaBot.Commands
{
    /// <summary>
    /// Defines a basic Channel interface
    /// </summary>
    public interface IChannel
    {
        string Name { get; set; }
        string ID { get; set; }
    }
}

