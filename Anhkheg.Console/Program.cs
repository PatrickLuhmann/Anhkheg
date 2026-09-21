// See https://aka.ms/new-console-template for more information

using Anhkheg.Domain.Models;
using Anhkheg.JSON;
using Microsoft.Extensions.Configuration;


#if WINDOWS
// We are displaying a lot of data so make sure the console window is wide.
Console.SetWindowSize(200, 50);
#else
Console.WriteLine("This is not Windows so we are stuck with your console window size.");
#endif

Console.WriteLine("Welcome to Anhkheg!");
Console.WriteLine($"Current directory: {Environment.CurrentDirectory}");
Console.WriteLine($"Current directory: {Directory.GetCurrentDirectory()}");

AnhkhegData carData = new();

var quit = false;
do
{
	Console.WriteLine();
	Console.Write("$> ");
	var userInput = Console.ReadLine();
	switch (userInput)
	{
		case "quit":
			quit = true;
			break;
		case "help":
			CmdHelp();
			break;
		case "view":
			carData.CmdAllVehiclesView();
			break;
		case "new":
			carData.CmdNewVehicle();
			break;
		case "select":
			carData.CmdSelectVehicle();
			break;
		case "add":
			carData.CmdAdd();
			break;
	}
} while (!quit);

return;

void CmdHelp()
{
	Console.WriteLine("Available Commands");
	Console.WriteLine("==================");
	//Console.WriteLine("add - add a fuel purchase");
	Console.WriteLine("view - view all fuel purchases");
	Console.WriteLine("new - add a new vehicle to the database");
	Console.WriteLine("select - select a vehicle to use");
	Console.WriteLine("quit - quit this program");
}

internal class AnhkhegData
{
	private List<VehicleData> _vehicles;
	private VehicleData? _currentVehicle;

	// The JSON file where we are storing our vehicle data.
	public string Filename { get; set; }

	private readonly AnhkhegService _service;

	public AnhkhegData()
	{
		// TODO: When we are ready to support running in a production environment,
		//       the path to the data store will not be static.
		Filename = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) +
			"/NerdyNeutrino/.development/anhkheg.json";

		_service = new AnhkhegService(Filename);
		_vehicles = _service.GetVehicles();
	}

	public void CmdAllVehiclesView()
	{
#if false
		int idx = 1;
		foreach (var vehicle in vehicles)
		{
			Console.WriteLine($"[{idx}]  {vehicle.Name}   {vehicle.Purchases.Count} purchases");
			idx++;
		}
#else
		for (int idx = 0; idx < _vehicles.Count; idx++)
		{
			// For human consumption, the index range starts at 1, not 0.
			Console.WriteLine($"[{idx + 1}]  {_vehicles[idx].Name}   {_vehicles[idx].Purchases.Count} purchases");
		}
#endif
	}

	public void CmdVehiclePurchasesView()
	{
		if (_currentVehicle is null)
			return;

		Console.WriteLine($"Vehicle: {_currentVehicle.Name}");
		Console.WriteLine();
		Console.WriteLine(
			"       |            |         | Trip   |        |          | Total  | Trip  | Total | Cumulative | Odo   | Miles   | Miles    | Fuel  |");
		Console.WriteLine(
			"ID     | Date       | Gallons | Milage | Cost   | Odometer | Milage | MPG   | MPG   | Gallons    | Diff  | Per Day | Per Week | Price |");
		Console.WriteLine(
			"====== | ========== | ======= | ====== | ====== | ======== | ====== | ===== | ===== | ========== | ===== | ======= | ======== | ===== |");
		foreach (var item in _currentVehicle.Purchases)
		{
			Console.Write(
				$"{item.Id,-6} | {item.Date,-10:yyyy-MM-dd} | {item.Gallons,-7:F3} | {item.TripMilage,-6:F1} | {item.Cost,-6:C2} | {item.Odometer,-8} | ");
			Console.WriteLine(
				$"{item.TotalMilage,-6:F1} | {item.MpgThisTrip,-5:F2} | {item.MpgTotal,-5:F2} | {item.CumulativeGallons,-10:F3} | {item.OdoDiff,-5:F2} | {item.MilesPerDay,-7:F2} | {item.MilesPerWeek,-8:F2} | {item.PriceOfFuel,-5:F2} |");
		}
	}

	public void CmdAdd()
	{
#if false
		// Get the info from the user.
		string? userInput;
		Console.WriteLine("Enter the data for the fuel purchase");
		Console.Write("Date: ");
		userInput = Console.ReadLine();
		DateTime date = Convert.ToDateTime(userInput);
		Console.Write("Gallons: ");
		userInput = Console.ReadLine();
		decimal gallons = Convert.ToDecimal(userInput);
		Console.Write("Trip Milage: ");
		userInput = Console.ReadLine();
		decimal milage = Convert.ToDecimal(userInput);
		Console.Write("Cost: ");
		userInput = Console.ReadLine();
		decimal cost = Convert.ToDecimal(userInput);
		Console.Write("Odometer: ");
		userInput = Console.ReadLine();
		Int32 odometer = Convert.ToInt32(userInput);

		// Create a new data record.
		var rec = new FuelPurchase()
		{
			Id = NextId++,
			Date = date,
			Gallons = gallons,
			TripMilage = milage,
			Cost = cost,
			Odometer = odometer,
			Vehicle = MyCar,
		};
		MyCar.FuelPurchases.Add(rec);

		// Create a new view entry.
		Purchases.Add(new PurchaseData(rec));

		CalculateGlobalProperties();
#endif
	}

	public void CmdSelectVehicle()
	{
		try
		{
			Console.Write("Enter the number of the vehicle to select: ");
			var userInput = Console.ReadLine();
			var number = Convert.ToInt32(userInput);
			// For human consumption, the index range starts at 1, not 0.
			if (number < 1 || number > _vehicles.Count)
			{
				Console.WriteLine($"ERROR: Number out of range. Valid range is 1 - {_vehicles.Count}.");
			}
			else
			{
				_currentVehicle = _vehicles[number - 1];
				CmdVehiclePurchasesView();
			}
		}
		catch
		{
			Console.WriteLine("ERROR: Input must be a number.");
		}
	}

	public void CmdNewVehicle()
	{
		Console.Write("Enter the name of the new vehicle: ");
		var userInput = Console.ReadLine();
		if (userInput is null) return;
		try
		{
			var vData = _service.CreateVehicle(userInput);
			_vehicles = _service.GetVehicles();
			_currentVehicle = vData;
		}
		catch (DuplicateVehicleNameException ex)
		{
			Console.WriteLine("ERROR: Duplicate vehicle names are not allowed.");
			Console.WriteLine($"{ex.Message}");
		}
		catch (Exception ex)
		{
			Console.WriteLine($"{ex.Message}");
		}
	}
}
