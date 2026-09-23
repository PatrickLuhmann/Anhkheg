// See https://aka.ms/new-console-template for more information

using Anhkheg.Domain.Models;
using Anhkheg.JSON;

internal class Program
{
	private static void Main(string[] args)
	{
		Console.WriteLine("Welcome to Anhkheg!");
		Console.WriteLine($"Current directory: {Environment.CurrentDirectory}");
		Console.WriteLine($"Current directory: {Directory.GetCurrentDirectory()}");
		Console.WriteLine($"Window Height: {Console.WindowHeight}  Window Width: {Console.WindowWidth}");
		if (OperatingSystem.IsWindows())
			Console.SetWindowSize(140, 50);
		else
		{
			Console.WriteLine("Cannot change window size on this operating system.");
			if (Console.WindowWidth < 140)
				Console.WriteLine("  WARNING: This program uses wide output. Setting your window width to 140 or more will provide a better experience.");
		}

		var app = new AnhkhegData();
		app.MainMenu();
	}
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

	public void MainMenu()
	{
		var quit = false;
		do
		{
			Console.WriteLine();
			Console.WriteLine("Vehicles In The Database");
			Console.WriteLine("========================");
			CmdAllVehiclesView();

			Console.WriteLine();
			Console.WriteLine("Commands: new, select, delete, quit");
			Console.Write("$> ");
			var userInput = Console.ReadLine();
			switch (userInput)
			{
				case "new":
					var vehicle = CmdNewVehicle();
					if (vehicle is not null)
						VehicleMenu(vehicle);
					break;
				case "select":
					CmdSelectVehicle();
					break;
				case "delete":
					CmdDeleteVehicle();
					break;
				case "quit":
					quit = true;
					break;
				case "help":
					MainMenuHelp();
					break;
			}
		} while (!quit);
	}

	void MainMenuHelp()
	{
		Console.WriteLine();
		Console.WriteLine("Available Commands");
		Console.WriteLine("==================");
		Console.WriteLine("new - add a new vehicle to the database");
		Console.WriteLine("select - select a vehicle to use");
		Console.WriteLine("delete - delete a vehicle from the database");
		Console.WriteLine("quit - quit this program");
	}

	void VehicleMenu(VehicleData vehicle)
	{
		var back = false;
		do
		{
			Console.WriteLine();
			Console.WriteLine($"Vehicle: {vehicle.Name}");
			Console.WriteLine($"Num Fuel Purchases: {vehicle.Purchases.Count}");


			Console.WriteLine();
			Console.WriteLine("Commands: add, edit, delete, back");
			Console.Write("$> ");
			var userInput = Console.ReadLine();
			switch (userInput)
			{
				case "add":
					break;
				case "edit":
					break;
				case "delete":
					break;
				case "back":
					back = true;
					break;
				case "help":
					VehicleMenuHelp();
					break;
			}
		} while (!back);
	}

	void VehicleMenuHelp()
	{
		Console.WriteLine("Available Commands");
		Console.WriteLine("==================");
		Console.WriteLine("add - add a new fuel purchase for this vehicle");
		Console.WriteLine("edit - edit an existing fuel purchase");
		Console.WriteLine("delete - delete a fuel purchase from this vehicle");
		Console.WriteLine("back - return to the main menu");
	}

	void CmdAllVehiclesView()
	{
		for (int idx = 0; idx < _vehicles.Count; idx++)
		{
			// For human consumption, the index range starts at 1, not 0.
			Console.WriteLine($"[{idx + 1}]  {_vehicles[idx].Name}   {_vehicles[idx].Purchases.Count} purchases");
		}
	}

	void CmdVehiclePurchasesView()
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

	void CmdAdd()
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

	void CmdSelectVehicle()
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

	void CmdDeleteVehicle()
	{
		try
		{
			Console.Write("Enter the number of the vehicle to delete: ");
			var userInput = Console.ReadLine();
			var number = Convert.ToInt32(userInput);
			// For human consumption, the index range starts at 1, not 0.
			if (number < 1 || number > _vehicles.Count)
			{
				Console.WriteLine();
				Console.WriteLine($"ERROR: Number out of range. Valid range is 1 - {_vehicles.Count}.");
			}
			else
			{
				_currentVehicle = _vehicles[number - 1];
				Console.WriteLine();
				Console.WriteLine("You are about to delete thsi vehicle:");
				Console.WriteLine($"Name: {_currentVehicle.Name}");
				Console.WriteLine($"Num Fuel Purchases: {_currentVehicle.Purchases.Count}");
				Console.WriteLine();
				Console.WriteLine("Delete this vehicle? [y/n]");
				userInput = Console.ReadLine();
				if (userInput?.ToLower() == "y")
				{
					_service.DeleteVehicle(_currentVehicle.Name);
					_currentVehicle = null;
					// NOTE: This will change the index numbers if the deleted vehicle wasn't the last one.
					_vehicles = _service.GetVehicles();
				}
			}
		}
		catch
		{
			Console.WriteLine("ERROR: Input must be a number.");
		}
	}

	VehicleData? CmdNewVehicle()
	{
		Console.WriteLine();
		Console.Write("Enter the name of the new vehicle: ");
		var userInput = Console.ReadLine();
		if (userInput is null) return null;
		VehicleData? vehicle = null;
		try
		{
			//var vData = _service.CreateVehicle(userInput);
			vehicle = _service.CreateVehicle(userInput);
			_vehicles = _service.GetVehicles();
			_currentVehicle = vehicle;
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
		return vehicle;
	}
}
