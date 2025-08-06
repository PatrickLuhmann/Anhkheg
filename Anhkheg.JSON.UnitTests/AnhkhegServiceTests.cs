using Anhkheg.Domain.Models;
using System;
using System.Reflection;
using System.Xml.Linq;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Anhkheg.JSON.UnitTests;

public class AnhkhegServiceTests
{
	public class CustomBeforeAfterAttribute : BeforeAfterTestAttribute
	{
		// In order to support running tests in parallel, copy the base
		// JSON file to a unique file for each test (and delete it when
		// the test completes). This requires each test to do the following:
		// - Have a unique name across all tests, not just within the test
		//   class. To do this I am prefixing each test name with the name
		//   of the test class.
		// - Have the [CustomBeforeAfter] attribute on each test.
		// - Pass the name of the unique file to the service constructor.
		public override void Before(MethodInfo methodUnderTest)
		{
			// TODO: Do I need this?
			base.Before(methodUnderTest);

			File.Copy("test_data.json", methodUnderTest.Name + ".json");
		}

		public override void After(MethodInfo methodUnderTest)
		{
			// TODO: Do I need this?
			base.After(methodUnderTest);

			File.Delete(methodUnderTest.Name + ".json");
		}
	}

	public class TestBase
	{
		public string? Filename { get; set; }
		public readonly ITestOutputHelper OutputHelper;

		public TestBase(ITestOutputHelper tout)
		{
			OutputHelper = tout;
		}
	}

	public class CreateVehicle : TestBase
	{
		public CreateVehicle(ITestOutputHelper tout) : base(tout) { }

		[Fact]
		[CustomBeforeAfter]
		public void CreateVehicle_Normal()
		{
			Filename = System.Reflection.MethodBase.GetCurrentMethod()!.Name + ".json";

			// ASSEMBLE
			AnhkhegService srv = new(Filename);

			// ACT
			string name = "New Vehicle Name";
			VehicleData vData = srv.CreateVehicle(name);

			// ASSERT
			Assert.Equal(name, vData.Name);
			Assert.Empty(vData.Purchases);

			List<VehicleData> vehicles = srv.GetVehicles();
			Assert.NotEmpty(vehicles);
			VehicleData? actVehicle = vehicles.Find(v => v.Name == name);
			Assert.NotNull(actVehicle);
			Assert.Equal(name, actVehicle.Name);
			Assert.Empty(actVehicle.Purchases);
		}

		[Fact]
		[CustomBeforeAfter]
		public void CreateVehicle_DuplicateName()
		{
			Filename = System.Reflection.MethodBase.GetCurrentMethod()!.Name + ".json";

			// ASSEMBLE
			string name = "1998 Pontiac Firebird Formula";
			AnhkhegService srv = new(Filename);

			// ACT + ASSERT
			Exception ex = Assert.Throws<DuplicateVehicleNameException>(() => srv.CreateVehicle(name));
			OutputHelper.WriteLine($"Exception message: {ex.Message}");
		}
	}

	public class GetVehicles : TestBase
	{
		public GetVehicles(ITestOutputHelper tout) : base(tout) { }

		[Fact]
		[CustomBeforeAfter]
		public void GetVehicles_Normal()
		{
			Filename = System.Reflection.MethodBase.GetCurrentMethod()!.Name + ".json";

			// ASSEMBLE
			AnhkhegService srv = new(Filename);

			// ACT
			List<VehicleData> vData = srv.GetVehicles();

			// ASSERT
			Assert.NotNull(vData);
			Assert.Equal(2, vData.Count);

			VehicleData? vehicle = vData.Find(v => v.Name == "2008 Honda Fit");
			Assert.NotNull(vehicle);
			Assert.Equal(2, vehicle.Purchases.Count);

			PurchaseData purchase = vehicle.Purchases[0];
			Assert.Equal(1, purchase.Id);
			Assert.Equal(new DateTime(2009, 1, 1), purchase.Date);
			Assert.Equal(8.76m, purchase.Gallons);
			Assert.Equal(321.09m, purchase.TripMilage);
			Assert.Equal(1.23m, purchase.Cost);
			Assert.Equal(9876, purchase.Odometer);
			Assert.Equal(321.09m, purchase.TotalMilage);
			Assert.Equal(36.65m, purchase.MpgThisTrip);
			Assert.Equal(36.65m, purchase.MpgTotal);
			Assert.Equal(8.76m, purchase.CumulativeGallons);
			Assert.Equal(9554.91m, purchase.OdoDiff);
			Assert.Equal(321.09m, purchase.MilesPerDay);
			Assert.Equal(2247.63m, purchase.MilesPerWeek);
			Assert.Equal(0.14m, purchase.PriceOfFuel);

			purchase = vehicle.Purchases[1];
			Assert.Equal(2, purchase.Id);
			Assert.Equal(new DateTime(2010, 1, 1), purchase.Date);
			Assert.Equal(7.62m, purchase.Gallons);
			Assert.Equal(299.99m, purchase.TripMilage);
			Assert.Equal(4.56m, purchase.Cost);
			Assert.Equal(13690, purchase.Odometer);
			Assert.Equal(621.08m, purchase.TotalMilage);
			Assert.Equal(39.37m, purchase.MpgThisTrip);
			Assert.Equal(37.92m, purchase.MpgTotal);
			Assert.Equal(16.38m, purchase.CumulativeGallons);
			Assert.Equal(13068.92m, purchase.OdoDiff);
			Assert.Equal(1.70m, purchase.MilesPerDay);
			Assert.Equal(11.90m, purchase.MilesPerWeek);
			Assert.Equal(0.60m, purchase.PriceOfFuel);

			vehicle = vData.Find(v => v.Name == "1998 Pontiac Firebird Formula");
			Assert.NotNull(vehicle);
			Assert.Equal(2, vehicle.Purchases.Count);

			purchase = vehicle.Purchases[0];
			Assert.Equal(3, purchase.Id);
			Assert.Equal(new DateTime(1999, 1, 1), purchase.Date);
			Assert.Equal(13.96m, purchase.Gallons);
			Assert.Equal(296.6m, purchase.TripMilage);
			Assert.Equal(28.64m, purchase.Cost);
			Assert.Equal(300, purchase.Odometer);
			Assert.Equal(296.6m, purchase.TotalMilage);
			Assert.Equal(21.25m, purchase.MpgThisTrip);
			Assert.Equal(21.25m, purchase.MpgTotal);
			Assert.Equal(13.96m, purchase.CumulativeGallons);
			Assert.Equal(3.4m, purchase.OdoDiff);
			Assert.Equal(296.6m, purchase.MilesPerDay);
			Assert.Equal(2076.2m, purchase.MilesPerWeek);
			Assert.Equal(2.05m, purchase.PriceOfFuel);

			purchase = vehicle.Purchases[1];
			Assert.Equal(4, purchase.Id);
			Assert.Equal(new DateTime(1999, 1, 23), purchase.Date);
			Assert.Equal(12.98m, purchase.Gallons);
			Assert.Equal(301.6m, purchase.TripMilage);
			Assert.Equal(33.33m, purchase.Cost);
			Assert.Equal(602, purchase.Odometer);
			Assert.Equal(598.2m, purchase.TotalMilage);
			Assert.Equal(23.24m, purchase.MpgThisTrip);
			Assert.Equal(22.20m, purchase.MpgTotal);
			Assert.Equal(26.94m, purchase.CumulativeGallons);
			Assert.Equal(3.8m, purchase.OdoDiff);
			Assert.Equal(27.19m, purchase.MilesPerDay);
			Assert.Equal(190.33m, purchase.MilesPerWeek);
			Assert.Equal(2.57m, purchase.PriceOfFuel);
		}
	}

	public class GetVehicleByName : TestBase
	{
		public GetVehicleByName(ITestOutputHelper tout) : base(tout) { }

		[Fact]
		[CustomBeforeAfter]
		public void GetVehicleByName_Present()
		{
			Filename = System.Reflection.MethodBase.GetCurrentMethod()!.Name + ".json";

			// ASSEMBLE
			AnhkhegService srv = new(Filename);

			// ACT
			string name = "1998 Pontiac Firebird Formula";
			VehicleData? vehicle = srv.GetVehicleByName(name);

			// ASSERT
			Assert.NotNull(vehicle);
			Assert.Equal(name, vehicle.Name);
			Assert.Equal(2, vehicle.Purchases.Count);

			var purchase = vehicle.Purchases[0];
			Assert.Equal(3, purchase.Id);
			Assert.Equal(new DateTime(1999, 1, 1), purchase.Date);
			Assert.Equal(13.96m, purchase.Gallons);
			Assert.Equal(296.6m, purchase.TripMilage);
			Assert.Equal(28.64m, purchase.Cost);
			Assert.Equal(300, purchase.Odometer);
			Assert.Equal(296.6m, purchase.TotalMilage);
			Assert.Equal(21.25m, purchase.MpgThisTrip);
			Assert.Equal(21.25m, purchase.MpgTotal);
			Assert.Equal(13.96m, purchase.CumulativeGallons);
			Assert.Equal(3.4m, purchase.OdoDiff);
			Assert.Equal(296.6m, purchase.MilesPerDay);
			Assert.Equal(2076.2m, purchase.MilesPerWeek);
			Assert.Equal(2.05m, purchase.PriceOfFuel);

			purchase = vehicle.Purchases[1];
			Assert.Equal(4, purchase.Id);
			Assert.Equal(new DateTime(1999, 1, 23), purchase.Date);
			Assert.Equal(12.98m, purchase.Gallons);
			Assert.Equal(301.6m, purchase.TripMilage);
			Assert.Equal(33.33m, purchase.Cost);
			Assert.Equal(602, purchase.Odometer);
			Assert.Equal(598.2m, purchase.TotalMilage);
			Assert.Equal(23.24m, purchase.MpgThisTrip);
			Assert.Equal(22.20m, purchase.MpgTotal);
			Assert.Equal(26.94m, purchase.CumulativeGallons);
			Assert.Equal(3.8m, purchase.OdoDiff);
			Assert.Equal(27.19m, purchase.MilesPerDay);
			Assert.Equal(190.33m, purchase.MilesPerWeek);
			Assert.Equal(2.57m, purchase.PriceOfFuel);
		}

		[Fact]
		[CustomBeforeAfter]
		public void GetVehicleByName_NotPresent()
		{
			Filename = System.Reflection.MethodBase.GetCurrentMethod()!.Name + ".json";

			// ASSEMBLE
			AnhkhegService srv = new(Filename);

			// ACT
			VehicleData? vehicle = srv.GetVehicleByName("2026 Lucid Air Grand Turing");

			// ASSERT
			Assert.Null(vehicle);
		}
	}

	public class DeleteVehicle : TestBase
	{
		public DeleteVehicle(ITestOutputHelper tout) : base(tout) { }

		[Fact]
		[CustomBeforeAfter]
		public void DeleteVehicle_NotTheLastOne()
		{
			Filename = System.Reflection.MethodBase.GetCurrentMethod()!.Name + ".json";

			// ASSEMBLE
			AnhkhegService srv = new(Filename);

			// ACT
			string name = "1998 Pontiac Firebird Formula";
			srv.DeleteVehicle(name);

			// ASSERT
			List<VehicleData> vData = srv.GetVehicles();
			Assert.NotEmpty(vData);
			Assert.Single(vData);

			VehicleData? vehicle = vData.Find(v => v.Name == "2008 Honda Fit");
			Assert.NotNull(vehicle);
			Assert.Equal(2, vehicle.Purchases.Count);

			PurchaseData purchase = vehicle.Purchases[0];
			Assert.Equal(1, purchase.Id);
			Assert.Equal(new DateTime(2009, 1, 1), purchase.Date);
			Assert.Equal(8.76m, purchase.Gallons);
			Assert.Equal(321.09m, purchase.TripMilage);
			Assert.Equal(1.23m, purchase.Cost);
			Assert.Equal(9876, purchase.Odometer);
			Assert.Equal(321.09m, purchase.TotalMilage);
			Assert.Equal(36.65m, purchase.MpgThisTrip);
			Assert.Equal(36.65m, purchase.MpgTotal);
			Assert.Equal(8.76m, purchase.CumulativeGallons);
			Assert.Equal(9554.91m, purchase.OdoDiff);
			Assert.Equal(321.09m, purchase.MilesPerDay);
			Assert.Equal(2247.63m, purchase.MilesPerWeek);
			Assert.Equal(0.14m, purchase.PriceOfFuel);

			purchase = vehicle.Purchases[1];
			Assert.Equal(2, purchase.Id);
			Assert.Equal(new DateTime(2010, 1, 1), purchase.Date);
			Assert.Equal(7.62m, purchase.Gallons);
			Assert.Equal(299.99m, purchase.TripMilage);
			Assert.Equal(4.56m, purchase.Cost);
			Assert.Equal(13690, purchase.Odometer);
			Assert.Equal(621.08m, purchase.TotalMilage);
			Assert.Equal(39.37m, purchase.MpgThisTrip);
			Assert.Equal(37.92m, purchase.MpgTotal);
			Assert.Equal(16.38m, purchase.CumulativeGallons);
			Assert.Equal(13068.92m, purchase.OdoDiff);
			Assert.Equal(1.70m, purchase.MilesPerDay);
			Assert.Equal(11.90m, purchase.MilesPerWeek);
			Assert.Equal(0.60m, purchase.PriceOfFuel);
		}

		[Fact]
		[CustomBeforeAfter]
		public void DeleteVehicle_TheLastOne()
		{
			Filename = System.Reflection.MethodBase.GetCurrentMethod()!.Name + ".json";

			// ASSEMBLE
			AnhkhegService srv = new(Filename);

			// ACT
			srv.DeleteVehicle("1998 Pontiac Firebird Formula");
			srv.DeleteVehicle("2008 Honda Fit");

			// ASSERT
			List<VehicleData> vData = srv.GetVehicles();
			Assert.Empty(vData);
		}

		[Fact]
		[CustomBeforeAfter]
		public void DeleteVehicle_NotPresent()
		{
			Filename = System.Reflection.MethodBase.GetCurrentMethod()!.Name + ".json";

			// ASSEMBLE
			AnhkhegService srv = new(Filename);

			// ACT + ASSERT
			Exception ex = Assert.Throws<VehicleNotFoundException>(
				() => srv.DeleteVehicle("1963 Volkswagen Beetle"));
		}
	}

	public class CreatePurchase : TestBase
	{
		public CreatePurchase(ITestOutputHelper tout) : base(tout) { }

		[Fact]
		[CustomBeforeAfter]
		public void CreatePurchase_InTheMiddle()
		{
			Filename = System.Reflection.MethodBase.GetCurrentMethod()!.Name + ".json";

			// ASSEMBLE
			AnhkhegService srv = new(Filename);
			var vehicles = srv.GetVehicles();
			VehicleData vehicle = vehicles.Find(v => v.Name == "2008 Honda Fit")!;
			DateTime date = new DateTime(2009, 9, 4);
			decimal gallons = 7.89m;
			decimal tripMilage = 303.5m;
			decimal cost = 17.12m;
			int odometer = 10001;

			// ACT
			srv.CreatePurchase(vehicle, date, gallons, tripMilage, cost, odometer);

			// ASSERT
			Assert.Equal(3, vehicle.Purchases.Count);

			PurchaseData actPurchase;

			// Verify that the first purchase didn't change.
			actPurchase = vehicle.Purchases[0];
			Assert.Equal(new DateTime(2009, 1, 1), actPurchase.Date);
			Assert.Equal(8.76m, actPurchase.Gallons);
			Assert.Equal(321.09m, actPurchase.TripMilage);
			Assert.Equal(1.23m, actPurchase.Cost);
			Assert.Equal(9876, actPurchase.Odometer);
			Assert.Equal(321.09m, actPurchase.TotalMilage);
			Assert.Equal(36.65m, actPurchase.MpgThisTrip);
			Assert.Equal(36.65m, actPurchase.MpgTotal);
			Assert.Equal(8.76m, actPurchase.CumulativeGallons);
			Assert.Equal(9554.91m, actPurchase.OdoDiff);
			Assert.Equal(321.09m, actPurchase.MilesPerDay);
			Assert.Equal(2247.63m, actPurchase.MilesPerWeek);
			Assert.Equal(0.14m, actPurchase.PriceOfFuel);

			// Verify the second purchase.
			actPurchase = vehicle.Purchases[1];
			Assert.Equal(date, actPurchase.Date);
			Assert.Equal(gallons, actPurchase.Gallons);
			Assert.Equal(tripMilage, actPurchase.TripMilage);
			Assert.Equal(cost, actPurchase.Cost);
			Assert.Equal(odometer, actPurchase.Odometer);
			Assert.Equal(624.59m, actPurchase.TotalMilage);
			Assert.Equal(38.47m, actPurchase.MpgThisTrip);
			Assert.Equal(37.51m, actPurchase.MpgTotal);
			Assert.Equal(16.65m, actPurchase.CumulativeGallons);
			Assert.Equal(9376.41m, actPurchase.OdoDiff);
			Assert.Equal(2.54m, actPurchase.MilesPerDay);
			Assert.Equal(17.78m, actPurchase.MilesPerWeek);
			Assert.Equal(2.17m, actPurchase.PriceOfFuel);

			// Verify that the third purchase was updated correctly.
			actPurchase = vehicle.Purchases[2];
			Assert.Equal(new DateTime(2010, 1, 1), actPurchase.Date);
			Assert.Equal(7.62m, actPurchase.Gallons);
			Assert.Equal(299.99m, actPurchase.TripMilage);
			Assert.Equal(4.56m, actPurchase.Cost);
			Assert.Equal(13690, actPurchase.Odometer);
			Assert.Equal(624.59M + 299.99m, actPurchase.TotalMilage);
			Assert.Equal(39.37m, actPurchase.MpgThisTrip);
			Assert.Equal(38.10m, actPurchase.MpgTotal);
			Assert.Equal(24.27m, actPurchase.CumulativeGallons);
			Assert.Equal(12765.42m, actPurchase.OdoDiff);
			Assert.Equal(2.53m, actPurchase.MilesPerDay);
			Assert.Equal(17.71m, actPurchase.MilesPerWeek);
			Assert.Equal(0.60m, actPurchase.PriceOfFuel);

			// Verify that the vehicle data was updated correctly in the store.
			AnhkhegService srvAfter = new(Filename);
			var afterVehicles = srvAfter.GetVehicles();
			VehicleData afterVehicle = afterVehicles.Find(v => v.Name == "2008 Honda Fit")!;
			Assert.Equal(vehicle, afterVehicle);
		}

		[Fact]
		[CustomBeforeAfter]
		public void CreatePurchase_VehicleNotFound()
		{
			Filename = System.Reflection.MethodBase.GetCurrentMethod()!.Name + ".json";

			// ASSEMBLE
			AnhkhegService srv = new(Filename);
			VehicleData badVehicle = new("1980 Chevrolet Citation");

			// ACT + ASSERT
			Exception ex = Assert.Throws<VehicleNotFoundException>(
				() => srv.CreatePurchase(badVehicle, new DateTime(2020, 1, 1), 12.34m, 234.5m, 45.67m, 13690));
		}

		[Fact]
		[CustomBeforeAfter]
		public void CreatePurchase_DuplicateOdometerValue()
		{
			Filename = System.Reflection.MethodBase.GetCurrentMethod()!.Name + ".json";

			// ASSEMBLE
			AnhkhegService srv = new(Filename);
			var vehicles = srv.GetVehicles();
			VehicleData vehicle = vehicles.Find(v => v.Name == "2008 Honda Fit")!;

			// ACT + ASSERT
			Exception ex = Assert.Throws<DuplicateOdometerValueException>(
				() => srv.CreatePurchase(vehicle, new DateTime(2010, 1, 2), 12.34m, 234.5m, 45.67m, 13690));
			ex = Assert.Throws<DuplicateOdometerValueException>(
				() => srv.CreatePurchase(vehicle, new DateTime(2009, 1, 2), 12.34m, 234.5m, 45.67m, 9876));
		}

		[Fact]
		[CustomBeforeAfter]
		public void CreatePurchase_InconsistentDate()
		{
			Filename = System.Reflection.MethodBase.GetCurrentMethod()!.Name + ".json";

			// ASSEMBLE
			AnhkhegService srv = new(Filename);
			var vehicles = srv.GetVehicles();
			VehicleData vehicle = vehicles.Find(v => v.Name == "2008 Honda Fit")!;

			// ACT + ASSERT
			Exception ex;
			// Date in between first and second but odometer before first.
			ex = Assert.Throws<InconsistentDateException>(
				() => srv.CreatePurchase(vehicle, new DateTime(2009, 1, 2), 12.34m, 234.5m, 45.67m, 9875));
			// Date after second but odometer before second.
			ex = Assert.Throws<InconsistentDateException>(
				() => srv.CreatePurchase(vehicle, new DateTime(2010, 1, 2), 12.34m, 234.5m, 45.67m, 13689));
			// Date before first but odometer after.
			ex = Assert.Throws<InconsistentDateException>(
				() => srv.CreatePurchase(vehicle, new DateTime(2008, 1, 1), 12.34m, 234.5m, 45.67m, 9877));
			// Date in between first and second but odometer after second.
			ex = Assert.Throws<InconsistentDateException>(
				() => srv.CreatePurchase(vehicle, new DateTime(2009, 1, 2), 12.34m, 234.5m, 45.67m, 13691));
		}

		[Fact]
		[CustomBeforeAfter]
		public void CreatePurchase_DuplicateDateAfter()
		{
			Filename = System.Reflection.MethodBase.GetCurrentMethod()!.Name + ".json";

			// ASSEMBLE
			AnhkhegService srv = new(Filename);
			var vehicles = srv.GetVehicles();
			VehicleData vehicle = vehicles.Find(v => v.Name == "2008 Honda Fit")!;
			DateTime date = new DateTime(2009, 1, 1);
			decimal gallons = 7.89m;
			decimal tripMilage = 303.5m;
			decimal cost = 17.12m;
			int odometer = 10179;

			// ACT
			srv.CreatePurchase(vehicle, date, gallons, tripMilage, cost, odometer);

			// ASSERT
			Assert.Equal(3, vehicle.Purchases.Count);

			PurchaseData actPurchase;

			// Verify that the first purchase didn't change.
			actPurchase = vehicle.Purchases[0];
			Assert.Equal(new DateTime(2009, 1, 1), actPurchase.Date);
			Assert.Equal(8.76m, actPurchase.Gallons);
			Assert.Equal(321.09m, actPurchase.TripMilage);
			Assert.Equal(1.23m, actPurchase.Cost);
			Assert.Equal(9876, actPurchase.Odometer);
			Assert.Equal(321.09m, actPurchase.TotalMilage);
			Assert.Equal(36.65m, actPurchase.MpgThisTrip);
			Assert.Equal(36.65m, actPurchase.MpgTotal);
			Assert.Equal(8.76m, actPurchase.CumulativeGallons);
			Assert.Equal(9554.91m, actPurchase.OdoDiff);
			Assert.Equal(321.09m, actPurchase.MilesPerDay);
			Assert.Equal(2247.63m, actPurchase.MilesPerWeek);
			Assert.Equal(0.14m, actPurchase.PriceOfFuel);

			// Verify the second purchase.
			actPurchase = vehicle.Purchases[1];
			Assert.Equal(date, actPurchase.Date);
			Assert.Equal(gallons, actPurchase.Gallons);
			Assert.Equal(tripMilage, actPurchase.TripMilage);
			Assert.Equal(cost, actPurchase.Cost);
			Assert.Equal(odometer, actPurchase.Odometer);
			Assert.Equal(624.59m, actPurchase.TotalMilage);
			Assert.Equal(38.47m, actPurchase.MpgThisTrip);
			Assert.Equal(37.51m, actPurchase.MpgTotal);
			Assert.Equal(16.65m, actPurchase.CumulativeGallons);
			Assert.Equal(9554.41m, actPurchase.OdoDiff);
			Assert.Equal(321.09m + tripMilage, actPurchase.MilesPerDay);
			Assert.Equal((321.09m + tripMilage) * 7, actPurchase.MilesPerWeek);
			Assert.Equal(2.17m, actPurchase.PriceOfFuel);

			// Verify that the third purchase was updated correctly.
			actPurchase = vehicle.Purchases[2];
			Assert.Equal(new DateTime(2010, 1, 1), actPurchase.Date);
			Assert.Equal(7.62m, actPurchase.Gallons);
			Assert.Equal(299.99m, actPurchase.TripMilage);
			Assert.Equal(4.56m, actPurchase.Cost);
			Assert.Equal(13690, actPurchase.Odometer);
			Assert.Equal(624.59m + 299.99m, actPurchase.TotalMilage);
			Assert.Equal(39.37m, actPurchase.MpgThisTrip);
			Assert.Equal(38.10m, actPurchase.MpgTotal);
			Assert.Equal(24.27m, actPurchase.CumulativeGallons);
			Assert.Equal(12765.42m, actPurchase.OdoDiff);
			Assert.Equal(2.53m, actPurchase.MilesPerDay);
			Assert.Equal(17.71m, actPurchase.MilesPerWeek);
			Assert.Equal(0.60m, actPurchase.PriceOfFuel);

			// Verify that the vehicle data was updated correctly in the store.
			AnhkhegService srvAfter = new(Filename);
			var afterVehicles = srvAfter.GetVehicles();
			VehicleData afterVehicle = afterVehicles.Find(v => v.Name == "2008 Honda Fit")!;
			Assert.Equal(vehicle, afterVehicle);
		}

		[Fact]
		[CustomBeforeAfter]
		public void CreatePurchase_DuplicateDateBefore()
		{
			Filename = System.Reflection.MethodBase.GetCurrentMethod()!.Name + ".json";

			// ASSEMBLE
			AnhkhegService srv = new(Filename);
			var vehicles = srv.GetVehicles();
			VehicleData vehicle = vehicles.Find(v => v.Name == "2008 Honda Fit")!;
			DateTime date = new DateTime(2009, 1, 1);
			decimal gallons = 5;
			decimal tripMilage = 100;
			decimal cost = 15;
			int odometer = 9776;

			// ACT
			srv.CreatePurchase(vehicle, date, gallons, tripMilage, cost, odometer);

			// ASSERT
			Assert.Equal(3, vehicle.Purchases.Count);

			PurchaseData actPurchase;

			// Verify that the first purchase.
			actPurchase = vehicle.Purchases[0];
			Assert.Equal(new DateTime(2009, 1, 1), actPurchase.Date);
			Assert.Equal(gallons, actPurchase.Gallons);
			Assert.Equal(tripMilage, actPurchase.TripMilage);
			Assert.Equal(cost, actPurchase.Cost);
			Assert.Equal(odometer, actPurchase.Odometer);
			Assert.Equal(tripMilage, actPurchase.TotalMilage);
			Assert.Equal(20, actPurchase.MpgThisTrip);
			Assert.Equal(20, actPurchase.MpgTotal);
			Assert.Equal(gallons, actPurchase.CumulativeGallons);
			Assert.Equal(9676, actPurchase.OdoDiff);
			Assert.Equal(tripMilage, actPurchase.MilesPerDay);
			Assert.Equal(tripMilage * 7, actPurchase.MilesPerWeek);
			Assert.Equal(3, actPurchase.PriceOfFuel);

			// Verify that the second purchase was updated correctly.
			actPurchase = vehicle.Purchases[1];
			Assert.Equal(date, actPurchase.Date);
			Assert.Equal(8.76m, actPurchase.Gallons);
			Assert.Equal(321.09m, actPurchase.TripMilage);
			Assert.Equal(1.23m, actPurchase.Cost);
			Assert.Equal(9876, actPurchase.Odometer);
			Assert.Equal(421.09m, actPurchase.TotalMilage);
			Assert.Equal(36.65m, actPurchase.MpgThisTrip);
			Assert.Equal(30.60m, actPurchase.MpgTotal);
			Assert.Equal(13.76m, actPurchase.CumulativeGallons);
			Assert.Equal(9454.91m, actPurchase.OdoDiff);
			Assert.Equal(421.09m, actPurchase.MilesPerDay);
			Assert.Equal(421.09m * 7, actPurchase.MilesPerWeek);
			Assert.Equal(0.14m, actPurchase.PriceOfFuel);

			// Verify that the third purchase was updated correctly.
			actPurchase = vehicle.Purchases[2];
			Assert.Equal(new DateTime(2010, 1, 1), actPurchase.Date);
			Assert.Equal(7.62m, actPurchase.Gallons);
			Assert.Equal(299.99m, actPurchase.TripMilage);
			Assert.Equal(4.56m, actPurchase.Cost);
			Assert.Equal(13690, actPurchase.Odometer);
			Assert.Equal(721.08m, actPurchase.TotalMilage);
			Assert.Equal(39.37m, actPurchase.MpgThisTrip);
			Assert.Equal(33.73m, actPurchase.MpgTotal);
			Assert.Equal(21.38m, actPurchase.CumulativeGallons);
			Assert.Equal(12968.92m, actPurchase.OdoDiff);
			Assert.Equal(1.98m, actPurchase.MilesPerDay);
			Assert.Equal(13.86m, actPurchase.MilesPerWeek);
			Assert.Equal(0.60m, actPurchase.PriceOfFuel);

			// Verify that the vehicle data was updated correctly in the store.
			AnhkhegService srvAfter = new(Filename);
			var afterVehicles = srvAfter.GetVehicles();
			VehicleData afterVehicle = afterVehicles.Find(v => v.Name == "2008 Honda Fit")!;
			Assert.Equal(vehicle, afterVehicle);
		}
	}
}
