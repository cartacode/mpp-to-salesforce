using System;
using System.Collections.Generic;
using System.IO;
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
using java.lang;
using Salesforce.Force;

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

    public class Task
    {
        public string Id { get; set; }
        public string name { get; set; }
        public string pse__Assigned_Resources__c { get; set; }
    }

    public class Account
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }

    public class Project
    {
        public string Id { get; set; }
        public string name { get; set; }
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
            // Empty TextBlock
            statusBlock.Text ="";

            DirectoryInfo objDirectoryInfo = new DirectoryInfo(@"G:\projects\.NET\MPP");
            FileInfo[] mppFiles = objDirectoryInfo.GetFiles("*.mpp", SearchOption.AllDirectories);

            foreach (var file in mppFiles)
            {
                ProjectReader reader = new MPPReader();
                ProjectFile projectObj = reader.read($"{file.FullName}");

                foreach (net.sf.mpxj.Task task in ToEnumerable(projectObj.getAllTasks()))
                {
                    //statusBlock.Text = statusBlock.Text +
                    //    ("Task: " + task.getName() + " ID=" + task.getID() + " Unique ID=" + task.getUniqueID() + "\n");

                    if (task.getID().toString() == "222")
                    {
                        Resource r = projectObj.getResourceByUniqueID(task.getUniqueID());
                        statusBlock.Text = statusBlock.Text + task.getName() + "\n";

                        if (r != null)
                        {
                            Console.WriteLine($"{r.getName()}");
                        }
                    }
                }
            }
        }

        private async void Button_Click_1(object sender, RoutedEventArgs e)
        {
            var instanceUrl = "https://xxx.salesforce.com/";
            var accessToken = "xxxxxxxxxxxxxxxxxxxxxxxxxxx";
            var apiVersion = "v37.0";

            var client = new ForceClient(instanceUrl, accessToken, apiVersion);
            var projectName = "Intergy Project Plan";

            var projects = await client.QueryAsync<Project>($"SELECT Id, name FROM pse__Proj__c where name like '%{projectName}%'");

            foreach (var project in projects.Records)
            {
                Console.WriteLine(project.name);
            }

            //var tasks = await client.QueryAsync<Task>("SELECT Id, name, pse__Assigned_Resources__c FROM pse__Project_Task__c limit 5");

            //foreach (var task in tasks.Records)
            //{
            //    Console.WriteLine(task.name);
            //}

        }
    }
}
