﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Doredis
{
    interface ILockableDataObject : ILockable, IPathObject
    {
    }
}
