using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using net.sf.mpxj;
using net.sf.mpxj.reader;
using net.sf.mpxj.writer;
using net.sf.mpxj.mpp;
using System.Collections.ObjectModel;
using java.util;
using System.Collections;

namespace TestWPFApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>

    public class EnumerableCollection
    {
        public EnumerableCollection(Collection collection)
        {
            m_collection = collection;
        }

        public IEnumerator GetEnumerator()
        {
            return new Enumerator(m_collection);
        }

        private Collection m_collection;
    }

    public class Enumerator : IEnumerator
    {
        public Enumerator(Collection collection)
        {
            m_collection = collection;
            m_iterator = m_collection.iterator();
        }

        public object Current
        {
            get
            {
                return m_iterator.next();
            }
        }

        public bool MoveNext()
        {
            return m_iterator.hasNext();
        }

        public void Reset()
        {
            m_iterator = m_collection.iterator();
        }

        private Collection m_collection;
        private Iterator m_iterator;
    }

    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private static EnumerableCollection ToEnumerable(Collection javaCollection)
        {
            return new EnumerableCollection(javaCollection);
        }

        private void Button_Click(object sender, RoutedEventArgs e) {
            ProjectReader reader = new MPPReader();
            ProjectFile projectObj = reader.read("G:\\projects\\.NET\\Intergy Project Plan 10-12-19 - AK.mpp");

            foreach (net.sf.mpxj.Task task in ToEnumerable(projectObj.getAllTasks()))
            {
                Console.WriteLine("Task: " + task.getName() + " ID=" + task.getID() + " Unique ID=" + task.getUniqueID());
            }
            statusBlock.Text = "Hello, Here we are!";
        }
    }
}
