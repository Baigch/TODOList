using ToDoList.Modle;
using System;
using System.Collections.Generic;
using System.Linq;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Shapes;
using Newtonsoft.Json;
using System.Diagnostics;
using Windows.UI.Text;
using ToDoList.Service;
using Windows.UI.Notifications;
using Windows.ApplicationModel.DataTransfer;
using Windows.ApplicationModel;
using SQLitePCL;

// https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x804 上介绍了“空白页”项模板

namespace ToDoList
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private SQLiteConnection conn = App.conn;
        public MainPage()
        {
            this.InitializeComponent();
            this.ViewModles = ViewModle.MyItem.getinstance();
            NavigationCacheMode = NavigationCacheMode.Enabled;

            
        }
        ViewModle.MyItem ViewModles { get; set; }
        Modle.MyList ShareItem;

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            
            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Collapsed;
            if (e.NavigationMode == NavigationMode.New)
            {
                //ViewModles.AllItems.Clear();
                //readDatebase();
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
                    title.Text = (string)composite["title"];
                    ITextRange range = detials.Document.GetRange(0, TextConstants.MaxUnitCount);
                    range.Text = (string)composite["detials"];
                    date.Date = (DateTimeOffset)composite["date"];
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
                ITextRange range = detials.Document.GetRange(0, TextConstants.MaxUnitCount);
                composite["title"] = title.Text;
                composite["detials"] = range.Text;
                composite["date"] = date.Date;
                ApplicationData.Current.LocalSettings.Values["TheWorkInProgress"] = composite;
                if (ViewModles.SelectItem != null)
                {
                    ApplicationData.Current.LocalSettings.Values["selectitem"] = ViewModles.AllItems.IndexOf(ViewModles.SelectItem);
                }
                List<string> L = new List<string>();
                var allitems = ViewModles.AllItems;
                foreach (var a in allitems)
                {
                    var item = new myItem(a.id,a.title, a.detail, a.date, a.completed);
                    L.Add(JsonConvert.SerializeObject(item));
                }
                ApplicationData.Current.LocalSettings.Values["allitems"] = JsonConvert.SerializeObject(L);
            }
        }

        async void OnShareDataRequested(DataTransferManager sender, DataRequestedEventArgs args)
        {
            var dp = args.Request.Data;
            var deferral = args.Request.GetDeferral();

            var photoFile = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///Assets/2.jpg"));
            dp.Properties.Title = ShareItem.title;
            dp.Properties.Description = ShareItem.detail;
            dp.SetText("done" + ShareItem.detail);
            dp.SetStorageItems(new List<StorageFile> { photoFile });
            deferral.Complete();
        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            ITextRange range = detials.Document.GetRange(0, TextConstants.MaxUnitCount);
            var parent = VisualTreeHelper.GetParent(sender as DependencyObject);
            Line line = VisualTreeHelper.GetChild(parent, 2) as Line;
            line.Opacity = 0;
            var data = (sender as FrameworkElement).DataContext;
            var item = MyListView.ContainerFromItem(data) as ListViewItem;
            string sql = @"UPDATE MyToDo SET FINISH=? WHERE ID=?";
            using (var cus = conn.Prepare(sql))
            {
                cus.Bind(1, "false");
                cus.Bind(2, (item.Content as MyList).id);
                cus.Step();              
            }
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            var parent = VisualTreeHelper.GetParent(sender as DependencyObject);
            Line line = VisualTreeHelper.GetChild(parent, 2) as Line;
            line.Opacity = 1;
            var data = (sender as FrameworkElement).DataContext;
            var item = MyListView.ContainerFromItem(data) as ListViewItem;
            string sql = @"UPDATE MyToDo SET FINISH=? WHERE ID=?";
            using (var cus = conn.Prepare(sql))
            {
                cus.Bind(1, "true");
                cus.Bind(2, (item.Content as MyList).id);
                cus.Step();
            }
        }

        private void MyListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            ITextRange range = detials.Document.GetRange(0, TextConstants.MaxUnitCount);
            ViewModles.SelectItem = (Modle.MyList)(e.ClickedItem);
            if (ViewModles.SelectItem != null)
            {
                createButton.Content = "Update";
            }
            if (MyBigList.Visibility != Visibility.Collapsed)
            {
                title.Text = ViewModles.SelectItem.title;
                range.Text = ViewModles.SelectItem.detail;
                date.Date = ViewModles.SelectItem.date;
            }
            else
            {
                Frame.Navigate(typeof(NewPage));
            }
        }

        private void AddAppBarButton_Click(object sender, RoutedEventArgs e)
        {
            if (Window.Current.Bounds.Width < 800)
            {
                ViewModles.SelectItem = null;
                Frame.Navigate(typeof(NewPage));
            }
            else
            {
                if (ViewModles.SelectItem != null)
                {
                    ViewModles.DeleteItem();
                    ViewModles.SelectItem = null;
                    Frame.Navigate(typeof(MainPage));
                }
            }
        }

        private void createButton_Click(object sender, RoutedEventArgs e)
        {
            ITextRange range = detials.Document.GetRange(0, TextConstants.MaxUnitCount);
            if (title.Text == "")
            {
                var i = new MessageDialog("Title不能为空！").ShowAsync();
            }
            if (range.Text == "")
            {
                var i = new MessageDialog("Detial不能为空！").ShowAsync();
            }
            if (date.Date < System.DateTime.Today)
            {
                var i = new MessageDialog("所选日期不能为今天以前！").ShowAsync();
            }
            if (title.Text != "" && range.Text != "" && date.Date >= DateTime.Now.Date)
            {
                if (ViewModles.SelectItem != null)
                {
                    try
                    {
                        string sql = @"UPDATE MyToDo SET DATA=?,TITLE=?,DETAIL=? WHERE ID=?";
                        using (var cus = conn.Prepare(sql))
                        {
                            cus.Bind(1, date.Date.ToString());
                            cus.Bind(2, title.Text.Trim());
                            cus.Bind(3, range.Text.Trim());
                            cus.Bind(4, ViewModles.SelectItem.id);
                            cus.Step();
                            ViewModles.UpdateItem("", title.Text.Trim(), range.Text.Trim(), date.Date.DateTime);
                        }
                        Frame.Navigate(typeof(MainPage));
                        circulationUpdate();
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.Message);
                    }
                }
                if (createButton.Content.ToString() == "Create" && ViewModles.SelectItem == null)
                {
                    try
                    {
                        string sql = @"INSERT INTO MyToDo(DATA,TITLE,DETAIL,FINISH) VALUES (?,?,?,?)";
                        using (var cus = conn.Prepare(sql))
                        {
                            cus.Bind(1, date.Date.ToString());
                            cus.Bind(2, title.Text.Trim());
                            cus.Bind(3, range.Text.Trim());
                            cus.Bind(4, "false");
                            cus.Step();
                            ViewModles.AddTodoItem(conn.LastInsertRowId(), title.Text, range.Text, date.Date.DateTime, false);
                        }
                        title.Text = "";
                        range.Text = "";
                        date.Date = System.DateTime.Today;
                        circulationUpdate();
                        TitleService.setBadgeCountOnTile(ViewModles.AllItems.Count);
                    }catch(Exception ex)
                    {
                        Debug.WriteLine(ex.Message);
                    }
                }
                if (createButton.Content.ToString() == "Update" && ViewModles.SelectItem != null)
                {

                    try
                    {
                        string sql = @"UPDATE MyToDo SET DATA=?,TITLE=?,DETAIL=? WHERE ID=?";
                        using (var cus = conn.Prepare(sql))
                        {
                            cus.Bind(1, date.Date.ToString());
                            cus.Bind(2, title.Text.Trim());
                            cus.Bind(3, range.Text.Trim());
                            cus.Bind(4, ViewModles.SelectItem.id);
                            cus.Step();
                            ViewModles.UpdateItem("", title.Text.Trim(), range.Text.Trim(), date.Date.DateTime);
                        }
                        Frame.Navigate(typeof(MainPage));
                        circulationUpdate();
                    }catch (Exception ex)
                    {
                        Debug.WriteLine(ex.Message);
                    }
                    
                }
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            ITextRange range = detials.Document.GetRange(0, TextConstants.MaxUnitCount);
            title.Text = "";
            range.Text = "";
            date.Date = System.DateTime.Now;
        }

        private void Share_Click(object sender, RoutedEventArgs e)
        {
            ShareItem = (Modle.MyList)((MenuFlyoutItem)sender).DataContext;

            DataTransferManager dataTransferManager = DataTransferManager.GetForCurrentView();
            dataTransferManager.DataRequested += DataTransferManager_DataRequested;
            DataTransferManager.ShowShareUI();
        }

        async void DataTransferManager_DataRequested(DataTransferManager sender, DataRequestedEventArgs args)
        {
            ///throw new NotImplementedException();
            DataRequest request = args.Request;

            request.Data.Properties.Title = ShareItem.title;
            request.Data.Properties.Description = ShareItem.detail;
            request.Data.SetText(ShareItem.detail);
            request.Data.SetText(ShareItem.detail);

            var Deferral = args.Request.GetDeferral();
            var SharePhoto = await Package.Current.InstalledLocation.GetFileAsync("Assets\\2.jpg");
            request.Data.Properties.Thumbnail = RandomAccessStreamReference.CreateFromFile(SharePhoto);
            request.Data.SetBitmap(RandomAccessStreamReference.CreateFromFile(SharePhoto));
            Deferral.Complete();
        }

        private void Edit_Click(object sender, RoutedEventArgs e)
        {
            var data = (sender as FrameworkElement).DataContext;
            var item = MyListView.ContainerFromItem(data) as ListViewItem;
            ViewModles.SelectItem = item.Content as MyList;
            if (MyBigList.Visibility.ToString() == "Collapsed")
            {
                Frame.Navigate(typeof(NewPage));
            }
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            /*createButton.Content = "Create";
            CancelButton_Click(null, null);
            string sql = @"DELETE FROM MyToDo WHERE ID=?";
            try
            {               
                using (var cus = conn.Prepare(sql))
                {
                    cus.Bind(1, ViewModles.SelectItem.id);
                    cus.Step();
                    //var data = (sender as FrameworkElement).DataContext;
                    //var item = MyListView.ContainerFromItem(data) as ListViewItem;
                    //ViewModles.SelectItem = item.Content as MyList;
                    
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
            ViewModles.DeleteItem();
            ViewModles.SelectItem = null;
            TitleService.setBadgeCountOnTile(ViewModles.AllItems.Count);*/
            dynamic ori = e.OriginalSource;
            ViewModles.SelectItem = (Modle.MyList)ori.DataContext;
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
            //    Frame.Navigate(typeof(MainPage), ViewModles);
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


        /*
        private void UpdateBadge(object sender, RoutedEventArgs e)
        {
            _count++;
            TitleService.setBadgeCountOnTile(_count);
        }
        */

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

        private async void search_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            var text = args.QueryText.Trim();
            if (text == "")
                return;
            string alert = "";
            var sql = "SELECT DATA,TITLE,DETAIL FROM MyToDo WHERE DATA LIKE ? OR TITLE LIKE ? OR DETAIL LIKE ?";
            using (var statement = conn.Prepare(sql))
            {
                statement.Bind(1, "%%" + text + "%%");
                statement.Bind(2, "%%" + text + "%%");
                statement.Bind(3, "%%" + text + "%%");
                while (SQLiteResult.ROW == statement.Step())
                {
                    var date = statement[0].ToString();
                    date = date.Substring(0, date.IndexOf(' '));
                    string title = statement[1] as string;
                    string detail = statement[2] as string;
                    alert +="Title: "+title+";\nDetail: "+detail + ";\nDue Date: " + statement[0].ToString() + "\n";
                }
                if (alert == "")
                    alert = "No result!\n";
                await new MessageDialog(alert).ShowAsync();
            }
        }
    }
}
