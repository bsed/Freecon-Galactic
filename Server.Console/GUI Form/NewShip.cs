//using System;
//using System.Collections.Generic;
//using System.Windows.Forms;
//using Server.Managers;
//using Server.Models.TypeEnums;

//namespace GUI
//{
//    public partial class NewShip : Form
//    {
//        private readonly BindingSource bs = new BindingSource();

//        public NewShip()
//        {
//            InitializeComponent();
//            addToDatabaseButton.Click += addToDatabaseButton_Click;

//            rebindData();

//            removeship.Click += removeship_Click;
//            pushToDB.Click += pushToDB_Click;

//            databaseRefresh.Click += databaseRefresh_Click;

//            // Binds data to drop down menu
//            var GraphicTypeList = new List<ShipTextures>();
//            for (int i = 0; i <= (byte)ShipTextures.Reaper; i++)
//                GraphicTypeList.Add((ShipTextures)i);

//            graphicComboBox.DataSource = GraphicTypeList;

//            // Binds data to drop down menu
//            var ShipTypeList = new List<ShipTypes>();
//            for (int i = 0; i <= (byte)ShipTypes.player_CustomShip; i++)
//                ShipTypeList.Add((ShipTypes)i);

//            enumBox.DataSource = ShipTypeList;
//        }

//        private void databaseRefresh_Click(object sender, EventArgs e)
//        {
//            //DatabaseManager.RefreshFromDatabase();
//            rebindData();
//        }

//        private void pushToDB_Click(object sender, EventArgs e)
//        {
//            //DatabaseManager.PushNewShipList(DatabaseManager.ShipTypeDB);
//        }

//        private void rebindData()
//        {
//            //bs.DataSource = DatabaseManager.ShipTypeDB;
//            shipGridView.DataSource = bs;
//        }

//        private void removeship_Click(object sender, EventArgs e)
//        {
//            try
//            {
//                if (shipGridView.SelectedRows.Count > 0)
//                {
//                    bs.RemoveAt(shipGridView.SelectedRows[0].Index);
//                }
//                else
//                {
//                    MessageBox.Show("Please select one row");
//                }
//            }
//            catch
//            {
//                MessageBox.Show("Some sort of exception, bad selection.");
//            }
//        }

//        private void addToDatabaseButton_Click(object sender, EventArgs e) // Adds a new ship to the database
//        {
//            var dbs = new ShipStats
//                          {
//                              Name = shipNameTextBox.Text,
//                              Price = int.Parse(priceTextBox.Text),
//                              Shields = int.Parse(shieldsTextBox.Text),
//                              Hull = int.Parse(hullTextBox.Text),
//                              Energy = int.Parse(energyTextBox.Text),
//                              Cargo = int.Parse(cargoTextBox.Text),
//                              Value = int.Parse(valueTextBox.Text),
//                              TopSpeed = int.Parse(topSpeedTextBox.Text),
//                              Acceleration = int.Parse(thrustTextBox.Text),
//                              TurnRate = float.Parse(turnTextBox.Text),
//                              RegenRate = float.Parse(regenTextBox.Text),
//                              Graphic = (ShipTextures)graphicComboBox.SelectedItem,
//                              ThrustGraphic = thrustTypeTextBox.Text,
//                              Description = descriptionTextBox.Text,
//                              Class = classTextBox.Text,
//                              //getShipType = (ShipTypes) enumBox.SelectedItem,
//                              SyncedWithClient = clientSyncedBool.Checked
//                          };
//            ConsoleManager.WriteToFreeLine("We have added a new ship, " + dbs.Name);
//            //DatabaseManager.PushNewShip(dbs);
//            //bs.DataSource = DatabaseManager.ShipTypeDB;
//        }

//        private void label1_Click(object sender, EventArgs e)
//        {
//        }

//        private void splitContainer1_Panel2_Paint(object sender, PaintEventArgs e)
//        {
//        }

//        private void label13_Click(object sender, EventArgs e)
//        {
//        }

//        private void shipGridView_Resize(object sender, EventArgs e)
//        {
//            shipGridView.Width = Width; //doesn't work :(
//        }
//    }
//}