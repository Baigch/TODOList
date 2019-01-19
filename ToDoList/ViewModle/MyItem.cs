using ToDoList.Modle;
using ToDoList.Service;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Notifications;
using SQLitePCL;

namespace ToDoList.ViewModle
{
    class MyItem
    {
        static MyItem myItem;
        public MyItem()
        {
            string sql = @"SELECT ID,DATA,TITLE,DETAIL,FINISH FROM MyToDo";
            try
            {
                
                using (var statement = App.conn.Prepare(sql))
                {
                    while (SQLiteResult.ROW == statement.Step())
                    {
                        long id = (long)statement[0];
                        DateTime date = Convert.ToDateTime(statement[1]);
                        string title = (string)statement[2];
                        string detail = (string)statement[3];
                        bool finish = Convert.ToBoolean(statement[4]);
                        this.AddTodoItem(id, title, detail, date, finish);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        public static MyItem getinstance()
        {
            if (myItem == null)
            {
                myItem = new MyItem();
            }
            return myItem;
        }
        private ObservableCollection<Modle.MyList> allItems = new ObservableCollection<Modle.MyList>();
        public ObservableCollection<Modle.MyList> AllItems
        {
            get
            {
                return this.allItems;
            }
        }

        /// private Modle.MyList selectItem = default(Modle.MyList);
        private Modle.MyList selectItem;
        public Modle.MyList SelectItem
        {
            get { return this.selectItem; }
            set { this.selectItem = value; }
        }

        public void AddTodoItem(long id,string title, string detail, DateTime date, bool finish)
        {
            this.allItems.Add(new Modle.MyList(id,title, detail, DateTime.Now, finish));


        }



        public void DeleteItem()
        {
            this.allItems.Remove(selectItem);
            this.selectItem = null;
        }

        public void UpdateItem(string id, string title, string detail, DateTime date)
        {
            this.selectItem.title = title;
            this.selectItem.detail = detail;
            this.selectItem.date = date;
            this.selectItem = null;


        }
    }
}
