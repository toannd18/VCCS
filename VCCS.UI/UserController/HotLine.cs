using System.Windows.Forms;

namespace VCCS.UI.UserController
{
    public partial class HotLine : UserControl
    {
        public string LblHotLine
        {
            get
            {
                return lblHotLine.Text;
            }
            set
            {
                lblHotLine.Text = value;
            }
        }

        public string LblSDT
        {
            get { return lblSdt.Text; }
            set { lblSdt.Text = value; }
        }

        public HotLine()
        {
            InitializeComponent();
        }
    }
}