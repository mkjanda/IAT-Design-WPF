# Changelog

All notable changes to **IAT Design (WPF)** are documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added

- .NET 10 / WPF designer application with MVVM (CommunityToolkit.Mvvm), MediatR, FluentValidation, and DI
- Domain model for IAT tests: blocks, trials, keys, text/image stimuli, instruction screens, surveys, layout
- OPC package persistence (`.iat`) with embedded images and JSON via `IProjectPackageService`
- Designer tabs: **Blocks**, **Layout**, **Stimuli**, **Trials**, **Instructions**, **Surveys**, **Deploy**
- Instruction screens: Text, Keyed Response, and Mock Item, with live layout-locked preview
- Survey designer with multiple response types (Likert, multiple choice, multi-select, date, text, number, regex), image header, and item editors
- Live trial and instruction previews on the Blocks tab (sequence rows, keys, error mark, outline)
- New / Open / Save / Save As project workflow with dirty tracking and close confirmation
- Network services for activation, email verification, deployment, results, and item-slide retrieval
- Dialog service, error banner, and localized string resources
- Export pipeline under `IAT.Core.Services.Export` (replaces former package/export god objects)
- Domain validation (`Validate` / `ValidateEntireTest`) plus FluentValidation validators
- Unit test foundation for domain validation (`xUnitIAT`)
- Production-oriented persistent `WebSocketService` (connect, keep-alive, reconnect, thread-safe send, connection state)
- `MediatR` request/notification handlers for receiving and responding to server websocket messages for test deployment, result retrieval, and maintenance tasks.

### Changed

- Domain objects no longer touch network, UI, or file system; side effects moved to services and extension methods
- Top-level `IatTest` owns child collections; children reference peers by `Guid`
- Client–server interactions split into focused services with `IWebSocketService` injected
- Style dictionaries consolidated: shell theme in `Styles.xaml`, control styles/converters in `ViewStyles.xaml`
- Residual code-behind removed from Trials L/R assignment, TextStimulus placeholder, and MainWindow tab handlers
- `Block.Name` is observable so renames update bound lists immediately
- Continue key locked to Space for instruction screens (standard IAT practice)
- Result data no longer carries legacy versioning fields
- Errors prefer the main-window error banner over modal dialogs where appropriate
- Deploy action buttons (header + bottom bar) use shared Secondary/Danger button styles with larger height and padding so labels are fully visible
- Left-pane list data (Blocks, Stimuli, Trials, Instructions, Surveys, Deployed Tests) uses larger type (14–15 pt) and light foreground `#F0F0F0` / `#B0B0B0` for secondary lines; shared `DarkListBoxItem` style applied for consistent selection and contrast on the dark theme
- Deploy tab activation calls `IServerReportService.RetrieveServerReport` and maps `ServerReport` into the account bar and deployed-tests list; WebSocket stays open while the tab is visible and is closed on deactivate
- `IServerReportService` registered in DI; transaction handlers bound per-call so they do not fight other network services

### Removed

- Support for pre–.NET 10 targets
- WinForms-era layout system and `LayoutElement` (superseded by `LayoutItem` / layout calculator)
- Old image-caching approach
- `TestPackage` / `TestExportService` god objects

### Fixed

- Inverted “every stimulus must be used in at least one trial” check in `ValidateEntireTest`
- Image generation service after packaging data-flow changes
- `TransactionState.Clear` now resets the completion event (previously called `Set`, so the next `WaitOne` returned immediately)
- Duplicate Page build-action items
- `App.xaml` root namespace (`IAT_Design_WPF`) blocking compilation
- Layout editor sizing, repositioning, and save/load
- Block/trial preview resizing with the window
- Surveys: add-item commands not re-evaluating `CanExecute` after selection changes

### Security

- Product activation via public/private AES key exchange (product key + verified email)
- Server handshake reduced to AES challenge/response of a random string (no extra WebSocket round-trip for show)
- Dedicated AES encrypt/decrypt service for sensitive payloads

### Deprecated

- None

