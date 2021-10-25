using System;
using System.Collections.Generic;
using System.Linq;
using FeeCalculator.Models;
using FeeCalculator.Services;

namespace Sim
{
	public class Program
	{
		private static readonly DatabaseService dbService = new();
		private static readonly Random rnd = new();
		private static readonly DateTime from = new(2021, 10, 25);
		private static readonly DateTime to = new(2021, 10, 26);

		static void Main(string[] args)
		{
			dbService.CreateDatabaseTables();

			List<DateTime> dateTimes = new();
			for (int i = 0; i < 10; i++)
			{
				dateTimes.Add(GetRandomDate(from, to));
			}

			TollFeeCalculator.GetTollFee(dateTimes, GetRandomVehicle());
		}

		public static DateTime GetRandomDate(DateTime from, DateTime to)
		{
			var range = to - from;

			var randTimeSpan = new TimeSpan((long)(rnd.NextDouble() * range.Ticks));

			return from + randTimeSpan;
		}

		public static Vehicle GetRandomVehicle()
		{
			Vehicle vehicle = new Vehicle
			{
				RegistrationNumber = $"{RandomString(3)}-{RandomNumberString(3)}",
				DailyAmount = 0,
				Type = VehicleType.Car
			};

			return vehicle;
		}

		private static string RandomString(int length)
		{
			const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

			return new string(Enumerable.Repeat(chars, length)
			  .Select(s => s[rnd.Next(s.Length)]).ToArray());
		}

		private static string RandomNumberString(int length)
		{
			const string chars = "1234567890";

			return new string(Enumerable.Repeat(chars, length)
			  .Select(s => s[rnd.Next(s.Length)]).ToArray());
		}
	}
}
