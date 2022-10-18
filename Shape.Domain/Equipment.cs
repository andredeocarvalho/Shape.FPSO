namespace Shape.Domain
{
    public class Equipment
    {
        public long Id { get; internal set; }
        public long VesselId { get; internal set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public string Location { get; set; }
        public bool Active { get; internal set; } = true;
    }
}