using System.Collections.Generic;
using Core.Models.Enums;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Core.Web.Schemas
{
    public class StatusIndicatorData
    {         

        [JsonConverter(typeof(StringEnumConverter))]
        public ImageAsset ImageAsset { get; set; }
        
        /// <summary>
        /// A URL for where the image exists.
        /// </summary>
        public string ImageLocation { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public ProblemFlagTypes ProblemFlag { get; set; }

        // Overrides ColonyStatusIndicatorType for the color. Should be an HTML compliant string.
        public string Color { get; set; }

        public string Name { get; set; }

        public StatusIndicatorData(ProblemFlagTypes problemFlag)
        {
            ProblemFlag = problemFlag;
            ImageAsset = _flagsToImageAssets[problemFlag];
            ImageLocation = _flagsToLocations[problemFlag];
            Color = _flagsToColors[problemFlag];
            Name = problemFlag.ToString();

        }

        static Dictionary<ProblemFlagTypes, ImageAsset> _flagsToImageAssets = new Dictionary<ProblemFlagTypes, ImageAsset>
        {
            {ProblemFlagTypes.HydrocarbonsDepleted, new ImageAsset()},
            {ProblemFlagTypes.OrganicsDepleted, new ImageAsset()},
            {ProblemFlagTypes.MedicineDepleted, new ImageAsset()},
            {ProblemFlagTypes.ThoriumDepeleted, new ImageAsset()},
            {ProblemFlagTypes.NotEnoughPower, new ImageAsset()},
            //TODO: add others as needed

        };

        static Dictionary<ProblemFlagTypes, string> _flagsToLocations = new Dictionary<ProblemFlagTypes, string>
        {
            {ProblemFlagTypes.HydrocarbonsDepleted, "some address here"},
            {ProblemFlagTypes.OrganicsDepleted, "some address here"},
            {ProblemFlagTypes.MedicineDepleted, "some address here"},
            {ProblemFlagTypes.ThoriumDepeleted, "some address here"},
            {ProblemFlagTypes.NotEnoughPower, "some address here"},

        };

        static Dictionary<ProblemFlagTypes, string> _flagsToColors = new Dictionary<ProblemFlagTypes, string>
        {
            {ProblemFlagTypes.HydrocarbonsDepleted, "Red"},
            {ProblemFlagTypes.OrganicsDepleted, "Red"},
            {ProblemFlagTypes.MedicineDepleted, "Red"},
            {ProblemFlagTypes.ThoriumDepeleted, "Red"},
            {ProblemFlagTypes.NotEnoughPower, "Red"},
            {ProblemFlagTypes.LowOrganics, "Yellow"},
            {ProblemFlagTypes.NegativeOrganicsRate, "Yellow"},


        };


    }
}