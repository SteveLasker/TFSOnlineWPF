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
using System.Collections.ObjectModel;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.Framework.Common;
using Microsoft.TeamFoundation.Framework.Client;
using System.Net;
using Microsoft.TeamFoundation.WorkItemTracking.Client;

namespace TFSOnline {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        public MainWindow() {
            InitializeComponent();

            // Refernce for TFS Online:
            // http://blogs.msdn.com/b/buckh/archive/2013/01/07/how-to-connect-to-tf-service-without-a-prompt-for-liveid-credentials.aspx

            // setup some default values
            // a TFS Online version, notice it requires HTTPS, and doesn't include the port
            this.TFSUrlComboBox.Items.Add("https://YourSite.visualstudio.com");
            // a TFS OnPrem URL, notice it doesn't use HTTPS and it includes the port
            this.TFSUrlComboBox.Items.Add("http://yourTFSServer:8080");
            this.TFSUrlComboBox.SelectedIndex = 0;

            // **********************************************************
            // NOTE: Due to the differences in OnPrem auth and TFS Online
            // this sample will currently only work with TFS Online
            // You can instance a Widnows identity to work with OnPrem, 
            // but was rushing thru to get this working
            // **********************************************************

            // Connecting to TFS Online requires the enablement of alternate IDs, 
            // so you have the option to use just a username, not your full email
            this.userNameTextBox.Text = "YourUserName";
            this.passwordTextBox.Text = "YourPassword";
        }

        private void QueryTFSButton_Click(object sender, RoutedEventArgs e) {


            string tfsUrl = this.TFSUrlComboBox.SelectedItem.ToString();

            // Be sure to enable the alternate credentials as noted in the above blog
            NetworkCredential netCred = new NetworkCredential(
                this.userNameTextBox.Text,
                this.passwordTextBox.Text);
            BasicAuthCredential basicCred = new BasicAuthCredential(netCred);
            TfsClientCredentials tfsCred = new TfsClientCredentials(basicCred);
            tfsCred.AllowInteractive = false;

            // Depending on how you connect to TFS, you will need to construct the URL differently

            // TfsTeamProjectCollection requires a collection to be specified
            // As of 1/27/15, TFS Online only supports the /DefaultCollection
            TfsTeamProjectCollection tpc = new TfsTeamProjectCollection(
                new Uri(tfsUrl + "/DefaultCollection"),
                tfsCred);
            tpc.Authenticate();

            // TfsConfigurationServer requires just the base URL, wihtout the collection specified
            // Also note, that TFS Online requires HTTPS://
            TfsConfigurationServer configurationServer = new TfsConfigurationServer(new Uri(tfsUrl), tfsCred);
            configurationServer.Authenticate();

            // Get the catalog of team project collections
            ReadOnlyCollection<CatalogNode> collectionNodes = configurationServer.CatalogNode.QueryChildren(
                new[] { CatalogResourceTypes.ProjectCollection },
                false, CatalogQueryOptions.None);

            // List the team project collections
            foreach (CatalogNode collectionNode in collectionNodes) {
                // Use the InstanceId property to get the team project collection
                Guid collectionId = new Guid(collectionNode.Resource.Properties["InstanceId"]);
                TfsTeamProjectCollection teamProjectCollection = configurationServer.GetTeamProjectCollection(collectionId);

                // Print the name of the team project collection
                Console.WriteLine("Collection: " + teamProjectCollection.Name);
                this.NodesListBox.Items.Add("Collection: " + teamProjectCollection.Name);

                // Get a catalog of team projects for the collection
                ReadOnlyCollection<CatalogNode> projectNodes = collectionNode.QueryChildren(
                    new[] { CatalogResourceTypes.TeamProject },
                    false, CatalogQueryOptions.None);


                // In Visual Studio 2015 Preview the TFS binaries are no longer GAC'd
                // One of the binaries is a native dll that must be available for PInvoke for the managed libraries
                // If you have VS 2013 installed side by side, this won't be an issue as they are GACd in VS 2013
                // If you have a pure VS 2015 environment, for now, you'll need to copy the 32 and 64 bit binararies to your bin directory
                // Error: Unable to load DLL 'Microsoft.WITDataStore32.dll'
                var foo = teamProjectCollection.GetService(typeof(WorkItemStore));

                // List the team projects in the collection
                foreach (CatalogNode projectNode in projectNodes) {
                    Console.WriteLine(" Team Project: " + projectNode.Resource.DisplayName);
                    this.NodesListBox.Items.Add(" Team Project: " + projectNode.Resource.DisplayName);
                }
            }
        }
    }
}
