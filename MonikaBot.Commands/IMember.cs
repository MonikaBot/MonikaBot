﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonikaBot.Commands
{
    public interface IMember
    {
        string ID { get; set; }
        string Name { get; set; }
        string Mention();
    }
}
