using Anhkheg.Domain.Models;

namespace Anhkheg.JSON.Entities;

public class FuelPurchase
{
	public int Id {  get; set; }
	public DateTime Date { get; set; }
	public decimal Gallons { get; set; }
	public decimal TripMilage { get; set; }
	public decimal Cost {  get; set; }
	public Int32 Odometer { get; set; }

	// Relationships

	public int VehicleId {  get; set; }
	public Vehicle Vehicle { get; set; }

	public PurchaseData ToModel()
	{
		// Since we only know about ourself, we cannot
		// fill in the global properties; the caller
		// is responsible for that.
		return new PurchaseData
		{
			Id = Id,
			Date = Date,
			Gallons = Gallons,
			TripMilage = TripMilage,
			Cost = Cost,
			Odometer = Odometer,
		};
	}
}
