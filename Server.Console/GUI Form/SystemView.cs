//using System;
//using System.Windows.Forms;
//using Server.Managers;

//namespace GUI
//{
//    public partial class SystemView : Form
//    {
//        private readonly BindingSource bs;

//        public SystemView()
//        {
//            InitializeComponent();
//            bs = new BindingSource();
//            rebindData();
//            refreshView.Click += refreshView_Click;
//        }

//        private void refreshView_Click(object sender, EventArgs e)
//        {
//            rebindData();
//            systemDataGrid.Refresh();
//        }

//        private void SystemView_Load(object sender, EventArgs e)
//        {
//        }

//        private void rebindData()
//        {

//            ConsoleManager.WriteToFreeLine("SystemView.Rebind is disabled.", ConsoleMessageType.Warning);
//            //bs.DataSource = GalaxyManager.Systems;
//            //systemDataGrid.DataSource = bs;
//        }
//    }
//}