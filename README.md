# IAT-Design-WPF

A modern WPF desktop application for designing, validating, and packaging Implicit Association Tests (IATs).  
Create complete test configurations—stimuli (text and image), blocks, trials, instruction screens, and surveys—then persist them as self-contained OPC packages.

## Current Status (August 2026)

**Design features are production-ready.**  
The **Deploy** tab is present in the UI and wired for development, but **server-side deployment, result retrieval, and live test administration will not function until the coordinated server release**.  

**Expected availability: 1 October 2026.**

Until then, use the application to author and package tests. Deployment workflows that contact the server will fail or return incomplete results.

## Features

- **Test Design** – Create and edit full IAT configurations with blocks, trials, and response keys.
- **Stimulus Management** – Text and image stimuli with live preview, font/color controls, and polymorphic serialization.
- **Instruction Screens** – Three screen types (Text, Keyed Response, Mock Item) with block assignment and layout-aware live preview.
- **Surveys** – Multi-item surveys supporting Likert, multiple-choice, multi-select, date, bounded text/number, regex, headers, and image items.
- **Layout Editor** – Visual editor for key positions, instruction regions, error marks, and continue prompts; values drive both designer previews and final package layout.
- **Validation** – Domain-level FluentValidation covering blocks, trials, stimuli, and instruction screens.
- **Package Persistence** – Save / Open / Save As using OPC packages that embed images and JSON configuration.
- **Deploy Tab** – UI and client-side wiring exist. **Server interaction is disabled until the October 2026 release.**

## Requirements

- .NET 10 SDK
- Windows (WPF runtime)
- Visual Studio 2022 or later (recommended)

## Installation

```bash
git clone https://github.com/mkjanda/IAT-Design-WPF.git
cd IAT-Design-WPF
dotnet restore
dotnet build
dotnet run --project "IAT Design WPF"
```

## Usage

1. Launch the application.
2. Create a new test or open an existing `.iat` package.
3. Use the **Blocks**, **Layout**, **Stimuli**, **Trials**, **Instructions**, and **Surveys** tabs to build the configuration.
4. Save the package. The package is self-contained and ready for later deployment.
5. The **Deploy** tab is visible but will not successfully contact the server until the coordinated release (expected 1 October 2026).

## Project Structure

| Project            | Responsibility                                      |
|--------------------|-----------------------------------------------------|
| `IAT.Core`         | Domain models, services, validation, export, networking |
| `IAT.ViewModels`   | MVVM view models and commands                       |
| `IAT.Views`        | WPF controls, dialogs, converters, styles           |
| `IAT Design WPF`   | Application entry point, DI composition, main window |

## Contributing

Fork the repository, create a feature branch, and open a pull request.  
All changes must include appropriate tests and follow the existing coding standards and architectural boundaries (domain models remain pure; side-effects live in services).

## License

MIT License. See `LICENSE` for details.
