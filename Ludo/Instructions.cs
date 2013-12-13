using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LudoRules
{
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
