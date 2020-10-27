using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using image_recognizer;
using System.Threading;
using System.Collections.Specialized;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace WpfApp1
{
    public partial class MainWindow : Window
    {
        private string folderName = "";
        Classificator cl;
        ObservableCollection<string> lst;
        List<int> lst_for_class;
        ObservableCollection<string> for_listbox2;
        SortedDictionary<string, int> map;
        SortedDictionary<string, List<string>> map_to_image;
        ObservableCollection<Img> imgs;
        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = this;
            lst = new ObservableCollection<string>();
            Label1.ItemsSource = lst;
            Rec.IsEnabled = false;
            Stop.IsEnabled = false;
            map = new SortedDictionary<string, int>();
            map_to_image = new SortedDictionary<string, List<string>>();
            lst_for_class = new List<int>();
            for_listbox2 = new ObservableCollection<string>();
            imgs = new ObservableCollection<Img>();
            int i = 0;
            foreach(var elem in Classificator.classLabels)
            {
                map.Add(elem, i);
                i++;
                lst_for_class.Add(0);
                map_to_image[elem] = new List<string>(); 
            }
            ListImages.ItemsSource = imgs;
        }
        private void Open(object sender, RoutedEventArgs e)
        {
            lst.Clear();
            lst_for_class = new List<int>();
            for_listbox2 = new ObservableCollection<string>();
            foreach (var elem in Classificator.classLabels)
            {
                lst_for_class.Add(0);
                for_listbox2.Add(elem + " ");
            }
            ListBox2.ItemsSource = for_listbox2;
            FolderBrowserDialog dialog = new FolderBrowserDialog();
            var res = dialog.ShowDialog();
            if (res == System.Windows.Forms.DialogResult.OK)
            {
                folderName = dialog.SelectedPath;
                Rec.IsEnabled = true;
            }
            cl = new Classificator(folderName);
        }
        public void StartRec(object sender, RoutedEventArgs e)
        {
            Rec.IsEnabled = false;
            Stop.IsEnabled = !Rec.IsEnabled;
            Thread thread1 = new Thread(() => cl.recognize());
            thread1.IsBackground = true;
                thread1.Start();
                Thread thread = new Thread(() =>
                {
                    foreach (var r in cl.answers())
                    {
                        Dispatcher.BeginInvoke((Action)(() =>
                           {
                           lst.Add(r);
                           string tem = "";
                           string[] massiv = r.Split('-');
                           tem = massiv[1];
                           int p = map[tem];
                           lst_for_class[p] += 1;
                           var t = Classificator.classLabels.ToList<string>().Zip(lst_for_class, (x, y) => x + "-" + y.ToString()).ToList<string>();
                           var oc = new ObservableCollection<string>();
                           foreach (var item in t)
                               oc.Add(item);
                           for (int k = 0; k < for_listbox2.Count; k++)
                           {
                               for_listbox2[k] = t[k];
                           }

                           BitmapImage myBitmapImage = new BitmapImage();
                           myBitmapImage.BeginInit();
                           myBitmapImage.UriSource = new Uri(folderName + "\\" + massiv[0]);
                           myBitmapImage.DecodePixelWidth = 200;
                           myBitmapImage.EndInit();
//                           ImagePath = folderName + "\\" + massiv[0];

                               //                              BitmapImage bmi = new BitmapImage(new Uri(folderName + "\\" + massiv[0], UriKind.Relative));
                               map_to_image[tem].Add(folderName + "\\" + massiv[0]);

                           }));
                    }
                    Dispatcher.BeginInvoke((Action)(() =>
                    {
                        Stop.IsEnabled = false;
                        Rec.IsEnabled = false;
                    }));
                }
                 );
            thread.IsBackground = true;
                thread.Start();
 
        }
        public void StopRec(object sender, RoutedEventArgs e)
        {
            Stop.IsEnabled = false;
            Rec.IsEnabled = false;
            cl.stop();
        }
        private void SelChng(object sender, EventArgs e)
        {
            imgs.Clear();
            var item = ListBox2.SelectedItem;
            foreach(var t in map_to_image[(item as string).Split('-')[0]])
            {
                imgs.Add(new Img(t));
            }
        }
    }
    public class Img : INotifyPropertyChanged
    {
        private string Image;

        public event PropertyChangedEventHandler PropertyChanged;
        public Img(string str)
        {
            ImagePath = str;
        }

        public string ImagePath
        {
            get
            {
                return Image;
            }

            set
            {
                Image = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Image"));
            }
        }
    }
}
