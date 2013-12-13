using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ludo
{
    class Dice
    {  
       
        public int rollDice()
        {
            Random num = new Random();
            int Number = num.Next(1, 7);

            return Number;
        }
    }
}
