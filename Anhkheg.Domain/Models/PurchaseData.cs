namespace Anhkheg.Domain.Models;

public class PurchaseData
{
	// TODO: Is there a better way to associate our object with the base object?
	public int Id { get; set; }
	public DateTime Date { get; set; }
	public decimal Gallons { get; set; }
	public decimal TripMilage { get; set; }
	public decimal Cost { get; set; }
	public Int32 Odometer { get; set; }

	// The following properties are derived.
	public decimal TotalMilage { get; set; }
	public decimal MpgThisTrip { get; set; }
	public decimal MpgTotal { get; set; }
	public decimal CumulativeGallons { get; set; }
	public decimal OdoDiff { get; set; }
	public decimal MilesPerDay { get; set; } // Cumulative from the first purchase
	public decimal MilesPerWeek { get; set; } // Cumulative from the first purchase
	public decimal PriceOfFuel { get; set; }

	public override bool Equals(object? obj)
	{
		if ((obj is null) || (GetType() != obj.GetType())) return false;
		
		PurchaseData other = (PurchaseData)obj;

		return (Id == other.Id) &&
			(this.Date == other.Date) &&
			(this.Gallons == other.Gallons) &&
			(this.TripMilage == other.TripMilage) &&
			(this.Cost == other.Cost) &&
			(this.Odometer == other.Odometer) &&
			(this.TotalMilage == other.TotalMilage) &&
			(this.MpgThisTrip == other.MpgThisTrip) &&
			(this.MpgTotal == other.MpgTotal) &&
			(this.CumulativeGallons == other.CumulativeGallons) &&
			(this.OdoDiff == other.OdoDiff) &&
			(this.MilesPerDay == other.MilesPerDay) &&
			(this.MilesPerWeek == other.MilesPerWeek) &&
			(this.PriceOfFuel == other.PriceOfFuel);
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(
			HashCode.Combine(Id, Date, Gallons, TripMilage, Cost, Odometer),
			HashCode.Combine(TotalMilage, MpgThisTrip, MpgTotal, CumulativeGallons),
			HashCode.Combine(OdoDiff, MilesPerDay, MilesPerWeek, PriceOfFuel));
	}

#if false
	public PurchaseData(FuelPurchase purchase)
	{
		// Just copy the base data.
		this.Id = purchase.Id;
		this.Date = purchase.Date;
		this.Gallons = purchase.Gallons;
		this.TripMilage = purchase.TripMilage;
		this.Cost = purchase.Cost;
		this.Odometer = purchase.Odometer;

		// Calculate the values that are only for this one purchase.
		this.MpgThisTrip = this.TripMilage / this.Gallons;
		this.PriceOfFuel = this.Cost / this.Gallons;

		// The rest of the properties are global, so an external entity must set them.
	}
#endif
}
