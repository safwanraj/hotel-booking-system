# Hotel Booking System

A small C# console application that reads hotel and booking data from JSON files
and allows two types of queries: checking availability for a date or date range,
and searching ahead for upcoming availability. I tried to keep the solution
simple, readable, and close to how I would normally approach the problem at work.

---

## Build & Run

### Build

From the project folder:

```
dotnet build
```

### Run

```
dotnet run --hotels hotels.json --bookings bookings.json
```

You’ll then be able to enter commands such as:

```
Availability(H1, 20251115, SGL)
Availability(H1, 20251115-20251118, DBL)
Search(H1, 40, SGL)
```

A blank line exits the program.

---

## Project Structure

```
HotelBookingSystem/
├── Program.cs
├── Models/
│   └── Models.cs
├── Services/
│   └── HotelManager.cs
├── hotels.json
└── bookings.json
```

I kept the structure small so everything is easy to review in one pass.

---

## Design Notes

* The program loads both JSON files at startup and keeps them in memory.
* Availability is calculated by:

  1. Counting total rooms of the requested type.
  2. Counting bookings that overlap the query period.
  3. Returning `total - overlapping` (may be negative for overbooking).
* Search walks day-by-day from today and groups consecutive days with positive availability into ranges.
* Departure dates are interpreted as exclusive (guest leaves on that date).

The focus was to keep the logic explicit and understandable without adding
unnecessary abstractions.

---

## Assumptions

* Dates follow `yyyyMMdd` format.
* Departure is exclusive (booking from `10–12` covers 10th and 11th).
* Invalid command formats are handled with a simple error message.
* Search uses the local current date (`DateTime.Today`) as the starting point.
* JSON inputs follow the structure described in the assignment.

---

## What I Would Improve Next

If this were a real production feature, I would consider:

* Adding proper validation for command input.
* Extracting date handling into a small helper to centralize logic.
* Adding unit tests around overlap calculation and search grouping.
* Making availability calculation more efficient if hotel/booking lists grew large.

For the assignment, I intentionally kept everything lean.

---

## Notes

I used AI tools to create the initial solution for this assignment. 
After that, I reviewed, tested, and refined the implementation to make sure 
I fully understand it—including the date overlap logic, availability calculation 
with overbooking, and the search range-finding method. I can confidently explain 
all design choices, discuss alternatives, and extend the code if needed.
