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
using System.IO;
using System.Drawing;
//using Microsoft.EntityFrameworkCore;
using System.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
/*using WpfApp1.Models.Image_db;*/ 
    //using System.Data.Entity;

namespace WpfApp1
{
    public partial class MainWindow : Window
    {
        private string folderName = "";
        Classificator cl;
        ObservableCollection<string> lst;
        List<int> lst_for_class;
        ObservableCollection<string> for_listbox2;
        ObservableCollection<string> for_listbox3;
        SortedDictionary<string, int> map;
        SortedDictionary<string, int> map_to_class;
        SortedDictionary<string, List<string>> map_to_image;
        ObservableCollection<Img> imgs;
        ObservableCollection<Img> Imgs;
        ApplicationContext db;
        List<string> Names_of_files;
        System.IO.MemoryStream memoryStream = new System.IO.MemoryStream();

        public MainWindow()
        {
//            var conn = new SqlConnection(@"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=C:\Users\vdtri\source\repos\Wpf_C#_2\WpfApp1\Database1.mdf;Integrated Security=True");
            InitializeComponent();
            this.DataContext = this;
            Names_of_files = new List<string>();
            db = new ApplicationContext();
            lst = new ObservableCollection<string>();
            Label1.ItemsSource = lst;
            Rec.IsEnabled = false;
            Stop.IsEnabled = false;
            map = new SortedDictionary<string, int>();
            map_to_image = new SortedDictionary<string, List<string>>();
            lst_for_class = new List<int>();
            for_listbox2 = new ObservableCollection<string>();
            for_listbox3 = new ObservableCollection<string>();
            imgs = new ObservableCollection<Img>();
            Imgs = new ObservableCollection<Img>();
            map_to_class = new SortedDictionary<string, int>();
            int i = 0;
            foreach(var elem in Classificator.classLabels)
            {
                map.Add(elem, i);
                i++;
                lst_for_class.Add(0);
                map_to_image[elem] = new List<string>(); 
            }
            ListImages.ItemsSource = imgs;
            ListView1.ItemsSource = Imgs;
            //            db = new ApplicationContext();
        }
        private void Open(object sender, RoutedEventArgs e)
        {
            for_listbox3.Clear();
            ListBox2.SelectedIndex =  - 1;
            lst.Clear();
            imgs.Clear();
            Imgs.Clear();
            Names_of_files.Clear();
            ListBox3.ItemsSource = for_listbox3;
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

            int k = 0;
            IEnumerable<string> dir = Directory.EnumerateFiles(folderName, "*.png").Concat(Directory.EnumerateFiles(folderName, "*.jpg")).ToList();
            foreach (var item in dir)
            {
                Imgs.Add(new Img(item, ""));
                map_to_class[item] = k;
                k++;
            }
            foreach (var elem in Classificator.classLabels)
            {
                map_to_image[elem] = new List<string>();
            }
        }
        public void StartRec(object sender, RoutedEventArgs e)
        {
            Rec.IsEnabled = false;
            Stop.IsEnabled = !Rec.IsEnabled;

            Names_of_files = new List<string>();
            IEnumerable<string> dir = Directory.EnumerateFiles(folderName, "*.png").Concat(Directory.EnumerateFiles(folderName, "*.jpg")).ToList();
            foreach (var elem in dir)
            {
                var byte_rep = ImageToByteArray(elem);
                bool flag = false;
                foreach (var item in db.Images.Include(i => i.Det))
                {
                    var tre =item.Det;
                    flag = GetHashSHA1(item.Det.ByteRepresent) == GetHashSHA1(byte_rep);
                    if (flag)
                    {
                        if (item.Det.ByteRepresent.SequenceEqual<byte>(byte_rep))
                        {
                            break;
                        }
                        flag = false;
                    }
                }
                if (!flag)
                {
                    Names_of_files.Add(elem);
                }
                /*                    flag = GetHashSHA1(item.ByteRepresent) == GetHashSHA1(byte_rep);
                                    if (flag)
                                    {
                                        if (item.ByteRepresent.SequenceEqual<byte>(byte_rep))
                                        {
                                            break;
                                        }
                                        flag = false;
                                    }
                                }
                                if (!flag)
                                {
                                    Names_of_files.Add(elem);
                                }*/
            }
            if (Names_of_files.Count != 0)
            {
                //                cl = new Classificator(new List<string>());
                cl = new Classificator(Names_of_files);
                Thread thread1 = new Thread(() => cl.recognize());
                thread1.IsBackground = true;
                thread1.Start();
                Thread thread = new Thread(() =>
                {
                    func();
                }
                 );
                thread.IsBackground = true;
                thread.Start();
            }
            Thread thread2 = new Thread(() =>
                {
                    func_1();
                }
            );
            thread2.IsBackground = true;
            thread2.Start();

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
            if (item as string != null)
            {
                foreach (var t in map_to_image[(item as string).Split('-')[0]])
                {
                    imgs.Add(new Img(t, ""));
                }
            }
        }
        public void func()
        {
            foreach (var r in cl.answers())
            {
                Dispatcher.BeginInvoke((Action)(() =>
                {
                    lst.Add(r);
                    string tem = "";
                    string[] massiv = r.Split('-');
                    tem = massiv[1];
                    db.Images.Add(new Models.Image_db
                    {
                        Path = folderName + "\\" + massiv[0],
                        Det = new Models.Detail { ByteRepresent = ImageToByteArray(folderName + "\\" + massiv[0]) },
                        Name = massiv[0],
                        ClassName = massiv[1],
                        Confidence = 0.0f,
                        Counter = 0
                    }); ;
 //                   db.Details.Add(new Models.Detail { ByteRepresent = ImageToByteArray(folderName + "\\" + massiv[0]) });
                    db.SaveChanges();
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
                    map_to_image[tem].Add(folderName + "\\" + massiv[0]);
                    Imgs[map_to_class[folderName + "\\" + massiv[0]]].Clss = massiv[1];

                }));
            }
            Dispatcher.BeginInvoke((Action)(() =>
            {
                Stop.IsEnabled = false;
                Rec.IsEnabled = false;
            }));
        }
        public void func_1()
        {
            IEnumerable<string> dir = Directory.EnumerateFiles(folderName, "*.png").Concat(Directory.EnumerateFiles(folderName, "*.jpg")).ToList();
            foreach (var elem in db.Images)
            {
                if (!dir.Contains(elem.Path))
                    continue;
                Dispatcher.BeginInvoke((Action)(() =>
                {
                    elem.Counter += 1;
                    var r = elem.Name + "-" + elem.ClassName;
                    string[] massiv = { elem.Name, elem.ClassName};
                    lst.Add(r + "_from_db");
                    string tem = "";
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
                    map_to_image[tem].Add(folderName + "\\" + massiv[0]);
                    Imgs[map_to_class[folderName + "\\" + massiv[0]]].Clss = massiv[1];


                }));
            }
            Dispatcher.BeginInvoke((Action)(() =>
            {
                Stop.IsEnabled = false;
                Rec.IsEnabled = false;
            }));
        }
        public byte[] ImageToByteArray(string imagefilePath)
        {
            byte[] bData = File.ReadAllBytes(imagefilePath);
            return bData;
        }
        public string GetHashSHA1(byte[] data)
        {
            using (var sha1 = new System.Security.Cryptography.SHA1CryptoServiceProvider())
            {
                return string.Concat(sha1.ComputeHash(data).Select(x => x.ToString("X2")));
            }
        }
        void ResetDB(object sender, EventArgs e)
        {
            db.Images.RemoveRange(db.Images);
            db.Details.RemoveRange(db.Details);
            db.SaveChanges();
        }
        void Stat(object sender, EventArgs e)
        {
            if (for_listbox3.Count == 0)
            {
                foreach(var t in db.Images)
                {
                    for_listbox3.Add(t.Name + " " + t.Counter);
                }
            }
        }
    }
    public class Img : INotifyPropertyChanged
    {
        private string Image;
        private string Class;

        public event PropertyChangedEventHandler PropertyChanged;
        public Img(string str1, string str2)
        {
            ImagePath = str1;
            Class = str2;
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
        public string Clss
        {
            get
            {
                return Class;
            }

            set
            {
                Class = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Clss"));
            }
        }
    }
}