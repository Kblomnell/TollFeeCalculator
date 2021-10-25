using System;
using System.Collections.Generic;
using FeeCalculator.Services;

namespace FeeCalculator.Models
{
	public class TollFeeCalculator
	{
		public string TimeSpan { get; set; }
		public int Tax { get; set; }

		private static readonly DatabaseService dbService = new();

		public static int GetTollFee(List<DateTime> dateTimes, Vehicle vehicle)
		{
			var totalFee = 0;
			try
			{
				DateTime startDate = dateTimes[0];

				foreach (var date in dateTimes)
				{
					var nextFee = CalculateTollFee(date, vehicle);
					var tempFee = CalculateTollFee(startDate, vehicle);

					var result = date.Subtract(startDate).TotalMinutes;

					if (result <= 60)
					{
						if (totalFee > 0)
						{
							totalFee -= tempFee;
						}

						if (nextFee >= tempFee)
						{
							tempFee = nextFee;
						}

						totalFee += tempFee;
					}
					else
					{
						totalFee += nextFee;
					}

					vehicle.DailyAmount = totalFee;
					dbService.InsertTransaction(vehicle);

					if (dbService.GetVehicleDailyAmount(vehicle.RegistrationNumber) > 60)
					{
						totalFee = 60;
					}
				}
			}
			catch (Exception exception)
			{
				Console.WriteLine(exception.Message);
			}

			return totalFee;
		}

		private static int CalculateTollFee(DateTime dateTime, Vehicle vehicle)
		{
			int fee = 0;
			List<TollFeeCalculator> tollFees = dbService.GetTollFees();

			try
			{
				if (IsTollFreeVehicle(vehicle.Type))
				{
					return fee;
				}

				if (IsTollFreeDate(dateTime))
				{
					return fee;
				}

				var timeSpan = int.Parse(dateTime.ToString("HHmm"));

				foreach (var item in tollFees)
				{
					var startTime = int.Parse(item.TimeSpan.Split('-')[0]);
					var endTime = int.Parse(item.TimeSpan.Split('-')[1]);

					if (timeSpan >= startTime && timeSpan <= endTime)
					{
						return item.Tax;
					}
				}
			}
			catch (Exception exception)
			{
				Console.WriteLine(exception.Message);
			}

			return fee;
		}

		private static bool IsTollFreeVehicle(VehicleType tollFreeVehicle)
		{
			var isFree = false;

			try
			{
				switch (tollFreeVehicle)
				{
					case VehicleType.Motorbike:
						{
							isFree = true;
							break;
						}
					case VehicleType.Emergency:
						{
							isFree = true;
							break;
						}
					case VehicleType.Diplomat:
						{
							isFree = true;
							break;
						}
					case VehicleType.HeavyBus:
						{
							isFree = true;
							break;
						}
					case VehicleType.Military:
						{
							isFree = true;
							break;
						}
					default:
						{
							break;
						}
				}
			}
			catch (Exception exception)
			{
				Console.WriteLine(exception.Message);
			}

			return isFree;
		}

		private static bool IsTollFreeDate(DateTime dateTime)
		{
			List<string> holidays = dbService.GetHolidays();

			try
			{
				if (dateTime.Month == 7)
				{
					return true;
				}

				if (dateTime.DayOfWeek == DayOfWeek.Saturday || dateTime.DayOfWeek == DayOfWeek.Sunday)
				{
					return true;
				}

				foreach (var holiday in holidays)
				{
					if (dateTime.Date.ToString("yyyy-MM-dd") == holiday)
					{
						return true;
					}
				}
			}
			catch (Exception exception)
			{
				Console.WriteLine(exception.Message);
			}

			return false;
		}
	}
}
