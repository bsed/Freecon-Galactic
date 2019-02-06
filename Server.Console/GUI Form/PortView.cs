//using System;
//using System.Windows.Forms;
//using Server.Managers;

//namespace GUI
//{
//    public partial class PortView : Form
//    {
//        private readonly BindingSource bs;

//        public PortView()
//        {
//            InitializeComponent();
//            bs = new BindingSource();
//            rebindData();
//            refreshView.Click += refreshView_Click;
//            portDataGrid.AutoGenerateColumns = false;
//        }

//        private void refreshView_Click(object sender, EventArgs e)
//        {
//            rebindData();
//            portDataGrid.Refresh();
//        }

//        private void rebindData()
//        {



//            //bs.DataSource = GalaxyManager.PortListForUI;
//            //portDataGrid.DataSource = bs;

//            // This is a pain to get everything to bind correctly... Will have to play with more.
//            /*foreach (Port p in GalaxyManager.PortListForUI)
//            {
//                portDataGrid.Columns.Add(new DataGridViewComboBoxColumn
//                {
//                    DataSource = p.ShipsForSale
//                });
//            }*/
//        }
//    }
//}