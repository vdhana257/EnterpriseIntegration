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

namespace TestPublisherHelper
{
    /// <summary>
    /// Interaction logic for SelectionPage.xaml
    /// </summary>
    public partial class SelectionPage : UserControl
    {
        public SelectionPage()
        {
            InitializeComponent();
            aux();
        }
        private void btnNext_Click(object sender, EventArgs e)
        {
            if(PublisherSelected.IsChecked == true)
            {
                Helper help = new Helper();
                home.Children.Clear();
                home.Children.Add(help);
            }

        }

        void aux()
        {
            Instructions.Text = "";
            string inst = @"        Instructions :
1. Create 2 folders - Input and Output
2. Copy Pre files with name 1_, 2_ into input folder
3. Copy post files with name 1_,2_ into output folder
4. Go to blobs and verify that abtestactual blob exists.
5. Cerate new blob
        i.vlmig - Expected
        ii.Vl - testcase
        iii.Vl - testsuite
6. Now upload inbound test suit json
        i.Name of file should be the same as the name in the file
        ii.Change following values
            i.Partner Name
            ii.Start / End date
            iii.AS2 from and to.
            iv.messageId - any
            v.SAS Token of expected.
7. Set public access level to public containers and blob
        i.Now right click - get shared access signature - set expiry time and give all access
        ii.	Copy signature and keep it somewhere.
8. Do step 7 for all 3 blobs- expected, test case and test suite
9. Upload TestSuite json as block blob.
10. Upload TechData2Test into Test case Blob
11.	Edit App.config
        i.	Change ABTestSuiteBlobContainerSASToken- URL encode in Notepad++
        ii. FileSourceActual= PrePath file
        iii.FileSourceExpected= Post file path
        iv. Change path of logs under myListener
";
            Instructions.Text = inst;
        }
    }
}
