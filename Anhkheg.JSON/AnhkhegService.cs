using Anhkheg.Domain.Models;
using Anhkheg.JSON.Entities;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Xml.Linq;

namespace Anhkheg.JSON;

#region Custom Exceptions
[Serializable]
public class DuplicateOdometerValueException : Exception
{
	public DuplicateOdometerValueException() { }
	public DuplicateOdometerValueException(string? message) : base(message) { }
	public DuplicateOdometerValueException(string? message, Exception? innerException) : base(message, innerException) { }
}

[Serializable]
public class DuplicateVehicleNameException : Exception
{
	public DuplicateVehicleNameException() { }
	public DuplicateVehicleNameException(string? message) : base(message) { }
	public DuplicateVehicleNameException(string? message, Exception? innerException) : base(message, innerException) { }
}

[Serializable]
public class VehicleNotFoundException : Exception
{
	public VehicleNotFoundException() { }
	public VehicleNotFoundException(string? message) : base(message) { }
	public VehicleNotFoundException(string? message, Exception? innerException) : base(message, innerException) { }
}

[Serializable]
public class InconsistentDateException : Exception
{
	public InconsistentDateException() { }
	public InconsistentDateException(string? message) : base(message) { }
	public InconsistentDateException(string? message, Exception? innerException) : base(message, innerException) { }
}
#endregion

public class AnhkhegService
{
	private string connectionString;
	private List<Vehicle> vehicles;
	private int highestPurchaseId = 0;

	public List<VehicleData> GetVehicles()
	{
		List<VehicleData> allVehicles = new();

		if (vehicles is not null)
		{
			foreach (Vehicle v in vehicles)
			{
				VehicleData vData = v.ToModel();
				vData.CalculateGlobalProperties();
				allVehicles.Add(vData);
			}
		}
		return allVehicles;
	}

	public VehicleData? GetVehicleByName(string name)
	{
		Vehicle? v = vehicles.Find(v => v.Name == name);
		if (v == null)
			return null;
		VehicleData vData = v.ToModel();
		vData.CalculateGlobalProperties();
		return vData;
	}

	public VehicleData CreateVehicle(string name)
	{
		// Vehicle names must be unique.
		if (vehicles.Exists(v => v.Name == name))
			throw new DuplicateVehicleNameException();

		Vehicle vehicle = new() { Name = name };

		vehicles.Add(vehicle);

		WriteDataToFile();

		return vehicle.ToModel();
	}

	public void DeleteVehicle(string name)
	{
		Vehicle? vehicle = vehicles.Find(v => v.Name == name) ?? throw new VehicleNotFoundException();
		vehicles.Remove(vehicle);
		WriteDataToFile();
	}

	public void CreatePurchase(VehicleData vData, DateTime date, decimal gallons, decimal tripMilage, decimal cost, int odometer)
	{
		Vehicle? vehicle = vehicles.Find(v => v.Name == vData.Name) ?? throw new VehicleNotFoundException();

		// Identical odometer values are not allowed.
		if (vData.Purchases.Exists(p => p.Odometer == odometer))
				throw new DuplicateOdometerValueException();

		// The date must be consistent with the odometer.
		// TODO: Can we assume that the list is already sorted?
		if (vData.Purchases.Count > 0)
		{
			// New odometer is before but date is after.
			PurchaseData? pur = vData.Purchases.Find(p => p.Odometer > odometer);
			if ((pur is not null) && (date > pur.Date))
				throw new InconsistentDateException();

			// New date is before but odometer is after.
			pur = vData.Purchases.Find(p => p.Date > date);
			if ((pur is not null) && (odometer > pur.Odometer))
				throw new InconsistentDateException();
		}

		// Create the new entity and update the store.
		FuelPurchase fuelPurchase = new()
		{
			Id = ++highestPurchaseId,
			Date = date,
			Gallons = gallons,
			TripMilage = tripMilage,
			Cost = cost,
			Odometer = odometer,
			VehicleId = vehicle.Id,
			Vehicle = vehicle,
		};
		vehicle.FuelPurchases.Add(fuelPurchase);
		WriteDataToFile();

		// Add the new purchase to the vehicle.
		vData.Purchases.Add(fuelPurchase.ToModel());
		vData.CalculateGlobalProperties();
	}

	private void WriteDataToFile()
	{
		var options = new JsonSerializerOptions { WriteIndented = true, ReferenceHandler = ReferenceHandler.Preserve };
		string jsonString = JsonSerializer.Serialize(vehicles, options);
		File.WriteAllText(connectionString, jsonString);
	}

	public AnhkhegService(string connStr)
	{
		connectionString = connStr;

		if (File.Exists(connectionString))
		{
			// Grab the vehicle purchase data once at the beginning.
			// We will not be reading the data every time the user
			// does a query.
			try
			{
				JsonSerializerOptions options = new()
				{
					WriteIndented = true,
					ReferenceHandler = ReferenceHandler.Preserve
				};
				string desString = File.ReadAllText(connectionString);
				var data = JsonSerializer.Deserialize<List<Vehicle>>(desString, options) ?? throw new Exception("Data file is not formatted correctly.");
				vehicles = data;
			}
			catch (Exception e)
			{
				Console.WriteLine(e.ToString());
				throw;
			}

			// Go through all purchases to find the highest ID. We need this when
			// creating new FuelPurchase entities.
			foreach (var vehicle in vehicles)
			{
				foreach (var purchase in vehicle.FuelPurchases)
				{
					if (purchase.Id > highestPurchaseId) highestPurchaseId = purchase.Id;
				}
			}
		}
		else
			vehicles = new List<Vehicle>();
	}
}
