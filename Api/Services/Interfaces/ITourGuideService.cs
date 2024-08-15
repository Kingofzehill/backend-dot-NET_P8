using GpsUtil.Location;
using TourGuide.Users;
using TourGuide.Utilities;
using TripPricer;

namespace TourGuide.Services.Interfaces
{
    public interface ITourGuideService
    {
        Tracker Tracker { get; }

        void AddUser(User user);
        List<User> GetAllUsers();
        // (FNCT01.03) populate FNCT01 updates: List<NearbyAttraction> replaces List<Attraction>. 
        // (FNCT01.07) populate GetNearByAttractions updates: add user in entry parameter.
        Task<List<NearbyAttraction>> GetNearByAttractions(User user, VisitedLocation visitedLocation);
        List<Provider> GetTripDeals(User user);
        User GetUser(string userName);
        VisitedLocation GetUserLocation(User user);
        List<UserReward> GetUserRewards(User user);
        Task<VisitedLocation> TrackUserLocation(User user);
    }
}