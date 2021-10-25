using System;

namespace FeeCalculator.Models
{
	public class Vehicle
	{
		public string RegistrationNumber { get; set; }
		public VehicleType Type { get; set; }
		public int DailyAmount { get; set; }
		public string FeeDate { get; set; } = DateTime.Now.ToString("yyyy-MM-dd");
	}

	public enum VehicleType
	{
		Motorbike = 0,
		Car = 1,
		Emergency = 2,
		Diplomat = 3,
		HeavyBus = 4, // If the bus weight is 14ton or more
		Military = 5
	}
}
