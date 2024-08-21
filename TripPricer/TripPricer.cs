using TripPricer.Helpers;

namespace TripPricer;

public class TripPricer
{
    public List<Provider> GetPrice(string apiKey, Guid attractionId, int adults, int children, int nightsStay, int rewardsPoints)
    {
        List<Provider> providers = new List<Provider>();
        HashSet<string> providersUsed = new HashSet<string>();

        // Sleep to simulate some latency
        Thread.Sleep(ThreadLocalRandom.Current.Next(1, 50));
        // FIX01 GetTripDeals unit test ==> validation
        // We extend the For loop high limit to 10 (5 previously).
        // It allows to load all suppliers (10, see GetProviderName).          
        for (int i = 0; i < 10; i++)
        {
            int multiple = ThreadLocalRandom.Current.Next(100, 700);
            double childrenDiscount = children / 3.0;
            double price = multiple * adults + multiple * childrenDiscount * nightsStay + 0.99 - rewardsPoints;

            if (price < 0.0)
            {
                price = 0.0;
            }

            string provider;
            do
            {
                provider = GetProviderName(apiKey, adults);
            } while (providersUsed.Contains(provider));

            providersUsed.Add(provider);
            providers.Add(new Provider(attractionId, provider, price));
        }
        return providers;
    }

    public string GetProviderName(string apiKey, int adults)
    {
        // FIX02 for GetTripDeals unit test validation ==>
        // Next is exclusive, it returns minvalue to maxvalue-1,
        // so 10 is never returned. We set the maxvalue to 11. 
        int multiple = ThreadLocalRandom.Current.Next(1, 11);

        return multiple switch
        {
            1 => "Holiday Travels",
            2 => "Enterprize Ventures Limited",
            3 => "Sunny Days",
            4 => "FlyAway Trips",
            5 => "United Partners Vacations",
            6 => "Dream Trips",
            7 => "Live Free",
            8 => "Dancing Waves Cruselines and Partners",
            9 => "AdventureCo",
            _ => "Cure-Your-Blues", // Note. "_" is equivalent to default.
        };        
    }
}
