namespace Core.Web.Schemas.Interfaces
{
    public interface IColonyPage
    {
        string PageName { get; set; }

        ColonyPages PageType { get; set; }

        bool IsEnabled { get; set; }

        int Order { get; set; }
    }
}
