using GpsUtil.Location;
using TourGuide.LibrairiesWrappers.Interfaces;
using TourGuide.Services.Interfaces;
using TourGuide.Users;

namespace TourGuide.Services;

public class RewardsService : IRewardsService
{
    private const double StatuteMilesPerNauticalMile = 1.15077945;
    // FIX3.1 temporarily raise _defaultProximityBuffer from 10 to 1000 
    // for increasing the detection of proximity attractions.
    private readonly int _defaultProximityBuffer = 10;
    private int _proximityBuffer;
    // FIX3.2 temporarily raise _attractionProximityRange from 200 to 20000 
    // for increasing the detection of proximity attractions
    private readonly int _defaultAttractionProximityRange = 200;
    // FIX3.3 add _ProximityRange variable to RewardsService class which allow
    // to extend default proximity range value (200).  
    private int _proximityRange;
    private readonly IGpsUtil _gpsUtil;
    private readonly IRewardCentral _rewardsCentral;
    private static int count = 0;

    public RewardsService(IGpsUtil gpsUtil, IRewardCentral rewardCentral)
    {
        _gpsUtil = gpsUtil;
        _rewardsCentral =rewardCentral;
        _proximityBuffer = _defaultProximityBuffer;
        // FIX3.4
        _proximityRange = _defaultAttractionProximityRange;
    }

    public void SetProximityBuffer(int proximityBuffer)
    {
        _proximityBuffer = proximityBuffer;
    }

    public void SetDefaultProximityBuffer()
    {
        _proximityBuffer = _defaultProximityBuffer;
    }

    // FIX3.4 new SetattractionProximityRange method : allows to update default proximity range value.    
    public void SetAttractionProximityRange(int attractionProximityRange)
    {
        _proximityRange = attractionProximityRange;
    }
    // FIX3.4
    public void SetDefaultAttractionProximityRange()
    {
        _proximityRange = _defaultAttractionProximityRange;
    }

   
    public void CalculateRewards(User user)
    {
        //count++;
        List<VisitedLocation> userLocations = user.VisitedLocations;
        List<Attraction> attractions = _gpsUtil.GetAttractions();

        // FIX04.1 Fix for preventing InvalidOperationException error :
        // replace For Each loop by For loop.
        //   See : https://makolyte.com/system-invalidoperationexception-collection-was-modified-enumeration-operation-may-not-execute/#Scenario_2_-_One_thread_is_modifying_the_collection_while_another_thread_is_looping_over_it
        //   OLD : foreach (var visitedLocation in userLocations)
        for (int i = 0; i < userLocations.Count; i++)
        {
            // OLD : foreach (var attraction in attractions)
            for (int j = 0; j < attractions.Count; j++)
            {
                // FNCT01.03 add check if the attraction is not already listed in UserRewards attraction list.
                if (NearAttraction(userLocations[i], attractions[j]) && AttractionHasNoUserReward(user, attractions[j]))
                {
                    user.AddUserReward(new UserReward(userLocations[i], attractions[j], GetRewardPoints(attractions[j], user)));
                }
            }
        }

    }

    // FIX07.01 checks if the attraction is not 
    // already listed in User attractions rewards.
    private static bool AttractionHasNoUserReward(User user, Attraction attraction)
    {
        for (int k = 0; k < user.UserRewards.Count; k++)
        {
            if (user.UserRewards[k].Attraction.AttractionName == attraction.AttractionName)
            {
                return false;
            }
        }
        return true;
    }

    // FIX03.5  update IsWithinAttractionProximity method for checking distance compared
    // to _ProximityRange  intead of comparing to constant value _defaultAttractionProximityRange.
    // It allows to extend constant promixity range value from 200 to a set value. 
    public bool IsWithinAttractionProximity(Attraction attraction, Locations location)
    {
        Console.WriteLine(GetDistance(attraction, location));
        return GetDistance(attraction, location) <= _proximityRange;
    }

    private bool NearAttraction(VisitedLocation visitedLocation, Attraction attraction)
    {
        return GetDistance(attraction, visitedLocation.Location) <= _proximityBuffer;
    }

    private int GetRewardPoints(Attraction attraction, User user)
    {
        return _rewardsCentral.GetAttractionRewardPoints(attraction.AttractionId, user.UserId);
    }

    public double GetDistance(Locations loc1, Locations loc2)
    {
        double lat1 = Math.PI * loc1.Latitude / 180.0;
        double lon1 = Math.PI * loc1.Longitude / 180.0;
        double lat2 = Math.PI * loc2.Latitude / 180.0;
        double lon2 = Math.PI * loc2.Longitude / 180.0;

        double angle = Math.Acos(Math.Sin(lat1) * Math.Sin(lat2)
                                + Math.Cos(lat1) * Math.Cos(lat2) * Math.Cos(lon1 - lon2));

        double nauticalMiles = 60.0 * angle * 180.0 / Math.PI;
        return StatuteMilesPerNauticalMile * nauticalMiles;
    }
}
