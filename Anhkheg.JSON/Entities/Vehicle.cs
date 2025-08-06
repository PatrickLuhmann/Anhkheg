using Anhkheg.Domain.Models;

namespace Anhkheg.JSON.Entities;

public class Vehicle
{
	public int Id {  get; set; }
	/// <summary>
	/// The name of the vehicle.
	/// This can be a nickname or the year/make/model.
	/// </summary>
	public string Name { get; set; } = "My Car";
	
	// Relationships

	public ICollection<FuelPurchase> FuelPurchases { get; set; } = new List<FuelPurchase>();

	public VehicleData ToModel()
	{
		VehicleData vData = new(Name);
		foreach (var item in FuelPurchases)
		{
			vData.Purchases.Add(item.ToModel());
		}

		return vData;
	}
}
