// See https://aka.ms/new-console-template for more information

using Anhkheg.Domain.Models;
using Anhkheg.JSON;
using Anhkheg.JSON.Entities;
using Anhkheg;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Configuration;


// We are displaying a lot of data so make sure the console window is wide.
Console.SetWindowSize(200, 50);

Console.WriteLine("Welcome to Anhkheg!");

AnhkhegData carData = new();

bool quit = false;
do
{
	Console.WriteLine();
	Console.Write("$> ");
	string? userInput = Console.ReadLine();
	if (userInput == "quit")
		quit = true;
	if (userInput == "help")
		CmdHelp();
	if (userInput == "view")
		carData.CmdAllVehiclesView();
	if (userInput == "new")
		carData.CmdNewVehicle();
	if (userInput == "select")
		carData.CmdSelectVehicle();
	if (userInput == "add")
		carData.CmdAdd();
} while (!quit);

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

class AnhkhegData
{
	private List<VehicleData> vehicles;
	private VehicleData? currentVehicle;

	// The JSON file where we are storing our vehicle data.
	public string Filename { get; set; }

	private readonly AnhkhegService service;

	public AnhkhegData()
	{
		// Get our filename from the configuration and load our data.
		var Config = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
		var connStr = Config.GetConnectionString("json") ?? throw new Exception("JSON filename is not present in configuration data.");
		Filename = connStr;

		service = new AnhkhegService(Filename);
		vehicles = service.GetVehicles();
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
		for (int idx =  0; idx < vehicles.Count; idx++)
		{
			// For human consumption, the index range starts at 1, not 0.
			Console.WriteLine($"[{idx+1}]  {vehicles[idx].Name}   {vehicles[idx].Purchases.Count} purchases");
		}
#endif
		return;
	}

	public void CmdVehiclePurchasesView()
	{
		if (currentVehicle is null)
			return;

		Console.WriteLine($"Vehicle: {currentVehicle.Name}");
		Console.WriteLine();
		Console.WriteLine("       |            |         | Trip   |        |          | Total  | Trip  | Total | Cumulative | Odo   | Miles   | Miles    | Fuel  |");
		Console.WriteLine("ID     | Date       | Gallons | Milage | Cost   | Odometer | Milage | MPG   | MPG   | Gallons    | Diff  | Per Day | Per Week | Price |");
		Console.WriteLine("====== | ========== | ======= | ====== | ====== | ======== | ====== | ===== | ===== | ========== | ===== | ======= | ======== | ===== |");
		foreach (var item in currentVehicle.Purchases)
		{
			Console.Write($"{item.Id,-6} | {item.Date,-10:yyyy-MM-dd} | {item.Gallons,-7:F3} | {item.TripMilage,-6:F1} | {item.Cost,-6:C2} | {item.Odometer,-8} | ");
			Console.WriteLine($"{item.TotalMilage,-6:F1} | {item.MpgThisTrip,-5:F2} | {item.MpgTotal,-5:F2} | {item.CumulativeGallons,-10:F3} | {item.OdoDiff,-5:F2} | {item.MilesPerDay,-7:F2} | {item.MilesPerWeek,-8:F2} | {item.PriceOfFuel,-5:F2} |");
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
			string? userInput;
			Console.Write("Enter the number of the vehicle to select: ");
			userInput = Console.ReadLine();
			Int32 number = Convert.ToInt32(userInput);
			// For human consumption, the index range starts at 1, not 0.
			if (number < 1 || number > vehicles.Count)
			{
				Console.WriteLine($"ERROR: Number out of range. Valid range is 1 - {vehicles.Count}.");
			}
			else
			{
				currentVehicle = vehicles[number-1];
				CmdVehiclePurchasesView();
			}
		}
		catch (Exception ex)
		{
			Console.WriteLine("ERROR: Input must be a number.");
		}
	}

	public void CmdNewVehicle()
	{
		string? userInput;
		Console.Write("Enter the name of the new vehicle: ");
		userInput = Console.ReadLine();
		try
		{
			var vData = service.CreateVehicle(userInput);
			vehicles = service.GetVehicles();
			currentVehicle = vData;
		}
		catch (DuplicateVehicleNameException ex)
		{
			Console.WriteLine($"ERROR: Duplicate vehicle names are not allowed.");
			Console.WriteLine($"{ex.Message}");
		}
		catch (Exception ex)
		{
			Console.WriteLine($"{ex.Message}");
		}
	}
}
