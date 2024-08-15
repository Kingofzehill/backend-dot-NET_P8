namespace GpsUtil.Location
{
    public class NearbyAttraction : Attraction
    {
        // (FNCT01.03) Add Distance between user location and attraction.
        public double RewardPoints { get; }
        public double Distance { get; }
        public double UserLongitude { get; }
        public double UserLatitude { get; }        
        public NearbyAttraction(double rewardPoints, double distance, double userLongitude, double userLatitude, string attractionName, string city, string state, double latitude, double longitude) : base(attractionName, city, state, latitude, longitude)
        {
            // (FNCT01.03)
            RewardPoints = rewardPoints;
            Distance = distance;
            UserLongitude = userLongitude;
            UserLatitude = userLatitude;
        }
    }
}
