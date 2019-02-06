using Freecon.Models.TypeEnums;
using Server.Models;
using System.Collections.Generic;

public class PSystemModel : AreaModel
{
    public override AreaTypes AreaType { get { return AreaTypes.System; } }

    public int NumberOfPlanets { get; set; }
    public bool HasPort { get; set; }
    public bool HasTwoOrMore { get; set; }
    public bool IsCluster { get; set; }

    public HashSet<int> MoonIDs { get; set; }

    public HashSet<int> PlanetIDs { get; set; }

    public HashSet<int> PortIDs { get; set; }

    public int? StarBaseID { get; set; }

    public Star Star { get; set; }


    public PSystemModel()
    {
        PlanetIDs = new HashSet<int>();
        MoonIDs = new HashSet<int>();
        PortIDs = new HashSet<int>();
        Warpholes = new List<Warphole>();
        StructureIDs = new HashSet<int>();
    }
}