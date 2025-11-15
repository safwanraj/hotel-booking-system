using System;
using System.IO;
using HotelBookingSystem.Services;

namespace HotelBookingSystem
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 4 || args[0] != "--hotels" || args[2] != "--bookings")
            {
                Console.WriteLine("Usage: myapp --hotels hotels.json --bookings bookings.json");
                return;
            }

            var hotelsFile = args[1];
            var bookingsFile = args[3];

            if (!File.Exists(hotelsFile) || !File.Exists(bookingsFile))
            {
                Console.WriteLine("Error: One or both files not found.");
                return;
            }

            var hotelManager = new HotelManager(hotelsFile, bookingsFile);

            Console.WriteLine("Hotel Booking System");
            Console.WriteLine("Enter commands (or blank line to exit):");
            Console.WriteLine("Examples:");
            Console.WriteLine("  Availability(H1, 20240901, SGL)");
            Console.WriteLine("  Availability(H1, 20240901-20240903, DBL)");
            Console.WriteLine("  Search(H1, 365, SGL)");
            Console.WriteLine();

            while (true)
            {
                Console.Write("> ");
                var input = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(input))
                {
                    break;
                }

                try
                {
                    var result = hotelManager.ProcessCommand(input.Trim());
                    Console.WriteLine(result);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                }
            }
        }
    }
}