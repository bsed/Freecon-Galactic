namespace Core.Models.Enums
{
    public enum SliderTypes:byte
    {
        //The way colonies are set up at the time of writing, each structure takes a certain number of colonists, so we'll have to either change that, or rethink "sliders"
        Construction,
        Research,
        Mining,
        Industry,
        Recreation,
        TaxRate,
        
    }
}
