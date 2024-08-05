using GpsUtil.Location;
using TourGuide.Users;

namespace TourGuide.Services.Interfaces
{
    public interface IRewardsService
    {
        void CalculateRewards(User user);
        double GetDistance(Locations loc1, Locations loc2);
        bool IsWithinAttractionProximity(Attraction attraction, Locations location);
        void SetDefaultProximityBuffer();
        void SetProximityBuffer(int proximityBuffer);
        // FIX 3.4 allows to change default ProximityRange value (200)
        void SetDefaultAttractionProximityRange();
        // FIX 3.4 allows to change default ProximityRange value (200).
        void SetAttractionProximityRange(int attractionProximityRange);
    }
}