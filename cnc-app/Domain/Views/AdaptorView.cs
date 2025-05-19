namespace APP.Domain.Views
{
    public partial class AdaptorView(Adaptor entity) : Adaptor(entity)
    {
        public string? DeviceName { get; set; }
        public string? LinkName { get; set; }
    }
}
