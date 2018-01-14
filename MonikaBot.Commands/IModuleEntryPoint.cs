using System;
namespace MonikaBot.Commands
{
    public interface IModuleEntryPoint
    {
        IModule GetModule();
    }
}
