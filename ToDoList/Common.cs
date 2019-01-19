using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToDoList
{
    class myItem
    {
        public long id;
        public DateTimeOffset date;
        //public string imgname;
        public string title;
        public string detail;
        public bool finish;
        public myItem(long id,string title, string detail, DateTimeOffset date, bool finish)
        {
            this.id = id;
            this.date = date;
            //this.imgname = imgname;
            this.title = title;
            this.detail = detail;
            this.finish = finish;
        }
    }
}
