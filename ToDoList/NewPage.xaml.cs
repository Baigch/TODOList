using ToDoList.Modle;
using ToDoList.Service;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Windows.UI.Core;
using Windows.UI.Notifications;
using Windows.UI.Popups;
using Windows.UI.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using SQLitePCL;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace ToDoList
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class NewPage : Page
    {
        public NewPage()
        {
            this.InitializeComponent();
        }
        private SQLiteConnection conn = App.conn;
        private ViewModle.MyItem ViewModles = ViewModle.MyItem.getinstance();

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;

            if (e.NavigationMode == NavigationMode.New)
            {
                if (ViewModles.SelectItem != null)
                {
                    CreateButton.Content = "Update";
                    a.Text = ViewModles.SelectItem.title;
                    ITextRange range = b.Document.GetRange(0, TextConstants.MaxUnitCount);
                    range.Text = ViewModles.SelectItem.detail;
                    date1.Date = ViewModles.SelectItem.date;
                }
                else
                {
                    CreateButton.Content = "Create";
                }
                ApplicationData.Current.LocalSettings.Values.Remove("Item");
                ApplicationData.Current.LocalSettings.Values.Remove("allitems");
                ApplicationData.Current.LocalSettings.Values.Remove("selectitem");
                Debug.WriteLine("00000");
            }
            else
            {
                if (ApplicationData.Current.LocalSettings.Values.ContainsKey("allitems"))
                {
                    ViewModles.AllItems.Clear();
                    List<string> L = JsonConvert.DeserializeObject<List<string>>(
                      (string)ApplicationData.Current.LocalSettings.Values["allitems"]);
                    foreach (var l in L)
                    {
                        myItem a = JsonConvert.DeserializeObject<myItem>(l);
                        MyList item = new MyList(a.id,a.title, a.detail, a.date.Date,a.finish);
                        item.completed = a.finish;
                        ViewModles.AllItems.Add(item);
                    }
                }
                if (ApplicationData.Current.LocalSettings.Values.ContainsKey("selectitem"))
                {
                    ViewModles.SelectItem = ViewModles.AllItems[(int)(ApplicationData.Current.LocalSettings.Values["selectitem"])];
                }
                if (ApplicationData.Current.LocalSettings.Values.ContainsKey("TheWorkInProgress"))
                {
                    var composite = ApplicationData.Current.LocalSettings.Values["TheWorkInProgress"]
                           as ApplicationDataCompositeValue;
                    a.Text = (string)composite["title"];
                    ITextRange range = b.Document.GetRange(0, TextConstants.MaxUnitCount);
                    range.Text = (string)composite["detials"];
                    date1.Date = (DateTimeOffset)composite["date"];
                    ApplicationData.Current.LocalSettings.Values.Remove("TheWorkInProgress");
                }
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            if (((App)App.Current).IsSuspending)
            {
                // Save volatile state in case we get terminated later on
                // then we can restore as if we'd never been gone

                ApplicationDataCompositeValue composite = new ApplicationDataCompositeValue();
                ITextRange range = b.Document.GetRange(0, TextConstants.MaxUnitCount);
                composite["title"] = a.Text;
                composite["detials"] = range.Text;
                composite["date"] = date1.Date;
                ApplicationData.Current.LocalSettings.Values["TheWorkInProgress"] = composite;
                if (ViewModles.SelectItem != null)
                {
                    ApplicationData.Current.LocalSettings.Values["selectitem"] = ViewModles.AllItems.IndexOf(ViewModles.SelectItem);
                }

                List<string> L = new List<string>();
                var allitems = ViewModles.AllItems;
                foreach (var a in allitems)
                {
                    var item = new myItem(a.id,a.title, a.detail, a.date.Date, a.completed);
                    L.Add(JsonConvert.SerializeObject(item));
                }
                ApplicationData.Current.LocalSettings.Values["allitems"] = JsonConvert.SerializeObject(L);
            }
        }

        private void Cancle_Click(object sender, RoutedEventArgs e)
        {
            a.Text = "";
            ITextRange range = b.Document.GetRange(0, TextConstants.MaxUnitCount);
            range.Text = "";
            date1.Date = DateTime.Now;
        }


        private void Creat_Click(object sender, RoutedEventArgs e)
        {
            ITextRange range = b.Document.GetRange(0, TextConstants.MaxUnitCount);
            if (a.Text == "")
            {
                var i = new MessageDialog("Title不能为空！").ShowAsync();
            }
            if (range.Text == "")
            {
                var i = new MessageDialog("Detail不能为空！").ShowAsync();
            }
            if (date1.Date < System.DateTime.Today)
            {
                var i = new MessageDialog("请选择今天之后的日期！").ShowAsync();
            }
            if (a.Text != "" && range.Text != "" && date1.Date >= DateTime.Now.Date)
            {
                ///Debug.Write("101");
                if (ViewModles.SelectItem != null)
                {
                    try
                    {
                        string sql = @"UPDATE MyToDo SET DATA=?,TITLE=?,DETAIL=? WHERE ID=?";
                        using (var cus = conn.Prepare(sql))
                        {
                            cus.Bind(1, date1.Date.ToString());
                            cus.Bind(2, a.Text.Trim());
                            cus.Bind(3, range.Text.Trim());
                            cus.Bind(4, ViewModles.SelectItem.id);
                            cus.Step();
                            ViewModles.UpdateItem("", a.Text, range.Text, date1.Date.DateTime);
                            ///Debug.Write("111");
                            Frame rootFrame = Window.Current.Content as Frame;
                            circulationUpdate();
                            rootFrame.GoBack();
                        }
                    }catch(Exception ex)
                    {
                        Debug.WriteLine(ex.Message);
                    }
                    
                }
                else
                {
                    ///Debug.Write("110");
                    try
                    {
                        string sql = @"INSERT INTO MyToDo(DATA,TITLE,DETAIL,FINISH) VALUES (?,?,?,?)";
                        using (var cus = conn.Prepare(sql))
                        {
                            cus.Bind(1, date1.Date.ToString());
                            cus.Bind(2, a.Text.Trim());
                            cus.Bind(3, range.Text.Trim());
                            cus.Bind(4, "false");
                            cus.Step();
                            ViewModles.AddTodoItem(conn.LastInsertRowId(), a.Text, range.Text, date1.Date.DateTime, false);
                        }
                        Frame rootFrame = Window.Current.Content as Frame;
                        rootFrame.GoBack();
                        circulationUpdate();
                        TitleService.setBadgeCountOnTile(ViewModles.AllItems.Count);
                    }catch(Exception ex)
                    {
                        Debug.WriteLine(ex.Message);
                    }
                }
            }
        }

        private void DeleteBar_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModles.SelectItem != null)
            {
                string sql = @"DELETE FROM MyTodo WHERE ID = ?";
                try
                {
                    using (var res = conn.Prepare(sql))
                    {
                        res.Bind(1, ViewModles.SelectItem.id);
                        res.Step();
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                    throw;
                }
                ViewModles.DeleteItem();
                ViewModles.SelectItem = null;
                ///Frame.Navigate(typeof(MainPage));
                Frame rootFrame = Window.Current.Content as Frame;
                rootFrame.GoBack();
                TitleService.setBadgeCountOnTile(ViewModles.AllItems.Count);
            }
        }

        private async void SelectButton_Click(object sender, RoutedEventArgs e)
        {
            FileOpenPicker openPicker = new FileOpenPicker();
            openPicker.ViewMode = PickerViewMode.Thumbnail;
            openPicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;

            openPicker.FileTypeFilter.Add(".jpg");
            openPicker.FileTypeFilter.Add(".jpeg");
            openPicker.FileTypeFilter.Add(".png");

            StorageFile file = await openPicker.PickSingleFileAsync();
            BitmapImage srcImage = new BitmapImage();

            if (file != null)
            {
                using (IRandomAccessStream stream = await file.OpenAsync(FileAccessMode.Read))
                {
                    await srcImage.SetSourceAsync(stream);
                    picture.Source = srcImage;
                }
            }
        }
        private void UpdatePrimaryTile(string input, string input2)
        {
            var xmlDoc = TitleService.CreateTiles(new PrimaryTile(input, input2));

            var updater = TileUpdateManager.CreateTileUpdaterForApplication();
            TileNotification notification = new TileNotification(xmlDoc);
            updater.Update(notification);
        }

        private void circulationUpdate()
        {
            TileUpdateManager.CreateTileUpdaterForApplication().Clear();
            for (int i = 0; i < ViewModles.AllItems.Count(); i++)
            {
                UpdatePrimaryTile(ViewModles.AllItems[i].title, ViewModles.AllItems[i].detail);
            }
        }
    }
}
