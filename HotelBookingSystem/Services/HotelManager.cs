using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;
using HotelBookingSystem.Models;

namespace HotelBookingSystem.Services
{
    public class HotelManager
    {
        private readonly List<Hotel> _hotels;
        private readonly List<Booking> _bookings;

        public HotelManager(string hotelsFilePath, string bookingsFilePath)
        {
            _hotels = LoadHotels(hotelsFilePath);
            _bookings = LoadBookings(bookingsFilePath);
        }

        private List<Hotel> LoadHotels(string filePath)
        {
            var json = File.ReadAllText(filePath);
            return JsonSerializer.Deserialize<List<Hotel>>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }

        private List<Booking> LoadBookings(string filePath)
        {
            var json = File.ReadAllText(filePath);
            return JsonSerializer.Deserialize<List<Booking>>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }

        public string ProcessCommand(string command)
        {
            if (command.StartsWith("Availability(") && command.EndsWith(")"))
            {
                return ProcessAvailability(command);
            }
            else if (command.StartsWith("Search(") && command.EndsWith(")"))
            {
                return ProcessSearch(command);
            }
            else
            {
                throw new ArgumentException("Invalid command format");
            }
        }

        private string ProcessAvailability(string command)
        {
            var args = ParseCommand(command, "Availability");

            if (args.Length != 3)
            {
                throw new ArgumentException("Availability command requires 3 arguments: hotelId, date/dateRange, roomType");
            }

            var hotelId = args[0];
            var dateInput = args[1];
            var roomType = args[2];

            var hotel = GetHotel(hotelId);
            var (startDate, endDate) = ParseDateRange(dateInput);

            var availability = CalculateAvailability(hotel, startDate, endDate, roomType);
            return availability.ToString();
        }

        private string ProcessSearch(string command)
        {
            var args = ParseCommand(command, "Search");

            if (args.Length != 3)
            {
                throw new ArgumentException("Search command requires 3 arguments: hotelId, daysAhead, roomType");
            }

            var hotelId = args[0];
            var daysAhead = int.Parse(args[1]);
            var roomType = args[2];

            var hotel = GetHotel(hotelId);
            var today = DateTime.Today;
            var endDate = today.AddDays(daysAhead);

            var availableRanges = FindAvailableRanges(hotel, today, endDate, roomType);

            if (availableRanges.Count == 0)
            {
                return string.Empty;
            }

            return string.Join(", ", availableRanges.Select(r =>
                $"({r.StartDate.ToString("yyyyMMdd")}-{r.EndDate.ToString("yyyyMMdd")}, {r.AvailableRooms})"));
        }

        private string[] ParseCommand(string command, string commandName)
        {
            var startIdx = commandName.Length + 1;
            var endIdx = command.Length - 1;
            var argsString = command.Substring(startIdx, endIdx - startIdx);

            return argsString.Split(',').Select(s => s.Trim()).ToArray();
        }

        private Hotel GetHotel(string hotelId)
        {
            var hotel = _hotels.FirstOrDefault(h => h.Id == hotelId);
            if (hotel == null)
            {
                throw new ArgumentException($"Hotel {hotelId} not found");
            }
            return hotel;
        }

        private (DateTime startDate, DateTime endDate) ParseDateRange(string dateInput)
        {
            if (dateInput.Contains("-"))
            {
                var parts = dateInput.Split('-');
                var startDate = DateTime.ParseExact(parts[0], "yyyyMMdd", CultureInfo.InvariantCulture);
                var endDate = DateTime.ParseExact(parts[1], "yyyyMMdd", CultureInfo.InvariantCulture);
                return (startDate, endDate);
            }
            else
            {
                var date = DateTime.ParseExact(dateInput, "yyyyMMdd", CultureInfo.InvariantCulture);
                return (date, date);
            }
        }

        private int CalculateAvailability(Hotel hotel, DateTime startDate, DateTime endDate, string roomType)
        {
            var totalRooms = hotel.Rooms.Count(r => r.RoomType == roomType);

            var overlappingBookings = _bookings
                .Where(b => b.HotelId == hotel.Id && b.RoomType == roomType)
                .Where(b => BookingsOverlap(b, startDate, endDate))
                .Count();

            return totalRooms - overlappingBookings;
        }

        private bool BookingsOverlap(Booking booking, DateTime startDate, DateTime endDate)
        {
            var bookingStart = DateTime.ParseExact(booking.Arrival, "yyyyMMdd", CultureInfo.InvariantCulture);
            var bookingEnd = DateTime.ParseExact(booking.Departure, "yyyyMMdd", CultureInfo.InvariantCulture)
                                        .AddDays(-1); 

            return bookingStart <= endDate && bookingEnd >= startDate;
        }


        private List<AvailabilityRange> FindAvailableRanges(Hotel hotel, DateTime startDate, DateTime endDate, string roomType)
        {
            var ranges = new List<AvailabilityRange>();
            var currentDate = startDate;

            while (currentDate < endDate)
            {
                var availability = CalculateAvailability(hotel, currentDate, currentDate, roomType);

                if (availability > 0)
                {
                    var rangeStart = currentDate;
                    var rangeEnd = currentDate;
                    var minAvailability = availability;
                    
                    while (rangeEnd < endDate)
                    {
                        var nextDay = rangeEnd.AddDays(1);
                        var nextAvailability = CalculateAvailability(hotel, nextDay, nextDay, roomType);

                        if (nextAvailability <= 0)
                        {
                            break;
                        }

                        rangeEnd = nextDay;
                        minAvailability = Math.Min(minAvailability, nextAvailability);
                    }

                    ranges.Add(new AvailabilityRange
                    {
                        StartDate = rangeStart,
                        EndDate = rangeEnd,
                        AvailableRooms = minAvailability
                    });

                    currentDate = rangeEnd.AddDays(1);
                }
                else
                {
                    currentDate = currentDate.AddDays(1);
                }
            }

            return ranges;
        }

        private class AvailabilityRange
        {
            public DateTime StartDate { get; set; }
            public DateTime EndDate { get; set; }
            public int AvailableRooms { get; set; }
        }
    }
}