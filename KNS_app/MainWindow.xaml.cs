using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace KNS_app
{
    public partial class Kbase
    {
        [JsonProperty("CourseId")]
        public Guid CourseId { get; set; }

        [JsonProperty("CourseName")]
        public string CourseName { get; set; }

        [JsonProperty("ChapterCount")]
        public int ChapterCount { get; set; }

        [JsonProperty("Chapters")]
        public List<Chapter> Chapters { get; set; }

        [JsonProperty("Tests")]
        public List<Test> Tests { get; set; }
    }

    public partial class Chapter
    {
        [JsonProperty("ChapterId")]
        public int ChapterId { get; set; }

        [JsonProperty("ChapterName")]
        public string ChapterName { get; set; }

        [JsonProperty("ChapterSummary")]
        public string ChapterSummary { get; set; }

        [JsonProperty("ChapterText")]
        public string ChapterText { get; set; }
    }

    public partial class Test
    {
        [JsonProperty("ChapterId")]
        public long ChapterId { get; set; }

        [JsonProperty("TestId")]
        public long TestId { get; set; }

        [JsonProperty("Question")]
        public string Question { get; set; }

        [JsonProperty("Answer")]
        public List<string> Answer { get; set; }

        [JsonProperty("OtherAnswers")]
        public List<string> OtherAnswers { get; set; }

        [JsonProperty("Type")]
        public bool Type { get; set; }
    }

    public partial class StudentProfile
    {
        [JsonProperty("Name")]
        public string Name { get; set; }

        [JsonProperty("Sid")]
        public string Sid { get; set; }

        [JsonProperty("Pass")]
        public string Pass { get; set; }

        [JsonProperty("StudentCourses")]
        public Dictionary<Guid, StudentCourse> StudentCourses { get; set; }
    }

    public partial class StudentCourse
    {
        [JsonProperty("CourseName")]
        public string CourseName { get; set; }

        [JsonProperty("ChapterCount")]
        public int ChapterCount { get; set; }

        [JsonProperty("ChaptersLocked")]
        public List<int> ChaptersLocked { get; set; }

        [JsonProperty("ChaptersUnlocked")]
        public List<int> ChaptersUnlocked { get; set; }

        [JsonProperty("TestsPassed")]
        public List<int> TestsPassed { get; set; }

        [JsonProperty("TestsFailed")]
        public List<int> TestsFailed { get; set; }
    }

    public class SChapter
    {
        public string ChapterName { get; set; }
        public bool locked { get; set; }
        public int id { get; set; }
    }

    public class IsObsoleteToTextDecorationsConverter : IValueConverter
    {
        public int getChIdByChName(string ChName, Kbase t)
        {
            int i = 0;
            while (i < t.ChapterCount)
            {
                if (t.Chapters[i].ChapterName.Equals(ChName))
                    return t.Chapters[i].ChapterId;
                i++;
            }
            return -1;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var t = MainWindow.getT();
            var sp = MainWindow.getSP();
                if (sp.StudentCourses[t.CourseId].ChaptersLocked.Contains(getChIdByChName((string)value, t)))
                {
                    Console.WriteLine("red");
                    return Brushes.Red;
                }
            Console.WriteLine("black");
            return Brushes.Black; 
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public partial class MainWindow : Window
    {
        public static StudentProfile sp = null;
        public static Kbase t = null;

        public static StudentProfile getSP()
        {
            return sp;
        }

        public static Kbase getT()
        {
            return t;
        }

        public static void WriteToJsonFile<T>(string filePath, T objectToWrite, bool append = false) where T : new()
        {
            TextWriter writer = null;
            try
            {
                var contentsToWriteToFile = JsonConvert.SerializeObject(objectToWrite);
                writer = new StreamWriter(filePath, append);
                writer.Write(contentsToWriteToFile);
            }
            finally
            {
                if (writer != null)
                    writer.Close();
            }
        }

        public static T ReadFromJsonFile<T>(string filePath) where T : new()
        {
            TextReader reader = null;
            try
            {
                reader = new StreamReader(filePath);
                var fileContents = reader.ReadToEnd();
                return JsonConvert.DeserializeObject<T>(fileContents);
            }
            finally
            {
                if (reader != null)
                    reader.Close();
            }
        }

        public void InitialState()
        {
            btStartChapter.IsEnabled = false;
            btStartTest.IsEnabled = false;
            ChView.IsEnabled = false;
            ChView.Visibility = Visibility.Hidden;
            ChView.Source = new Uri("about:blank");
            tbq.Visibility = Visibility.Hidden;
            rba1.Visibility = Visibility.Hidden;
            rba2.Visibility = Visibility.Hidden;
            rba3.Visibility = Visibility.Hidden;
            rba4.Visibility = Visibility.Hidden;
            rba5.Visibility = Visibility.Hidden;
            Console.WriteLine("State change inactive");
        }

        public void RegularState()
        {
            Console.WriteLine("State change active");
            btStartChapter.IsEnabled = true;
            btStartTest.IsEnabled = true;
        }

        public void showChapterView(string source)
        {
            ChView.IsEnabled = true;
            ChView.Visibility = Visibility.Visible;
            ChView.Source = new Uri(source);
        }

        public void showTestView()
        {

        }

        public MainWindow()
        {
            InitializeComponent();
            InitialState();
            t = ReadFromJsonFile<Kbase>(System.IO.Path.Combine(Directory.GetCurrentDirectory(), "knowledgebase.json"));
            sp = ReadFromJsonFile<StudentProfile>(System.IO.Path.Combine(Directory.GetCurrentDirectory(), "profile.json"));
            nameLabel.Content = "Hello, " +sp.Name;
            courseName.Content = "Chapters in course '" + t.CourseName + "':";

            //create business data
            var itemList = new List<SChapter>();
            int i = 0;
            while (i < t.ChapterCount)
            {
                itemList.Add(new SChapter { ChapterName = t.Chapters[i].ChapterName, locked = sp.StudentCourses[t.CourseId].ChaptersLocked.Contains(t.Chapters[i].ChapterId), id = t.Chapters[i].ChapterId });
                i++;
            }
 
            //link business data to CollectionViewSource
            CollectionViewSource itemCollectionViewSource;
            itemCollectionViewSource = (CollectionViewSource)(FindResource("ItemCollectionViewSource"));
            itemCollectionViewSource.Source = itemList;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            WriteToJsonFile<StudentProfile>(System.IO.Path.Combine(Directory.GetCurrentDirectory(), "profile.json"), sp);
        }

        private void ChDt_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Console.WriteLine(((SChapter)ChDt.SelectedCells.First().Item).locked.ToString());
            if (((SChapter)ChDt.SelectedCells.First().Item).locked)
                InitialState();
            else
                RegularState();
            Console.WriteLine(((SChapter)ChDt.SelectedCells.First().Item).ChapterName);
        }

        private void btStartChapter_Click(object sender, RoutedEventArgs e)
        {
            InitialState();
            if (!sp.StudentCourses[t.CourseId].ChaptersLocked.Contains(((SChapter)ChDt.SelectedCells.First().Item).id))
                showChapterView(getChapterSource(t, ((SChapter)ChDt.SelectedCells.First().Item).id));
            RegularState();
        }

        private string getChapterSource(Kbase t, int id)
        {
            int i = 0;
            Console.WriteLine(id.ToString());
            while (i < t.ChapterCount)
            {
                if (t.Chapters[i].ChapterId == id)
                {
                    Console.WriteLine(System.IO.Path.Combine(Directory.GetCurrentDirectory(), t.Chapters[i].ChapterText));
                    return System.IO.Path.Combine(Directory.GetCurrentDirectory(), t.Chapters[i].ChapterText);
                }
                i++;
            }
            return "";
        }

        private void btStartTest_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
