namespace Freecon.Client.Mathematics.TileEditor
{
//    internal class XMLSaver
//    {
//        private string parseMe;
//        private XDocument tileSaver;

//        public void SaveLevel(PlanetLevel level, String filename)
//        {
//            tileSaver = new XDocument(
//                new XElement("PlanetLayout",
//                             new XElement("PlanetType", level.PlanetType),
//                             new XElement("Module",
//                                          new XElement("SizeX", level.xSize),
//                                          new XElement("SizeY", level.ySize)
//                                 )
//                    )
//                );
//            string saveString = "";
//            for (int y = 0; y < level.ySize; y++)
//                for (int x = 0; x < level.xSize; x++)
//                {
//                    if (y == level.ySize - 1 && x == level.xSize - 1)
//                        saveString += (byte) level.PlanetMap[x, y].type;
//                    else
//                        saveString += (byte) level.PlanetMap[x, y].type + ",";
//                }

//            tileSaver.Element("PlanetLayout") // Saves string to 
//                .Element("Module")
//                .Add(new XElement("TileList", saveString));

//            tileSaver.Save(filename);
//#if DEBUG
//            Console.WriteLine("XML Saved, " + level.xSize *level.ySize + " tiles encoded.");
//#endif
//        }

//        public PlanetLevel LoadLevel(ContentManager content, String filename)
//        {
//            try
//            {
//                var XmlRdr = new XmlTextReader(filename);

//                bool LevelInitialized = false;
//                int xSize = 0, ySize = 0;
//                String planetType = "";
//                var newLevel = new PlanetLevel(content, 0, 0, "");

//                while (XmlRdr.Read())
//                {
//                    if (XmlRdr.NodeType == XmlNodeType.Element && XmlRdr.Name == "PlanetType")
//                    {
//                        planetType = XmlRdr.ReadElementContentAsString();
//                    }
//                    if (XmlRdr.NodeType == XmlNodeType.Element && XmlRdr.Name == "SizeX")
//                    {
//                        xSize = XmlRdr.ReadElementContentAsInt();
//                    }
//                    if (XmlRdr.NodeType == XmlNodeType.Element && XmlRdr.Name == "SizeY")
//                    {
//                        ySize = XmlRdr.ReadElementContentAsInt();
//                        newLevel = new PlanetLevel(content, xSize, ySize, planetType);
//                        LevelInitialized = true;
//                    }
//                    if (LevelInitialized)
//                    {
//                        if (XmlRdr.NodeType == XmlNodeType.Element
//                            && XmlRdr.Name == "TileList")
//                        {
//                            parseMe = XmlRdr.ReadElementString();
//                            string[] mapArray = parseMe.Split(',');
//                                // Splits string in element into small strings to be parsed to ints.
//                            for (int y = 0; y < ySize; y++)
//                                for (int x = 0; x < xSize; x++)
//                                {
//                                    //newLevel.PlanetMap[x, y].tileType = Convert.ToInt32(mapArray[y * xSize + x]);
//                                    // Takes a 1-dimensional array, iterates over it, and turns it into a 2d array. Converts string to Int
//                                }
//#if DEBUG
//                            Console.WriteLine("Level Read, " + xSize * ySize + " tiles decoded.");
//#endif
//                            break;
//                        }
//                    }
//                }

//                XmlRdr.Close();
//                return newLevel;
//            }
//            catch
//            {
//                var newLevel = new PlanetLevel(content, "Earthlike", content.Load<Texture2D>(@"Modules/4wayC"));
//                return newLevel;
//            }
//        }

//        public void SaveConfig(String filename)
//        {
//            tileSaver = new XDocument(
//                new XElement("LoadingConfig",
//                             new XElement("Keys")
//                    )
//                );
//            for (int y = 1; y < 10; y++)
//            {
//                tileSaver.Element("LoadingConfig")
//                    .Element("Keys")
//                    .Add(new XElement("Key" + y, "FileName"));
//            }
//            tileSaver.Save(filename);
//        }

//        public String[] LoadConfig(String filename)
//        {
//            var names = new String[9];
//            var XmlRdr = new XmlTextReader(filename);
//            try
//            {
//                while (XmlRdr.Read())
//                {
//                    for (int y = 1; y < 10; y++)
//                    {
//                        if (XmlRdr.NodeType == XmlNodeType.Element && XmlRdr.Name == "Key" + y)
//                        {
//                            names[y - 1] = XmlRdr.ReadElementContentAsString();
//                        }
//                    }
//                }
//            }
//            catch
//            {
//            }
//            return names;
//        }
//    }
}