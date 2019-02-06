using Freecon.Models.TypeEnums;

namespace Server.Models.Structures
{
    public class Refinery : Structure<RefineryModel, StructureStats>
    {

        public Refinery()
        { }

        public Refinery(RefineryModel model)
        {
            _model = model;
        }

        public Refinery(float posX, float posY, int galaxyID, int ownerID, CommandCenter commandCenter, int currentAreaID):base(posX, posY, galaxyID, ownerID, currentAreaID)
        {

            _model.StructureType = StructureTypes.Refinery;
           
            _model.ThoriumOreToThorium = 1;
            _model.IronOreToSteel = 1;
            _model.BauxiteToAluminum = 1;
            _model.OrganicsToMedicine = 1;
            _model.SilicaToSilicon = 1;
            _model.HydrocarbonsToFullerenes = 1;

        }

    }

    public class RefineryModel : StructureModel<StructureStats>
    {
        #region Sliders
        //Percent directed to available resources.
        //If no resources are present, value defaults to 0.
        //Total must sum to 100
        public float ThoriumOreToThoriumSlider { get; set; }
        public float IronOreToSteelSlider { get; set; }
        public float BauxiteToAluminumSlider { get; set; }
        public float OrganicsToMedicineSlider { get; set; }
        public float SilicaToSiliconSlider { get; set; }
        public float HydrocarbonsToFullerenesSlider { get; set; }

        #endregion

        #region Conversion Rates
        //Units per minute
        public float ThoriumOreToThorium { get; set; }
        public float IronOreToSteel { get; set; }
        public float BauxiteToAluminum { get; set; }
        public float OrganicsToMedicine { get; set; }
        public float SilicaToSilicon { get; set; }
        public float HydrocarbonsToFullerenes { get; set; }


        #endregion

        public RefineryModel()
        {
            StructureType = StructureTypes.Refinery; 

        }

        public RefineryModel GetClone()
        {
            return (RefineryModel)MemberwiseClone();
        }
    }
}
