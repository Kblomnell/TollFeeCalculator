using System;
using System.Collections.Generic;
using FeeCalculator.Models;
using Microsoft.Data.Sqlite;

namespace FeeCalculator.Services
{
	public class DatabaseService
	{
		private readonly string connectiongString = "Data Source=../../../../Database/TollCalculatorDB.sqlite";

		public void CreateDatabaseTables()
		{
			using (SqliteConnection con = new(connectiongString))
			{
				con.Open();

				string holidayTable = "CREATE TABLE IF NOT EXISTS Holiday (Id INTEGER UNIQUE PRIMARY KEY AUTOINCREMENT, Date TEXT UNIQUE)";

				using (SqliteCommand cmd = new(holidayTable, con))
				{
					cmd.ExecuteNonQuery();
				}

				string tollFeeTable = "CREATE TABLE IF NOT EXISTS TollFee (Id INTEGER UNIQUE PRIMARY KEY AUTOINCREMENT, TimeSpan TEXT NOT NULL UNIQUE, Tax INTEGER NOT NULL)";

				using (SqliteCommand cmd = new(tollFeeTable, con))
				{
					cmd.ExecuteNonQuery();
				}

				string transactionTable = "CREATE TABLE IF NOT EXISTS VehicleTransaction (Id INTEGER UNIQUE PRIMARY KEY AUTOINCREMENT, RegistrationNumber TEXT NOT NULL UNIQUE, DailyAmount INTEGER NOT NULL DEFAULT 0, FeeDate TEXT DEFAULT current_date)";

				using (SqliteCommand cmd = new(transactionTable, con))
				{
					cmd.ExecuteNonQuery();
				}
			}

			PopulateHolidays();
			PopulateTollFees();
		}

		public void InsertHoliday(string date)
		{
			try
			{
				using (SqliteConnection con = new(connectiongString))
				{
					con.Open();

					string InsertSql = "INSERT INTO Holiday (Date) values (@date)";

					using (SqliteCommand cmd = new(InsertSql, con))
					{
						cmd.Parameters.AddWithValue("@date", date);

						cmd.ExecuteNonQuery();
					}
				}
			}
			catch (Exception exception)
			{
				Console.WriteLine(exception.Message);
			}
		}

		public void InsertTollFee(TollFeeCalculator tollFeeCalculator)
		{
			try
			{
				using (SqliteConnection con = new(connectiongString))
				{
					con.Open();

					string InsertSql = "INSERT INTO TollFee (TimeSpan, Tax) values (@timeSpan, @tax)";

					using (SqliteCommand cmd = new(InsertSql, con))
					{
						cmd.Parameters.AddWithValue("@timeSpan", tollFeeCalculator.TimeSpan);
						cmd.Parameters.AddWithValue("@tax", tollFeeCalculator.Tax);

						cmd.ExecuteNonQuery();
					}
				}
			}
			catch (Exception exception)
			{
				Console.WriteLine(exception.Message);
			}
		}

		public void InsertTransaction(Vehicle vehicle)
		{
			try
			{
				if (!RegistrationNumberExists(vehicle.RegistrationNumber))
				{
					using (SqliteConnection con = new(connectiongString))
					{
						con.Open();

						string InsertSql = "INSERT INTO VehicleTransaction (RegistrationNumber, DailyAmount) values (@registrationNumber, @dailyAmount)";

						using (SqliteCommand cmd = new(InsertSql, con))
						{
							cmd.Parameters.AddWithValue("@registrationNumber", vehicle.RegistrationNumber);
							cmd.Parameters.AddWithValue("@dailyAmount", vehicle.DailyAmount);

							cmd.ExecuteNonQuery();
						}
					}
				}
				else
				{
					UpdateTransaction(vehicle);
				}
			}
			catch (Exception exception)
			{
				Console.WriteLine(exception.Message);
			}
		}

		public int GetVehicleDailyAmount(string registrationNumber)
		{
			int amount = 0;

			try
			{
				using (SqliteConnection con = new(connectiongString))
				{
					con.Open();

					string query = "SELECT * FROM VehicleTransaction " +
						$"WHERE RegistrationNumber = @registrationNumber";

					using (SqliteCommand cmd = new(query, con))
					{
						cmd.Parameters.AddWithValue("@registrationNumber", registrationNumber);

						using (SqliteDataReader reader = cmd.ExecuteReader())
						{
							while (reader.Read())
							{
								amount = Convert.ToInt32(reader["DailyAmount"]);
							}
						}
					}
				}
			}
			catch (Exception exception)
			{
				Console.WriteLine(exception.Message);
			}

			return amount;
		}

		public List<string> GetHolidays()
		{
			List<string> holidays = new();

			try
			{
				using (SqliteConnection con = new(connectiongString))
				{
					con.Open();

					string query = "SELECT * FROM Holiday";
					using (SqliteCommand cmd = new(query, con))
					{
						using (SqliteDataReader reader = cmd.ExecuteReader())
						{
							while (reader.Read())
							{
								holidays.Add((string)reader["Date"]);
							}
						}
					}
				}
			}
			catch (Exception exception)
			{
				Console.WriteLine(exception.Message);
			}

			return holidays;
		}

		public List<TollFeeCalculator> GetTollFees()
		{
			List<TollFeeCalculator> tollFees = new();

			try
			{
				using (SqliteConnection con = new(connectiongString))
				{
					con.Open();

					string query = "SELECT * FROM TollFee";
					using (SqliteCommand cmd = new(query, con))
					{
						using (SqliteDataReader reader = cmd.ExecuteReader())
						{
							while (reader.Read())
							{
								TollFeeCalculator tollFee = new();
								tollFee.TimeSpan = (string)reader["TimeSpan"];
								tollFee.Tax = Convert.ToInt32(reader["Tax"]);
								tollFees.Add(tollFee);
							}
						}
					}
				}
			}
			catch (Exception exception)
			{
				Console.WriteLine(exception.Message);
			}

			return tollFees;
		}

		private void UpdateTransaction(Vehicle vehicle)
		{
			try
			{
				using (SqliteConnection con = new(connectiongString))
				{
					con.Open();

					string InsertSql = @"Update VehicleTransaction 
                    set DailyAmount=@dailyAmount
                    where RegistrationNumber = @registrationNumber and FeeDate = CURRENT_DATE";

					using (SqliteCommand cmd = new(InsertSql, con))
					{
						cmd.Parameters.AddWithValue("@dailyAmount", SqliteType.Integer).Value = vehicle.DailyAmount;
						cmd.Parameters.AddWithValue("@registrationNumber", SqliteType.Text).Value = vehicle.RegistrationNumber;

						cmd.ExecuteNonQuery();
					}
				}
			}
			catch (Exception exception)
			{
				Console.WriteLine(exception.Message);
			}
		}

		private bool RegistrationNumberExists(string registrationNumber)
		{
			var exists = false;

			try
			{
				using (SqliteConnection con = new SqliteConnection(connectiongString))
				{
					con.Open();

					string commandText = "SELECT count(*) FROM VehicleTransaction WHERE RegistrationNumber=@registrationNumber and FeeDate = CURRENT_DATE";

					using (SqliteCommand cmd = new(commandText, con))
					{
						cmd.Parameters.AddWithValue("@registrationNumber", registrationNumber);
						int count = Convert.ToInt32(cmd.ExecuteScalar());

						if (count == 1)
						{
							exists = true;
						}
					}
				}
			}
			catch (Exception exception)
			{
				Console.WriteLine(exception.Message);
			}

			return exists;
		}

		private void PopulateTollFees()
		{
			TollFeeCalculator tollFee0 = new() { TimeSpan = "0600-0629", Tax = 9 };
			InsertTollFee(tollFee0);

			TollFeeCalculator tollFee1 = new() { TimeSpan = "0630-0659", Tax = 16 };
			InsertTollFee(tollFee1);

			TollFeeCalculator tollFee2 = new() { TimeSpan = "0700-0759", Tax = 22 };
			InsertTollFee(tollFee2);

			TollFeeCalculator tollFee3 = new() { TimeSpan = "0800-0829", Tax = 16 };
			InsertTollFee(tollFee3);

			TollFeeCalculator tollFee4 = new() { TimeSpan = "0830-1459", Tax = 9 };
			InsertTollFee(tollFee4);

			TollFeeCalculator tollFee5 = new() { TimeSpan = "1500-1529", Tax = 16 };
			InsertTollFee(tollFee5);

			TollFeeCalculator tollFee6 = new() { TimeSpan = "1530-1659", Tax = 22 };
			InsertTollFee(tollFee6);

			TollFeeCalculator tollFee7 = new() { TimeSpan = "1700-1759", Tax = 16 };
			InsertTollFee(tollFee7);

			TollFeeCalculator tollFee8 = new() { TimeSpan = "1800-1829", Tax = 9 };
			InsertTollFee(tollFee8);
		}

		private void PopulateHolidays()
		{
			var dateTime0 = new DateTime(2021, 01, 01);
			InsertHoliday(dateTime0.Date.ToString("yyyy-MM-dd"));

			var dateTime1 = new DateTime(2021, 01, 05);
			InsertHoliday(dateTime1.Date.ToString("yyyy-MM-dd"));

			var dateTime2 = new DateTime(2021, 01, 06);
			InsertHoliday(dateTime2.Date.ToString("yyyy-MM-dd"));

			var dateTime3 = new DateTime(2021, 04, 01);
			InsertHoliday(dateTime3.Date.ToString("yyyy-MM-dd"));

			var dateTime4 = new DateTime(2021, 04, 02);
			InsertHoliday(dateTime4.Date.ToString("yyyy-MM-dd"));

			var dateTime5 = new DateTime(2021, 04, 05);
			InsertHoliday(dateTime5.Date.ToString("yyyy-MM-dd"));

			var dateTime6 = new DateTime(2021, 04, 30);
			InsertHoliday(dateTime6.Date.ToString("yyyy-MM-dd"));

			var dateTime7 = new DateTime(2021, 05, 12);
			InsertHoliday(dateTime7.Date.ToString("yyyy-MM-dd"));

			var dateTime8 = new DateTime(2021, 05, 13);
			InsertHoliday(dateTime8.Date.ToString("yyyy-MM-dd"));

			var dateTime9 = new DateTime(2021, 06, 25);
			InsertHoliday(dateTime9.Date.ToString("yyyy-MM-dd"));

			var dateTime10 = new DateTime(2021, 11, 05);
			InsertHoliday(dateTime10.Date.ToString("yyyy-MM-dd"));

			var dateTime11 = new DateTime(2021, 12, 24);
			InsertHoliday(dateTime11.Date.ToString("yyyy-MM-dd"));

			var dateTime12 = new DateTime(2021, 12, 31);
			InsertHoliday(dateTime12.Date.ToString("yyyy-MM-dd"));
		}
	}
}
