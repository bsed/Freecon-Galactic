//using System;
//using System.Windows.Forms;
//using Server.Managers;

//namespace GUI
//{
//    public partial class BaseView : Form
//    {
//        public BaseView()
//        {
//            InitializeComponent();
//            button1.Click += Btn_Clicked;
//            openNewShipMenu.Click += openNewShipMenu_Click;

//            showSystemViewButton.Click += showSystemViewButton_Click;
//            portViewOpen.Click += portViewOpen_Click;
//        }

//        private void portViewOpen_Click(object sender, EventArgs e)
//        {
//            var pvView = new PortView();
//            pvView.Show();
//        }

//        private void showSystemViewButton_Click(object sender, EventArgs e)
//        {
//            var sysView = new SystemView();
//            sysView.Show();
//        }

//        private void openNewShipMenu_Click(object sender, EventArgs e)
//        {
//            //var nsView = new NewShip();
//            //nsView.Show();
//            throw new Exception("newShipMenu is no longer implemented, needs to be fixed.");
//        }

//        public void Btn_Clicked(object ob, EventArgs e)
//        {
//            //var playerView = new PlayerView();
//            //playerView.Show();
//        }

//        private void button5_Click(object sender, EventArgs e)
//        {
//            return; // Breaking this since it no longer works. Should be a command or something.
//            //Program.createOne = true;
//            //Program.createType = ShipTypes.NPC_Barge;
//            /*
//                        for (int i = 0; i < PlayerManager.Players.Count; i++)
//                        {
//                            if (PlayerManager.Players[i].connection == null) // Only change online ships
//                                continue;
//                            //PlayerManager.Players[i].ship.shipType = (byte)ShipTypes.player_Barge;
//                            PlayerManager.Players[i].ship.primaryWeapon = new Laser();

//                            NetOutgoingMessage msg = ConnectionManager.server.CreateMessage();
//                            msg.Write((byte)MessageTypes.ChangeShipType);
//                            msg.Write((byte)ShipTypes.player_Barge);
//                            msg.Write((byte)WeaponTypes.laser);
//                            ConnectionManager.server.SendMessage(msg, PlayerManager.Players[i].connection, NetDeliveryMethod.ReliableOrdered);
//                        }
//            */
//        }

//        private void button3_Click(object sender, EventArgs e)
//        {
//            return; // Breaking this since it no longer works. Should be a command or something.
//            //Program.createOne = true;
//            //Program.createType = ShipTypes.NPC_Penguin;
//            /*
//                        for (int i = 0; i < PlayerManager.Players.Count; i++)
//                        {
//                            if (PlayerManager.Players[i].connection == null) // Only change online ships
//                                continue;
//                            //PlayerManager.Players[i].ship.shipType = (byte)ShipTypes.player_Penguin;
//                            PlayerManager.Players[i].ship.primaryWeapon = new Laser();


//                            NetOutgoingMessage msg = ConnectionManager.server.CreateMessage();
//                            msg.Write((byte)MessageTypes.ChangeShipType);
//                            msg.Write((byte)ShipTypes.player_Penguin);
//                            msg.Write((byte)WeaponTypes.laser);

//                            ConnectionManager.server.SendMessage(msg, PlayerManager.Players[i].connection, NetDeliveryMethod.ReliableOrdered);
//                        }
//            */
//        }

//        private void button4_Click(object sender, EventArgs e)
//        {
//            return; // Breaking this since it no longer works. Should be a command or something.
//            //Program.createOne = true;
//            //Program.createType = ShipTypes.NPC_BattleCruiser;
//            /*
//                        for (int i = 0; i < PlayerManager.Players.Count; i++)
//                        {
//                            if (PlayerManager.Players[i].connection == null) // Only change online ships
//                                continue;
//                            //PlayerManager.Players[i].ship.shipType = (byte)ShipTypes.player_BattleCruiser;
//                            PlayerManager.Players[i].ship.primaryWeapon = new BC_Laser();

//                            NetOutgoingMessage msg = ConnectionManager.server.CreateMessage();
//                            msg.Write((byte)MessageTypes.ChangeShipType);
//                            msg.Write((byte)ShipTypes.player_BattleCruiser);
//                            msg.Write((byte)WeaponTypes.BC_Laser);
//                            ConnectionManager.server.SendMessage(msg, PlayerManager.Players[i].connection, NetDeliveryMethod.ReliableOrdered);
//                        }
//            */
//        }

//        private void button6_Click(object sender, EventArgs e)
//        {
//            return; // Breaking this since it no longer works. Should be a command or something.
//            /*
//                        for (int i = 0; i < PlayerManager.Players.Count; i++)
//                        {
//                            if (PlayerManager.Players[i].connection == null) // Only change online ships
//                                continue;

//                            //PlayerManager.Players[i].ship.shipType = (byte)ShipTypes.player_Reaper;
//                            PlayerManager.Players[i].ship.primaryWeapon = new LaserWave();

//                            NetOutgoingMessage msg = ConnectionManager.server.CreateMessage();
//                            msg.Write((byte)MessageTypes.ChangeShipType);
//                            msg.Write((byte)ShipTypes.player_Reaper);
//                            msg.Write((byte)WeaponTypes.LaserWave);
//                            ConnectionManager.server.SendMessage(msg, PlayerManager.Players[i].connection, NetDeliveryMethod.ReliableOrdered);
//                        }
//            */
//        }

//        private void button7_Click(object sender, EventArgs e)
//        {
//            ConsoleManager.WriteToFreeLine("Button not implemented.");
//        }

//        private void button8_Click(object sender, EventArgs e)
//        {
//            ConsoleManager.WriteToFreeLine("Button not implemented.");
//        }

//        private void button9_Click(object sender, EventArgs e)
//        {
//            ConsoleManager.WriteToFreeLine("Button not implemented.");
//        }

//        private void button10_Click(object sender, EventArgs e)
//        {
//            ConsoleManager.WriteToFreeLine("Button not implemented.");
//        }

//        private void button11_Click(object sender, EventArgs e)
//        {
//            ConsoleManager.WriteToFreeLine("Button not implemented.");
//        }

//        private void BaseView_Load(object sender, EventArgs e)
//        {
//        }

//        private void button2_Click(object sender, EventArgs e)
//        {
//            ConsoleManager.WriteToFreeLine("You suck");
//        }
//    }
//}