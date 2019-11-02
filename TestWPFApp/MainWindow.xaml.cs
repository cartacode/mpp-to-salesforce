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
using System.Windows.Forms;

using net.sf.mpxj;
using net.sf.mpxj.reader;
using net.sf.mpxj.writer;
using net.sf.mpxj.mpp;
using System.Collections.ObjectModel;
using java.util;
using System.Collections;
using java.lang;
using Salesforce.Force;
using System.Globalization;

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
        public string Practice_Management_Go_Live_Date__c { get; set; }
    }

    public class Project
    {
        public string Id { get; set; }
        public string name { get; set; }

        public string EHR_Actual_Go_Live_Date__c { get; set; }
        public string Practice_Management_Actual_Go_Live_Date__c { get; set; }

        // public string Practice_Management_Go_Live_Date__c { get; set; }
    }

    public partial class MainWindow : Window
    {
        string MPPFolderPath = "C:";

        public MainWindow()
        {
            InitializeComponent();
        }

        private static EnumerableCollection ToEnumerable(Collection javaCollection)
        {
            return new EnumerableCollection(javaCollection);
        }

        private async void Button_Click(object sender, RoutedEventArgs e) {
            string instanceUrl = "https://greenwayhealth.my.salesforce.com/";
            string accessToken = "";
            string apiVersion = "v37.0";
            string folderPath = "";

            ForceClient client = new ForceClient(instanceUrl, accessToken, apiVersion);

            // Empty TextBlock
            statusBlock.Text = "";

            using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
            {
                dialog.SelectedPath = Properties.Settings.Default.MPPFolderPath;
                System.Windows.Forms.DialogResult result = dialog.ShowDialog();
                folderPath = dialog.SelectedPath;
                Properties.Settings.Default.MPPFolderPath = dialog.SelectedPath;
                Properties.Settings.Default.Save();
            }

            DirectoryInfo objDirectoryInfo = new DirectoryInfo(folderPath);
            FileInfo[] mppFiles = objDirectoryInfo.GetFiles("*.mpp", SearchOption.TopDirectoryOnly);

            if (mppFiles.Length == 0)
            {
                System.Windows.MessageBox.Show("There are no MPP files");
            }
            else
            {
                // Read mpp files
                foreach (var file in mppFiles)
                {
                    ProjectReader reader = new MPPReader();
                    ProjectFile projectObj = reader.read($"{file.FullName}");
                    var projectName = file.Name.Replace(".mpp", "");

                    // Get Tasks from MPP file
                    foreach (net.sf.mpxj.Task task in ToEnumerable(projectObj.getAllTasks()))
                    {
                        // Filter tasks to be updated
                        if (task.getID().toString() == "222")
                        {
                            var projects = await client.QueryAsync<Project>(
                                $"SELECT Id, Name, EHR_Actual_Go_Live_Date__c FROM pse__Proj__c where Name like 'Baton Rouge Primary Care Collaborative%' limit 1");

                            foreach (var project in projects.Records)
                            {
                                DateTime startDate = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddMilliseconds(
                                    task.getStart().getTime()).ToLocalTime();
                                DateTime endDate = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddMilliseconds(
                                    task.getFinish().getTime()).ToLocalTime();

                                string startDataVal = startDate.ToString("yyyy-MM-ddTHH:mm:ss+0000");
                                string endDataVal = startDate.ToString("yyyy-MM-ddTHH:mm:ss+0000");

                                var updateProject = new Project()
                                {
                                    name = project.name,
                                    EHR_Actual_Go_Live_Date__c = startDataVal,
                                    Practice_Management_Actual_Go_Live_Date__c = startDataVal
                                };

                                var success = await client.UpdateAsync("pse__Proj__c", project.Id, updateProject);
                                var responseMessage = "is successfully updated!";
                                if (success.Success == true)
                                {
                                    responseMessage = "is not updated";
                                }

                                statusBlock.Text = statusBlock.Text + $"Project: {project.name} # " +
                                    $"Task: {task.getID().toString()} {responseMessage} \n";

                                //Resource r = projectObj.getResourceByUniqueID(task.getUniqueID());

                                //if (r != null)
                                //{
                                //    Console.WriteLine($"{r.getName()}");
                                //}

                            }
                        }
                    }
                }
            }

        }

        private async void Button_Click_1(object sender, RoutedEventArgs e)
        {
            var instanceUrl = "https://greenwayhealth.my.salesforce.com/";
            var accessToken = "00D30000001ICYF!AQ4AQFGI9G3LB87J0Jwi7NA1DzTaFnn1_V.CW4jC4sIRjPb71wnpWffZ6sehZW2xBUotcPODYb4MYbupztO3DAI7nkPg6Z5j";
            var apiVersion = "v37.0";
            statusBlock.Text = "";

            //var client = new ForceClient(instanceUrl, accessToken, apiVersion);
            //var projectName = "Intergy Project Plan";

            //var projects = await client.QueryAsync<Project>($"SELECT Id, name FROM pse__Proj__c where name like '%{projectName}%'");

            //foreach (var project in projects.Records)
            //{
            //    Console.WriteLine(project.name);
            //}

            //var tasks = await client.QueryAsync<Task>("SELECT Id, name, pse__Assigned_Resources__c FROM pse__Project_Task__c limit 5");

            //foreach (var task in tasks.Records)
            //{
            //    Console.WriteLine(task.name);
            //}

        }
    }
}
