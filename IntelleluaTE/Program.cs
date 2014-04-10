#region Using Directives

using System;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using System.ServiceModel;
using System.ServiceModel.Description;
#endregion Using Directives


namespace SCide
{
    public class ServiceProxy : ClientBase<IService1>
    {
        public ServiceProxy()
            : base(new ServiceEndpoint(ContractDescription.GetContract(typeof(IService1)),
                new NetNamedPipeBinding(), new EndpointAddress("net.pipe://localhost/IntelluaIDE" + AppDomain.CurrentDomain.BaseDirectory.GetHashCode().ToString())))
        {

        }
        public void OpenFile(string filename)
        {
            Channel.IPCOpenFile(filename);
        }
    }

    internal static class Program
    {
        #region Fields

        public static MainForm _mainForm = null;

        #endregion Fields


        #region Methods

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        private static void Main(string[] args)
        {
            System.Threading.Mutex mutex = new System.Threading.Mutex(false, "IntelluaIDE" + AppDomain.CurrentDomain.BaseDirectory.GetHashCode().ToString());
            try
            {
                if (mutex.WaitOne(0, false))
                {
                             // Run the application
                    Application.EnableVisualStyles();
                    Application.SetCompatibleTextRenderingDefault(false);
                    data = new Intellua.AutoCompleteData("classdef.xml");
                    _mainForm = new MainForm(args);
                    Application.Run(_mainForm);
                }
                else
                {
                    var sp = new ServiceProxy();

                    if (args != null && args.Length != 0)
                    {
                        //Open the document specified on the command line
                        FileInfo fi = new FileInfo(args[0]);
                        if (fi.Exists)
                        sp.OpenFile(fi.FullName);
                       
                    }
                    return;
                }
            }
            finally
            {
                if (mutex != null)
                {
                    mutex.Close();
                    mutex = null;
                }
            }



        }

        #endregion Methods

        public static Intellua.AutoCompleteData data;
        #region Properties

        public static string Title
        {
            get
            {
                object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyTitleAttribute), false);
                if (attributes.Length > 0)
                {
                    AssemblyTitleAttribute titleAttribute = (AssemblyTitleAttribute)attributes[0];
                    if (!String.IsNullOrEmpty(titleAttribute.Title))
                        return titleAttribute.Title;
                }

                // If there was no Title attribute, or if the Title attribute was the empty string, return the .exe name
                return Path.GetFileNameWithoutExtension(Assembly.GetExecutingAssembly().CodeBase);
            }
        }

        #endregion Properties
    }
}