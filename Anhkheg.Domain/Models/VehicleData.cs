namespace Anhkheg.Domain.Models;

public class VehicleData
{
	public string Name { get; set; }
	public List<PurchaseData> Purchases { get; set; } = new();

	public void CalculateGlobalProperties()
	{
		// Sort the list by odometer so that the purchases are in the correct order.
		Purchases.Sort((a, b) => (a.Odometer.CompareTo(b.Odometer)));

		decimal totalMilage = 0;
		decimal cumulativeGallons = 0;
		foreach (var item in Purchases)
		{
			totalMilage += item.TripMilage;
			item.TotalMilage = totalMilage;

			item.MpgThisTrip = Math.Round(item.TripMilage / item.Gallons, 2);

			cumulativeGallons += item.Gallons;
			item.CumulativeGallons = Math.Round(cumulativeGallons, 3);

			item.MpgTotal = Math.Round(item.TotalMilage / item.CumulativeGallons, 2);

			item.OdoDiff = Math.Round(item.Odometer - item.TotalMilage, 2);

			// Special case: the purchase(s) on the first day would result in a divide by zero exception.
			var numDays = (int)(item.Date - Purchases[0].Date).TotalDays;
			if (numDays == 0)
				numDays = 1;
			item.MilesPerDay = Math.Round(item.TotalMilage / numDays, 2);
			item.MilesPerWeek = Math.Round(item.MilesPerDay * 7, 2);

			item.PriceOfFuel = Math.Round(item.Cost / item.Gallons, 2);
		}
	}

	public VehicleData(string name)
	{
		Name = name;
	}

	public override bool Equals(object? obj)
	{
		if ((obj is null) || (GetType() != obj.GetType())) return false;

		VehicleData other = (VehicleData)obj;

		return (this.Name == other.Name) && (this.Purchases.SequenceEqual(other.Purchases));
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(Name, Purchases);
	}
}
