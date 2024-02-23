using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalProduct
{
    public class ConsoleSpinner
    {
        int counter;
        public ConsoleSpinner()
        {
            counter = 0;
        }
        public void Turn()
        {
            counter++;
            switch (counter % 2)
            {
                case 0: Console.Write("|"); break;
                case 1: Console.Write("_"); break;
            }
            Console.SetCursorPosition(Console.CursorLeft - 1, Console.CursorTop);
            System.Threading.Thread.Sleep(500);
        }
    }
}
