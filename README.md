# MyPost C# Advanced

A small postal-processing console application built with C# and .NET. It loads people, homes, and letters from JSON files, validates the input records, creates a parcel queue, marks undeliverable letters as returned, and persists the result.

## Features

- Validation for people, homes, and letters
- JSON persistence for every model
- Case-insensitive person lookup with normalized whitespace
- Duplicate-safe add operations
- Parcel creation and return-to-sender handling
- Repeatable processing without duplicate queue entries
- Sample data generated only when input files do not already exist

## Run

The project targets .NET 10. From the repository root:

```powershell
dotnet run --project ConsoleApp/ConsoleApp.csproj
```

By default, JSON files are placed in a `Data` folder next to the compiled application. Pass a directory as the first argument to use a custom data location:

```powershell
dotnet run --project ConsoleApp/ConsoleApp.csproj -- ./Data
```

Input files are named `People.json`, `Houses.json`, and `Letters.json`. The processed queue is written to `ParcelPost.json`.

Run the automated test suite with:

```powershell
dotnet test ConsoleApp.Tests/ConsoleApp.Tests.csproj
```

## Processing rules

- Invalid records are ignored while loading input files.
- A letter is delivered to the receiver's registered home when both are found.
- If the receiver or their home cannot be found, the parcel is marked as returned and routed to the sender's registered address when available.
- Calling `ProcessLetters` again rebuilds the queue instead of duplicating entries.
