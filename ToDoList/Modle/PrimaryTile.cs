using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToDoList.Modle
{
    class PrimaryTile
    {
        public string time { get; set; }
        public string message { get; set; }
        public string message2 { get; set; }
        public string branding { get; set; } = "name";
        public string appName { get; set; } = "ToDoList";

        public PrimaryTile(string input, string input2)
        {
            time = input;
            message = input2;
        }
    }
}
