using GpsUtil.Helpers;
using GpsUtil.Location;
using GpsUtil.Helpers;

namespace GpsUtil;

public class GpsUtil
{
    private static readonly SemaphoreSlim rateLimiter = new(1000, 1000);

    public VisitedLocation GetUserLocation(Guid userId)
    {
        rateLimiter.Wait();
        try
        {
            Sleep();

            // (FIX09) update longitude and lattitudes calculation for random coordinates creation
            // for increasing nearby attractions detection (longitude betwween
            // -95 and -105°, lattitude between 35 and 45°).
            double longitude = ThreadLocalRandom.NextDouble(-105.0, -95.0);
            longitude = Math.Round(longitude, 6);

            double latitude = ThreadLocalRandom.NextDouble(35, 45);
            latitude = Math.Round(latitude, 6);

            VisitedLocation visitedLocation = new(userId, new Locations(latitude, longitude), DateTime.UtcNow);

            return visitedLocation;
        }
        finally
        {
            rateLimiter.Release();
        }
    }

    public List<Attraction> GetAttractions()
    {
        rateLimiter.Wait();

        try
        {
            SleepLighter();

            List<Attraction> attractions = new()
            // (FIX08) update attractions longitude and lattitude coordinates for increasing
            // nearby attractions detection (longitude betwween
            // -95 and -105°, lattitude between 35 and 45°).
        {
            new Attraction("Disneyland", "Anaheim", "CA", 35.817595, -102.922008),
            new Attraction("Jackson Hole", "Jackson Hole", "WY", 43.582767, -103.821999),
            new Attraction("Mojave National Preserve", "Kelso", "CA", 36.141689, -104.510399),
            new Attraction("Joshua Tree National Park", "Joshua Tree National Park", "CA", 33.881866, -105.90065),
            new Attraction("Buffalo National River", "St Joe", "AR", 35.985512, -99.757652),
            new Attraction("Hot Springs National Park", "Hot Springs", "AR", 36.52153, -95.042267),
            new Attraction("Kartchner Caverns State Park", "Benson", "AZ", 37.837551, -105.347382),
            new Attraction("Legend Valley", "Thornville", "OH", 39.937778, -95.40667),
            new Attraction("Flowers Bakery of London", "Flowers Bakery of London", "KY", 37.131527, -96.07486),
            new Attraction("McKinley Tower", "Anchorage", "AK", 45.218887, -104.877502),
            new Attraction("Flatiron Building", "New York City", "NY", 40.741112, -97.989723),
            new Attraction("Fallingwater", "Mill Run", "PA", 39.906113, -97.468056),
            new Attraction("Union Station", "Washington D.C.", "CA", 38.897095, -98.006332),
            new Attraction("Roger Dean Stadium", "Jupiter", "FL", 38.890959, -98.116577),
            new Attraction("Texas Memorial Stadium", "Austin", "TX", 40.283682, -99.732536),
            new Attraction("Bryant-Denny Stadium", "Tuscaloosa", "AL", 40.208973, -99.550438),
            new Attraction("Tiger Stadium", "Baton Rouge", "LA", 41.412035, -100.183815),
            new Attraction("Neyland Stadium", "Knoxville", "TN", 41.955013, -100.925011),
            new Attraction("Kyle Field", "College Station", "TX", 42.61025, -101.339844),
            new Attraction("San Diego Zoo", "San Diego", "CA", 42.735317, -104.149048),
            new Attraction("Zoo Tampa at Lowry Park", "Tampa", "FL", 43.012804, -101.469269),
            new Attraction("Franklin Park Zoo", "Boston", "MA", 43.302601, -101.086731),
            new Attraction("El Paso Zoo", "El Paso", "TX", 44.769125, -102.44487),
            new Attraction("Kansas City Zoo", "Kansas City", "MO", 44.007504, -102.529625),
            new Attraction("Bronx Zoo", "Bronx", "NY", 45.852905, -103.872971),
            new Attraction("Cinderella Castle", "Orlando", "FL", 45.419411, -103.5812)
        };

            return attractions;
        }
        finally
        {
            rateLimiter.Release();
        }
    }

    private void Sleep()
    {
        int delay = ThreadLocalRandom.Current.Next(30, 100);
        Thread.Sleep(delay);
    }

    private void SleepLighter()
    {
        Thread.Sleep(10);
    }
}
