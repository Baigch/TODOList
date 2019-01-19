using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToDoList.Modle
{
    class MyList : INotifyPropertyChanged
    {
        public long id;
        public string _title;
        public string _detial;
        public bool _completed;
        public DateTime _date;

        public MyList(long id, string title, string detail, DateTime date, bool completed)
        {
            this.id = id;
            this.title = title;
            this.detail = detail;
            this.completed = completed;
            this.date = date;
        }

    public event PropertyChangedEventHandler PropertyChanged;

        public string title { get { return _title; } set { _title = value; NotifyPropertyChanged("title"); } }
        public string detail { get { return _detial; } set { _detial = value; NotifyPropertyChanged("detail"); } }
        public bool completed { get { return _completed; } set { _completed = value; NotifyPropertyChanged("completed"); } }
        public DateTime date { get { return _date; } set { _date = value; NotifyPropertyChanged("date"); } }

        public void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }        
    }
}
