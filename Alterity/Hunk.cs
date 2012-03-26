using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Alterity
{
    internal interface Hunk
    {
        Hunk[] UndoPrior(Hunk hunk);
    }
}