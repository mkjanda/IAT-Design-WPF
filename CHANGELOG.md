# Changelog

All notable changes to **IAT Design (WPF)** are documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

Scope of this revision: contents of `IAT-Design-WPF-master-29.zip` versus
`IAT-Design-WPF-master-28.zip`, merged with the `[Unreleased]` notes already
present in that drop. Items that exist only in later delta zips (and are
**not** in zip 29) are listed under Known gaps — they are not claimed as shipped.

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
- `MediatR` request/notification handlers for receiving and responding to server websocket messages for test deployment, result retrieval, and maintenance tasks
- Block instructions as `FormattedText`: `IatTest` owns the collection; `Block.BlockInstructionsId` is set and kept in sync with `Block.BlockInstructions`. `IatTest.EnsureBlockInstructions` creates or updates the row
- Combined response keys: `Key.ComponentIds`, separator `" or "`, and `Key.FormatStackedDisplay` so the Trials tab stores a single line (`Good or Flower`) while preview and slides stack on `" or "`
- Generate 7-block IAT builds compatible / incompatible keys from practice blocks 1–2; those practice blocks are validated (instructions, trials, key text) before blocks 3–7 are generated
- Per-side response-key font family, size, and color on the Trials tab. After a standard 7-block structure exists, editors for blocks 3–7 are disabled; practice edits update shared keys in place and `PropagateDerivedKeysFromPractice` rebuilds derived key text and style (style follows the first component, same as `CreateCombinedKey`)
- Deploy **Current Test**: `DeployManagerViewModel` runs `ITestExportService.PrepareForServerUploadAsync` then `ITestDeploymentService.Deploy`. Password comes from the header field / AppData; `IsBusy` locks the tab; `SetIATPassword` runs on success
- Export DI completions: `IItemSlideExportProcessor` and `ITestMapperService` registered. `BlockExportProcessor` takes `IImageGenerationService`
- `FileEntityType` and `ResourceType` live in `IAT.Core.Enumerations` (`ResourceType` token is `ErrorMark`, not `ErrorMarker`)

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
- `Block.NumPresentations` is the administration count and is independent of `TrialIds.Count`. Defaults via `DefaultPresentationsFor`: 20 on blocks 4 and 7, 10 otherwise (Add Block, Create Block, and generate fill practice blocks still at ≤ 0)
- Deploy **Test Name** is bound to `IatTest.Name`, independent of the package file path. Synced when the tab activates; committed before `DeployCurrentTest`. Save still seeds `Name` from the filename only when it is still default or empty
- `WebSocketService.TransactionCommands` is populated once at construction. `TestDeploymentService` no longer overwrites the map per operation
- Deployment handshake handlers collapsed to the verbs the server actually sends: `RequestManifest`, `RequestUpload`, `RequestEncryptionKey`, `RequestConfigFile`. One-off types (`AbortTransaction`, `ClientDeleted` / `ClientFrozen`, `DeploymentSuccess` / `DeploymentFail`, `EncryptionKeyReceived`, `IATBeingDeployed`, `ManifestReceived`, `RequestIATUpload`, …) are removed
- `TransactionState` carries ProductKey, password, IAT name, and both manifests across the deploy handshake. `RequestConnection` sends ProductKey from state (not a one-off local-storage read on the wire message)

### Removed

- Support for pre–.NET 10 targets
- WinForms-era layout system and `LayoutElement` (superseded by `LayoutItem` / layout calculator)
- Old image-caching approach
- `TestPackage` / `TestExportService` god objects
- “Download All Results” button and its placeholder command from the Deploy tab bottom bar (per-test Retrieve is the supported path)
- Non-functional “Save” button from the Blocks tab sidebar (project-level New / Open / Save / Save As already cover persistence; block edits are live against the domain model)

### Fixed

- Inverted “every stimulus must be used in at least one trial” check in `ValidateEntireTest`
- Image generation service after packaging data-flow changes
- `TransactionState.Clear` now resets the completion event (previously called `Set`, so the next `WaitOne` returned immediately)
- Duplicate Page build-action items
- `App.xaml` root namespace (`IAT_Design_WPF`) blocking compilation
- Layout editor sizing, repositioning, and save/load
- Block/trial preview resizing with the window
- Surveys: add-item commands not re-evaluating `CanExecute` after selection changes
- Deploy tab: Retrieve / Clear / Delete set `IsBusy` for the duration of the network operation; all action buttons (Retrieve, Clear, Delete, Delete Selected, Refresh, Deploy) disable via CanExecute until the call completes, fails, or the service times out/cancels
- Stimuli tab list is properly scrollable (ListBox fills remaining DockPanel height; vertical scrollbar enabled)
- Delete Block button on the Blocks tab is wired to `DeleteBlockCommand` (respects the 7-block structure lock)
- Text stimulus font family, size, and color are preserved across successive “Add Text Stimulus” actions (last committed style is remembered on Save). Capture is restricted to pure text editors so saving an image stimulus no longer overwrites the remembered text style with base-class defaults
- Default text style aligned to domain `TextStyle` (Segoe UI / 24 / Black) in both the editor ViewModel and the remembered-style fields
- Stimulus Save no longer clears ListBox selection: `UpdateStimulus` mutates in place when the concrete type matches (avoids Remove+Add), and `Saved` is raised *before* any collection mutation so the manager handler cannot be detached mid-Save
- `Stimulus` inherits `ObservableObject`; `TextStimulus.Text` and `ImageStimulus.FileName` raise PropertyChanged so the Stimuli list updates in place after Save (no more frozen "New Text Stimulus" labels)
- New blocks get their own empty Left/Right `Key` instances (no shared ids with Block 1). `LayoutViewModel.ApplyBlockKeys` no longer falls back to the first key in the collection by `LayoutItem`
- Trials-tab key editors: load-suppress so a selection change cannot persist stale opposite-side text; `PersistKeySide` edits one side only; copy-on-write when a `Key` is shared across blocks (Block 5 reusing Block 2 after generate)
- Blocks-tab sequence `DataGrid`: `CanUserSortColumns=False` so instruction/trial order cannot be resorted by column header
- Export resolves left/right response keys with `GetKeyById` (`Key` implements `IFormattedText`). Block instructions still use `GetFormattedTextById`. Looking keys up in the formatted-text cache dropped response keys from the package
- `BlockValidator` rejects blank or placeholder `"Block Instructions"` and requires non-empty left/right key ids
- `TextExportProcessor` adds the encoder frame before PNG encode (export was writing empty images)
- Blocks-tab live preview: instruction `TextBlock` binds `FontFamily` / `FontSize` / `Foreground` to `LayoutViewModel`; Instruction Style pushes through `PushBlockInstructionsToPreview` → `ApplyBlockInstructions(text, style)`. Instruction-screen preview applies `screen.Style` to the shared body `TextBlock`

### Security

- Product activation via public/private AES key exchange (product key + verified email)
- Server handshake reduced to AES challenge/response of a random string (no extra WebSocket round-trip for show)
- Dedicated AES encrypt/decrypt service for sensitive payloads

### Deprecated

- None

### Known gaps (not in zip 29)

These exist as later deltas or analysis notes. Do not treat them as shipped in this drop.

- `ManifestType` still uses `[Description]`. GManifest expects `[XmlEnum]` tokens `FileManifest` / `ItemSlideManifest`
- `XmlSerializer` still emits `Files` before `ProductKey` / `IATName` because those members are inherited from `ManifestDirectory`. The Java XSD wants ProductKey/IATName first
- Wire format still dispatches `TransactionRequest` through `TransactionCommands`. Document-root `Message` subclasses (envelope dropped on the wire) are not in this zip
- Sample `Untitled-1.xml` still fails GManifest for the two serializer/XSD mismatches above

## How this file was built

Compared `IAT-Design-WPF-master-28.zip` (2026-08-23) to `IAT-Design-WPF-master-29.zip` (2026-08-29). Changelog bullets already present in zip 29 were kept. Source diffs that change designer behavior, export, or the deploy protocol were added. File-only churn (DI registration order with no new service, comment-only edits) was omitted.
