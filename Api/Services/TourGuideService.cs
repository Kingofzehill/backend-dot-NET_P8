using GpsUtil.Location;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using TourGuide.LibrairiesWrappers.Interfaces;
using TourGuide.Services.Interfaces;
using TourGuide.Users;
using TourGuide.Utilities;
using TripPricer;

namespace TourGuide.Services;

public class TourGuideService : ITourGuideService
{
    private readonly ILogger _logger;
    private readonly IGpsUtil _gpsUtil;
    private readonly IRewardsService _rewardsService;
    private readonly TripPricer.TripPricer _tripPricer;
    public Tracker Tracker { get; private set; }
    private readonly Dictionary<string, User> _internalUserMap = new();
    private const string TripPricerApiKey = "test-server-api-key";
    private bool _testMode = true;

    public TourGuideService(ILogger<TourGuideService> logger, IGpsUtil gpsUtil, IRewardsService rewardsService, ILoggerFactory loggerFactory)
    {
        _logger = logger;
        _tripPricer = new();
        _gpsUtil = gpsUtil;
        _rewardsService = rewardsService;

        CultureInfo.CurrentCulture = new CultureInfo("en-US");

        if (_testMode)
        {
            _logger.LogInformation("TestMode enabled");
            _logger.LogDebug("Initializing users");
            InitializeInternalUsers();
            _logger.LogDebug("Finished initializing users");
        }

        var trackerLogger = loggerFactory.CreateLogger<Tracker>();

        Tracker = new Tracker(this, trackerLogger);
        AddShutDownHook();
    }

    public List<UserReward> GetUserRewards(User user)
    {
        return user.UserRewards;
    }

    // FIX Perf optimization ==> async&await.
    public async Task<VisitedLocation> GetUserLocation(User user)
    {
        return user.VisitedLocations.Any() ? user.GetLastVisitedLocation() : await TrackUserLocation(user);
    }

    public User GetUser(string userName)
    {
        return _internalUserMap.ContainsKey(userName) ? _internalUserMap[userName] : null;
    }

    public List<User> GetAllUsers()
    {
        return _internalUserMap.Values.ToList();
    }

    public void AddUser(User user)
    {
        if (!_internalUserMap.ContainsKey(user.UserName))
        {
            _internalUserMap.Add(user.UserName, user);
        }
    }

    public List<Provider> GetTripDeals(User user)
    {
        int cumulativeRewardPoints = user.UserRewards.Sum(i => i.RewardPoints);
        List<Provider> providers = _tripPricer.GetPrice(TripPricerApiKey, user.UserId,
            user.UserPreferences.NumberOfAdults, user.UserPreferences.NumberOfChildren,
            user.UserPreferences.TripDuration, cumulativeRewardPoints);
        user.TripDeals = providers;
        return providers;
    }

    // FIX Perf optimization ==> async&await / mutliple await.
    public async Task<VisitedLocation> TrackUserLocation(User user)
    {
        VisitedLocation visitedLocation = await _gpsUtil.GetUserLocation(user.UserId);
        user.AddToVisitedLocations(visitedLocation);
        await _rewardsService.CalculateRewards(user);
        return visitedLocation;
    }

    // (FNCT01) update GetNearByAttractions method for : get 5 nearby attractions of the last user location.
    // No matter how far they are.
    public List<NearbyAttraction> GetNearByAttractions(User user, VisitedLocation visitedLocation)
    {
        double distanceFromAttractionInList;
        double attractionReward;
        List<NearbyAttraction> nearbyAttractions = new ();
        List<Attraction> attractions = _gpsUtil.GetAttractions();
        // (FNCT01.09) set ProxmityBuffer from 10 to int.MaxValue. Required
        // for adding reward to Attractions further than the 10 miles proximityBuffer.
        _rewardsService.SetProximityBuffer(int.MaxValue);
        // (FNCT01.05-2) add CalculateRewards call in GetNearbyAttractions method for being
        // able to add RewardPoints to each NearbyAttraction as requested in the TODO of
        // TourGuideController.GetNearbyAttractions API method.     
        _rewardsService.CalculateRewards(user);

        for (int i = 0; i < attractions.Count; i++)
        {
            // (FNCT01.01) check distance of attraction from user localization 
            distanceFromAttractionInList = _rewardsService.GetDistance(attractions[i], visitedLocation.Location);
            // (FNCT01.02) add attractions to nearbyAttractions list.          
            attractionReward = AttractionReward(user, attractions[i]);
            var nearbyAttraction = new NearbyAttraction(attractionReward, distanceFromAttractionInList, visitedLocation.Location.Longitude,
                visitedLocation.Location.Latitude, attractions[i].AttractionName, attractions[i].City,
                attractions[i].State, attractions[i].Latitude, attractions[i].Longitude);
            nearbyAttractions.Add(nearbyAttraction);                                                                          
        }

        // Sort nearbyAttractions items list by distance descending order.
        nearbyAttractions.Sort((x, y) => x.Distance.CompareTo(y.Distance));
        // Keep the first 5 items.
        List<NearbyAttraction> firstFiveNearbyAttractions = new(nearbyAttractions.Take(5));

        // (FNCT01.09) returns to default ProximityBuffer value.
        _rewardsService.SetProximityBuffer(10);
        
        return firstFiveNearbyAttractions;
    }

    // (FNCT 01.10) returns RewardPoints of an Attraction.
    private static double AttractionReward(User user, Attraction attraction)
    {
        double attractionReward = 0;
        for (int k = 0; k < user.UserRewards.Count; k++)
        {
            if (user.UserRewards[k].Attraction.AttractionName == attraction.AttractionName)
            {
                attractionReward = user.UserRewards[k].RewardPoints;                
            }
        }
        return attractionReward;
    }

    private void AddShutDownHook()
    {
        AppDomain.CurrentDomain.ProcessExit += (sender, e) => Tracker.StopTracking();
    }

    /**********************************************************************************
    * 
    * Methods Below: For Internal Testing
    * 
    **********************************************************************************/

    private void InitializeInternalUsers()
    {
        for (int i = 0; i < InternalTestHelper.GetInternalUserNumber(); i++)
        {
            var userName = $"internalUser{i}";
            var user = new User(Guid.NewGuid(), userName, "000", $"{userName}@tourGuide.com");
            GenerateUserLocationHistory(user);
            _internalUserMap.Add(userName, user);
        }

        _logger.LogDebug($"Created {InternalTestHelper.GetInternalUserNumber()} internal test users.");
    }

    private void GenerateUserLocationHistory(User user)
    {
        for (int i = 0; i < 3; i++)
        {
            var visitedLocation = new VisitedLocation(user.UserId, new Locations(GenerateRandomLatitude(), GenerateRandomLongitude()), GetRandomTime());
            user.AddToVisitedLocations(visitedLocation);
        }
    }

    private static readonly Random random = new Random();

    private double GenerateRandomLongitude()
    {
        return new Random().NextDouble() * (180 - (-180)) + (-180);
    }

    private double GenerateRandomLatitude()
    {
        return new Random().NextDouble() * (90 - (-90)) + (-90);
    }

    private DateTime GetRandomTime()
    {
        return DateTime.UtcNow.AddDays(-new Random().Next(30));
    }
}
