using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LudoRules
{
    /// <summary>
    /// Will be of help to the GUI in order to show instructions
    /// </summary>
    public enum Instructions
    {
        Introduce,
        NotIntroduce,
        Move,
        MoveAndKnockout,
        CollisionWithSelf,
        Exit,
        Victory
    }
}
