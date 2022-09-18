namespace ScooterMiners.Shared
{
    public class Beacon
    {
        public bool IsActive { get; set; }
        public bool IsBeacon { get; set; }
        public int MAC { get; set; }
        public int PIN { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public long Ticks { get; set; }
    }
}