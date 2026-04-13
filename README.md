# IAT-Design-WPF

A WPF application for designing and managing Implicit Association Tests (IATs). This tool allows users to create, validate, and persist IAT test configurations, including stimuli (images and text), blocks, trials, and instructions, using a package-based file format.

## Features

- **Test Design**: Create and edit IAT tests with customizable blocks, trials, and stimuli.
- **Stimulus Management**: Support for image and text stimuli with polymorphic serialization.
- **Validation**: Built-in domain validation to ensure test integrity before saving.
- **Package Persistence**: Save and load tests as OPC (Open Packaging Conventions) files, embedding images and JSON data.
- **WPF UI**: Modern desktop interface for intuitive test authoring.

## Requirements

- .NET 10 SDK
- Windows (for WPF runtime)
- Visual Studio 2022 or later (recommended for development)

## Installation

1. Clone the repository:
   ```
   git clone https://github.com/mkjanda/IAT-Design-WPF.git
   cd IAT-Design-WPF
   ```
2. Restore dependencies:
   ```
   dotnet restore
   ```
3. Build the solution:
   ```
   dotnet build
   ```
4. Run the application:
   ```
   dotnet run --project "IAT Design WPF"
   ```

## Usage

1. Launch the application.
2. Create a new IAT test or load an existing one from a `.iat` package file.
3. Add blocks, trials, and stimuli (images or text).
4. Validate the test to check for errors.
5. Save the test as a package for distribution or further use.

For detailed API usage, refer to the service classes in `IAT.Core\Services`.

## Project Structure

- `IAT.Core`: Core domain models, services, and utilities.
- `IAT.ViewModels`: View models for MVVM architecture.
- `IAT.Views`: WPF views and UI components.
- `IAT Design WPF`: Main application entry point.

## Contributing

Contributions are welcome! Please fork the repository, create a feature branch, and submit a pull request. Ensure all changes include tests and follow the existing code style.

## License

This project is licensed under the MIT License. See `LICENSE` for details.
