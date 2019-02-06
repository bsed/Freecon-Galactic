using System.Collections.Generic;
using Core.Models.Enums;

namespace Core.Models
{
    public class Slider
    {

        public SliderTypes SliderType { get; set; }

        public string DisplayName { get; set; }

        public string Tooltip { get; set; }

        public float CurrentValue { get; set; }


        public Slider(SliderTypes sliderType, float currentValue)
        {
            SliderType = sliderType;
            CurrentValue = currentValue;
            DisplayName = TypeToDisplayName[SliderType];
            Tooltip = TypeToTooltip[SliderType];
        }

        public static Dictionary<SliderTypes, string> TypeToTooltip = new Dictionary<SliderTypes,string>()
        {
            {SliderTypes.Construction, "Modifies rate of colony structure construction."},
            {SliderTypes.Industry, "Modifies rate of factory output."},
            {SliderTypes.Mining, "Modifies rate of resource extraction."},
            {SliderTypes.Recreation, "Modifies rate of colonist medicine use."},
            {SliderTypes.Research, "Modifies rate of colony research discovery."},
            {SliderTypes.TaxRate, "Modifies how hard you fuck your hard working colonists."},

        };

        public static Dictionary<SliderTypes, string> TypeToDisplayName = new Dictionary<SliderTypes, string>()
        {
            {SliderTypes.Construction, "Construction"},
            {SliderTypes.Industry, "Industry"},
            {SliderTypes.Mining, "Mining"},
            {SliderTypes.Recreation, "Recreation"},
            {SliderTypes.Research, "Research"},
            {SliderTypes.TaxRate, "Tax Rate"},

        };

    }
}
