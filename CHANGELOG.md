# Changelog

All notable changes to **IAT Design (WPF)** are documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/).
The project intends to follow [Semantic Versioning](https://semver.org/spec/2.0.0.html)
once a public release is cut. Until then, **drop numbers** are the versions:
each section is one `IAT-Design-WPF-master-N.zip` compared to the previous drop.

Dates are the newest file timestamp inside that zip, not the date the zip was
copied into this archive.

How to read it:

- **[Unreleased]** is work *after* drop 29 (named delta zips and the 2026-08-30 session).
- **[29] … [1]** are consecutive snapshots. Drop **7 is byte-identical to drop 6**.
- In-repo `CHANGELOG.md` inside the zips is **not** the source of truth. Drops
  1–19 carry a long essay that accretes slowly. Drops 20–28 carry a short
  Keep-a-Changelog skeleton that barely moves. This file is reconstructed from
  neighbor diffs of the trees.

Arc of the sequence:

| Drops | Window | What the product is becoming |
| --- | --- | --- |
| 1–7 | May 2026 | Domain + export pipeline + first network services. Almost no designer UI. |
| 8–16 | May–July | Blocks, Stimuli, Layout, Trials. File operations and live preview. |
| 17–19 | late July | Surveys, Deploy shell, Instructions. All seven tabs exist. |
| 20–24 | early August | Changelog hygiene, typo-filename fixes, server report / results crypto. |
| 25–29 | mid–late August | Deploy as a report list, 7-block authoring, handshake collapse. |
| Unreleased | 30 Aug + | GManifest XSD match, contain-fit / shrink-to-fit. |

---

## [Unreleased]

Work that exists as later deltas or in the 2026-08-30 source tree.
**Not** in `IAT-Design-WPF-master-29.zip`.

### Added

- `IFileManifestBuilder.AddFile` takes an explicit `resourceId` so ErrorMark (1000)
  and key outlines (1001 / 1002) match `DisplayItem.Id` / config IDs instead of
  `Files.Count + 1`
- Left and right key outlines as separate files (`KeyOutlineLeft.png` /
  `KeyOutlineRight.png`)

### Changed

- `Manifest` is flattened (no longer a `ManifestDirectory`) so `XmlSerializer`
  emits `ProductKey`, then `IATName`, then `File*`. That is the GManifest child
  order the Java XSD expects
- `ManifestType` serializes with `[XmlEnum]` tokens `FileManifest` /
  `ItemSlideManifest` (not `[Description]`)
- `ResourceType` wire tokens match the XSD exactly (`ErrorMark`, `Javascript`,
  `ItemSlide`, `Image`, `TestConfiguration`, `KeyOutline`)
- Image stimuli in preview / slides use contain-fit rather than letterboxed JPEG
- Key labels, instruction text, and continue prompts shrink-to-fit in their layout
  boxes
- `Manifest` registered in `XmlDeserializationService`

### Fixed

- Sample `Untitled-1.xml` was captured against the pre-flatten serializer; it is
  not a GManifest oracle. Regenerate it from current `Manifest` before re-validating
- Do not wrap `Manifest` in `GEnvelope` / `Envelope` for GManifest validation.
  That wrapper is unused on the wire; both sides send `Message` subclasses as the
  document root

### Sources

- `IAT-Design-WPF-Manifest-XmlAttribute-Fix.zip`
- `IAT-Design-WPF-ManifestType-XmlEnum-Fix.zip`
- `IAT-Design-WPF-Manifest-GManifest-Order-ResourceId.zip`
- `IAT-Design-WPF-Image-Contain-Fit.zip`
- `IAT-Design-WPF-Key-Label-Fit.zip`
- `IAT-Design-WPF-Instruction-Text-Fit.zip`
- `IAT-Design-WPF-Session-2026-08-30.zip`
- Notes: `GManifest-XML-Schema-Mismatch.docx`,
  `IAT-Design-WPF-Session-Transcript-2026-08-30.docx`

---

## [29] - 2026-08-28

Source: `IAT-Design-WPF-master-29.zip` versus drop 28.

Product drop: standard 7-block authoring, deploy-current-test, and the first cut
of GManifest enumerations. Handshake verbs on the client collapse to what the
server actually sends.

### Added

- Combined response keys: `Key.ComponentIds`, separator `" or "`,
  `Key.FormatStackedDisplay` / `FormatAuthoringDisplay`. Trials tab stores a
  single line (`Good or Flower`); preview and slides stack on `" or "`
- Generate 7-block IAT now builds compatible / incompatible keys from practice
  blocks 1–2. Those practice blocks are validated (instructions, trials, key text)
  before blocks 3–7 are generated
- Per-side response-key font family, size, and color on the Trials tab. After a
  standard 7-block structure exists, editors for blocks 3–7 are disabled. Practice
  edits update shared keys in place; `PropagateDerivedKeysFromPractice` rebuilds
  derived key text and style (style follows the first component, same as
  `CreateCombinedKey`)
- Block instructions as `FormattedText`: `IatTest` owns the collection;
  `Block.BlockInstructionsId` stays in sync with `Block.BlockInstructions`.
  `IatTest.EnsureBlockInstructions` creates or updates the row
- Deploy **Current Test**: `DeployManagerViewModel` runs
  `ITestExportService.PrepareForServerUploadAsync` then
  `ITestDeploymentService.Deploy`. Password from the header field / AppData;
  `IsBusy` locks the tab; `SetIATPassword` on success
- Export DI completions: `IItemSlideExportProcessor` and `ITestMapperService`
  registered. `BlockExportProcessor` takes `IImageGenerationService`
- `FileEntityType` and `ResourceType` live in `IAT.Core.Enumerations`
- Deploy handshake handlers for the verbs the server sends: `RequestManifest`,
  `RequestUpload`, `RequestEncryptionKey`, `RequestConfigFile`
- `ActivationStatus` enumeration

### Changed

- `WebSocketService.TransactionCommands` is populated once at construction.
  `TestDeploymentService` no longer overwrites the map per operation
- `TransactionState` carries ProductKey, password, IAT name, and both manifests
  across the deploy handshake. `RequestConnection` sends ProductKey from state
- Deploy **Test Name** is bound to `IatTest.Name`, independent of the package
  path. Synced when the tab activates; committed before `DeployCurrentTest`.
  Save still seeds `Name` from the filename only when it is still default or empty
- `Block.NumPresentations` is the administration count and is independent of
  `TrialIds.Count`. Defaults via `DefaultPresentationsFor`: 20 on blocks 4 and 7,
  10 otherwise
- Blocks-tab live preview: instruction `TextBlock` binds `FontFamily` / `FontSize`
  / `Foreground` to `LayoutViewModel`. Instruction Style pushes through
  `PushBlockInstructionsToPreview` → `ApplyBlockInstructions(text, style)`
- New blocks get their own empty Left/Right `Key` instances (no shared ids with
  Block 1). `LayoutViewModel.ApplyBlockKeys` no longer falls back to the first
  key in the collection by `LayoutItem`

### Removed

- “Download All Results” button and its placeholder command from the Deploy tab
  bottom bar (per-test Retrieve is the supported path)
- Non-functional “Save” button from the Blocks tab sidebar
- One-off handshake types the server does not send: `AbortTransaction`,
  `ClientDeleted` / `ClientFrozen`, `DeploymentSuccess` / `DeploymentFail`,
  `EncryptionKeyReceived`, `IATBeingDeployed`, `ManifestReceived`,
  `RequestIATUpload`, `RequestFileManifest`, `RequestFiles`,
  `RequestItemSlideManifest`

### Fixed

- Deploy Retrieve / Clear / Delete set `IsBusy`; action buttons disable via
  CanExecute until the call completes, fails, or times out
- Stimuli tab list is properly scrollable
- Delete Block on the Blocks tab is wired to `DeleteBlockCommand` (respects the
  7-block structure lock)
- Text stimulus font family, size, and color are preserved across successive
  “Add Text Stimulus” actions. Image-stimulus save no longer overwrites the
  remembered text style
- Default text style aligned to domain `TextStyle` (Segoe UI / 24 / Black)
- Stimulus Save no longer clears ListBox selection (`UpdateStimulus` mutates
  in place; `Saved` is raised before collection mutation)
- `Stimulus` inherits `ObservableObject`; `TextStimulus.Text` and
  `ImageStimulus.FileName` raise PropertyChanged
- Trials-tab key editors: load-suppress so a selection change cannot persist
  stale opposite-side text; `PersistKeySide` edits one side only; copy-on-write
  when a `Key` is shared across blocks
- Blocks-tab sequence `DataGrid`: `CanUserSortColumns=False`
- Export resolves left/right response keys with `GetKeyById` (`Key` implements
  `IFormattedText`). Looking keys up in the formatted-text cache dropped them
  from the package
- `BlockValidator` rejects blank or placeholder `"Block Instructions"` and
  requires non-empty left/right key ids
- `TextExportProcessor` adds the encoder frame before PNG encode, and no longer
  disposes the memory stream before `ToArray()`
- `BeginIATBlock.NumPresentations` is populated from `Block.NumPresentations`
  on export

---

## [28] - 2026-08-23

Source: `IAT-Design-WPF-master-28.zip` versus drop 27.

Protocol orchestration moves back into the focused network services.
The short-lived `WebSocketTransaction` helper from drop 27 does not survive.

### Changed

- `ActivationService`, `DeletionService`, `EmailVerificationService`,
  `GetItemSlidesService`, `ResendEmailVerificationService`,
  `ResultRetrievalService`, `ServerReportService`, and
  `TestDeploymentService` take the handshake steps back in-process
- `NoSuchIATHandler` / transaction success-fail handlers tightened to the
  remaining command set

### Removed

- `IAT.Core/Services/Network/WebSocketTransaction.cs`
- `IAT.Core/Services/Network/ItemSlideRetriever.cs` (retrieval stays on
  `GetItemSlidesService`)

---

## [27] - 2026-08-22

Source: `IAT-Design-WPF-master-27.zip` versus drop 26.

Two stories: the first **Generate 7-block IAT** lock on the Blocks tab,
and a collapse of per-operation MediatR handlers into fewer transmission types.

### Added

- Generate 7-block IAT from exactly two practice blocks.
  `IsStandardStructureLocked` disables Add Block / re-generate once the
  standard structure exists
- `IATExistsHandler` replaces separate deploy-vs-retrieve “IAT exists” handlers
- `WebSocketTransaction` helper (removed again in drop 28)

### Changed

- `Commands.cs` shrinks by more than half. Per-operation
  `RequestTransmission*` handlers give way to a shared
  `RequestTransmissionHandler`
- Network services slim down; drop 28 puts the logic back
- `LocalStorageService` and `ImagePackageService` expand
- `BlockEditViewModel` and `DeployManagerViewModel` are the two large
  view-model diffs

### Removed

- Separate deploy/retrieve existence handlers, password-valid-* family,
  `ItemSlidesReadyHandler`, `ResultsReadyHandler`, `VerifyPasswordHandler`,
  `RequestRSAKeyHandler`, and the `RequestTransmission<Operation>` family

### Fixed

- Error banner frozen-`Storyboard` / re-show path (`ErrorBannerControl`
  roughly doubles in this drop)

---

## [26] - 2026-08-20

Source: `IAT-Design-WPF-master-26.zip` versus drop 25.

Small protocol / config-file drop. No new designer tabs.

### Changed

- `AuthTokenHandler` becomes a real handler (string resources + transaction
  state) instead of a stub
- `BeginIATBlock` and related `IAT.Core/ConfigFile/*` types pick up
  `[XmlType]` / `[XmlIgnore]` so the uploaded config matches the Java side
- `ManifestReceivedCommand`, `RSAKeyHandler`, `EncryptionKeyReceivedHandler`
  trimmed
- `ResultSetDescriptor` loses unused surface
- `StringDictionary.xaml` copy edited

---

## [25] - 2026-08-19

Source: `IAT-Design-WPF-master-25.zip` versus drop 24.

Deploy tab becomes a report list, not just a form.

### Added

- Deployed-test **URL** on each report row, with `OpenUrlCommand`. Empty URL
  displays as an em-dash; malformed URLs fail quiet
- `AuthTokenHandler`, `NoSuchIATHandler`, generic
  `RequestTransmissionHandler`, and a stub
  `RequestTransmissionVerifyPasswordHandler`

### Changed

- Deployed-test timestamp prefers **upload time** and falls back to last
  data-retrieval for older server payloads
- `DeployManagerControl` gains the left-pane “Deployed Tests / IAT Reports”
  layout
- `NoSuchIATDeploymentHandler` and `RequestTransmissionActivationHandler`
  are replaced by the generic types above

### Removed

- `NoSuchIATDeploymentHandler`
- `RequestTransmissionActivationHandler`

---

## [24] - 2026-08-17

Source: `IAT-Design-WPF-master-24.zip` versus drop 23.

A handshake-envelope drop. Dark dialogs already landed in 23.

### Added

- `Envelope` wire type (`IAT.Core/Serializable/Envelope.cs`)

### Changed

- `Handshake`, `HandshakeHandler`, `TransactionRequest`, `ServerReport`,
  `ServerReportService`, `WebSocketService`, and `XmlDeserializationService`
  learn the envelope
- Deploy view-model and `StringDictionary` strings follow
- README touch-up

---

## [23] - 2026-08-16

Source: `IAT-Design-WPF-master-23.zip` versus drop 22.

Results crypto and the remaining deploy-handshake verbs.

### Added

- `ManifestType` enumeration
- `DeploymentSuccessHandler`, `DeploymentFailHandler`
- `RequestConfigFileHandler`, `RequestFileManifestHandler`,
  `RequestFilesHandler`, `RequestItemSlideManifestHandler`,
  `RequestItemSlidesHandler`, `RequestRSAKeyHandler`
- `ResultCryptoService` and the decryption service (filename in this drop
  is `ResutDescyptionService.cs`)

### Changed

- Confirmation and notification dialogs restyled to the dark theme
  (`ConfirmationDialog.xaml` / `NotificationDialog.xaml` first grow here)
- Broad handler pass around the new verbs

---

## [22] - 2026-08-11

Source: `IAT-Design-WPF-master-22.zip` versus drop 21.

Same-day follow-up to 21.

### Changed

- Results payload type renamed `ResultSet` → `TestRessults` (typo filename
  travels with it)
- `Commands`, `TransactionState`, item-slide / upload handlers, and
  `StringDictionary` adjusted to the new type

### Removed

- `IAT.Core/Serializable/ResultSet.cs`

---

## [21] - 2026-08-11

Source: `IAT-Design-WPF-master-21.zip` versus drop 20.

Server-report and deletion stack. App icon.

### Added

- Application icon (`IAT-Design.ico`)
- `TransationType` enumeration (filename spelling is in the tree)
- `ManifestReceivedCommand` (replaces `ManifestHandler`)
- `PasswordValidDeleteHandler`, `PasswordValidDeleteDataHandler`
- `RequestTransmissionServerReportHandler`, `ServerReportHandler`,
  `ResultSetDescriptorHandler`
- Wire types: `IWebSocketMessage`, `ResultSet` / descriptor / TOC,
  `ServerReport`
- `DeletionService`, `ServerReportService`

### Removed

- `ManifestHandler`

---

## [20] - 2026-08-06

Source: `IAT-Design-WPF-master-20.zip` versus drop 19.

Hygiene drop. In-repo changelog is rewritten from the May essay into the
short Keep-a-Changelog skeleton that then freezes through drop 28.

### Added

- `PlaceholderAttached` behavior for empty-field chrome
- `BoolToVisibilityConverter` under its correct filename
- `StimulusValidator` under its correct filename

### Changed

- `WebSocketService` takes the production-oriented persistent-connection
  work (connect, keep-alive, reconnect, thread-safe send)
- Residual code-behind removed from Trials L/R assignment, the text-stimulus
  placeholder, and MainWindow tab handlers
- Instruction / survey / layout / designer view-models touched for the
  same pass
- Domain validation tests in `xUnitIAT` expand

### Removed

- `SimulusValidator.cs` (typo filename)
- `BoolToVisibilityConverrter.cs` (typo filename)

### Fixed

- `TransactionState.Clear` resets the completion event (it previously
  called `Set`, so the next `WaitOne` returned immediately) — this is the
  window of `IAT-Design-WPF-TransactionState-Completion-Restore.zip`

---

## [19] - 2026-07-29

Source: `IAT-Design-WPF-master-19.zip` versus drop 18.

Cosmetic follow-up the same day as 18. No new types.

### Changed

- `Styles.xaml` and `ViewStyles.xaml`
- `InstructionManagerControl.xaml`
- README

---

## [18] - 2026-07-29

Source: `IAT-Design-WPF-master-18.zip` versus drop 17.

Instructions tab lands. All seven designer tabs now exist.

### Added

- `InstructionManagerViewModel` + `InstructionManagerControl`
- `InstructionScreenTypeConverter`
- Domain `KeyedInstructionScreen` under its correct name

### Changed

- Main window gains the Instructions tab
- Block preview and layout calculator grow instruction-screen preview
- Survey manager XAML follow-up

### Removed

- `KeyedInstructionsSveen.cs` (typo filename from the May domain pass)

---

## [17] - 2026-07-27

Source: `IAT-Design-WPF-master-17.zip` versus drop 16.

Surveys tab and Deploy tab land. The misspelled `IAT.VIewModels` project
folder is replaced by `IAT.ViewModels`.

### Added

- `SurveyManagerViewModel` + `SurveyManagerControl` (Likert, multiple
  choice, multi-select, date, text, number, regex, image header)
- `DeployManagerViewModel` + `DeployManagerControl` (first integrated
  Deploy UI — not yet a server-report list)
- `ResponseDefinition` on the domain
- `DateOnlyToStringConverter`, `NullToBoolConverter`,
  `NullToVisibilityConverter`
- `BoolToVisibilityConverrter.cs` (typo filename; renamed in drop 20)

### Removed

- `IAT.VIewModels/` project folder
- Converter copies that had been living under `IAT.ViewModels/Converters`
- `ObservableValue`, `SaveFile` leftovers

---

## [16] - 2026-07-23

Source: `IAT-Design-WPF-master-16.zip` versus drop 15.

File operations become a product workflow.

### Added

- `TestModifiedMessage` (dirty tracking)
- `KeyedDirectionJsonConverter`

### Changed

- New / Open / Save / Save As wiring on `MainWindow` /
  `TestDesignerViewModel` / `IDialogService` / `ProjectPackageService`
- Layout calculator and block/trial view-models follow dirty-state

---

## [15] - 2026-07-20

Source: `IAT-Design-WPF-master-15.zip` versus drop 14.

Preview and layout-editor sizing pass. No new files.

### Changed

- `Block`, `IatTest`, `LayoutCalculatorService`, `LayoutViewModel`,
  `BlockEditView`, `LayoutEditView`, `TrialsManager*`
- `KeyedDirection` enumeration
- `ValidationResult`

### Fixed

- Block / trial preview resizing with the window
- Layout editor repositioning and deflate-on-resize

---

## [14] - 2026-07-18

Source: `IAT-Design-WPF-master-14.zip` versus drop 13.

Trials and Blocks preview polish. Text-stimulus placeholder behavior.

### Changed

- `BlockEditViewModel` / `BlockEditView`
- `TrialsManagerViewModel` / `TrialsManagerControl`
- `TextStimulusEditControl` (+ code-behind placeholder)

---

## [13] - 2026-07-17

Source: `IAT-Design-WPF-master-13.zip` versus drop 12.

Layout tab and Trials tab land.

### Added

- `LayoutEditView` (+ code-behind)
- `TrialsManagerViewModel` + `TrialsManagerControl`

### Changed

- Main window hosts Layout and Trials tabs
- `LayoutViewModel` and `BlockEditView` grow to drive the new surfaces

---

## [12] - 2026-07-15

Source: `IAT-Design-WPF-master-12.zip` versus drop 11.

Stimuli-editor polish. No new files.

### Changed

- Image / text stimulus editors and the stimuli manager list
- `ViewStyles.xaml`, `IatTest`

---

## [11] - 2026-07-13

Source: `IAT-Design-WPF-master-11.zip` versus drop 10.

Six-week gap after drop 10. Text vs image stimulus editors split.

### Added

- `TextStimulusEditControl`, `ImageStimulusEditControl`,
  `ImageStimulusEditViewModel`
- `StimulusEditTemplateSelector`

### Changed

- Stimuli manager hosts the template selector
- Palette / style dictionaries
- Domain `TextStimulus` / `ImageStimulus`

---

## [10] - 2026-05-29

Source: `IAT-Design-WPF-master-10.zip` versus drop 9.

Stimuli library as a first-class manager, not a one-off editor.

### Added

- `StimuliManagerViewModel` + `StimuliManagerControl`

### Changed

- Main window, `BlockEditViewModel`, `StimuliEditViewModel`,
  `TestDesignerViewModel`

---

## [9] - 2026-05-21

Source: `IAT-Design-WPF-master-9.zip` versus drop 8.

Views move under `IAT.Views/Controls`. Error banner and dark shell styles.

### Added

- `BlockEditView` / `BlockEditViewModel` (replaces `BlockEditorView`)
- `StimulusEdit` + `StimuliEditViewModel`
- `ErrorBannerControl`
- `Styles.xaml`, `ModernStyles.xaml`, `ViewStyles.xaml`
- `UserNotificationMessage`
- `HalfValueConverter`
- `TestDesignerViewModel` returns under `Controls/`

### Removed

- `BlockEditorView`, `StimuliView`, `StimuliViewModel`
- `Dictionary1.xaml`
- leftover `IAT_Design_WPF/Services/LayoutService.cs`

---

## [8] - 2026-05-18

Source: `IAT-Design-WPF-master-8.zip` versus drop 7.

First designer chrome: a Blocks view and a Stimuli view.

### Added

- `BlockEditorView` + `BlockEditorView.cs`
- `StimuliView` + `StimuliViewModel`
- `BoolToBorderBrushConverter`
- `IAT.Views/Resources/styles.xaml`
- `ItemSlideRetriever`

### Removed

- Shell copy of `ImageGenerationService`
- `DeploymentUpdate` serializable
- `RawByteExportProcessor`
- `IAT.Views/Class1.cs`

---

## [7] - 2026-05-16

Source: `IAT-Design-WPF-master-7.zip` versus drop 6.

**Byte-identical to drop 6.** Kept in the numbering so later drop numbers
stay aligned with the zip names.

---

## [6] - 2026-05-16

Source: `IAT-Design-WPF-master-6.zip` versus drop 5.

The modular export pipeline replaces the `TestPackage` god object.
FluentValidation arrives (including the `SimulusValidator` typo filename).

### Added

- `IAT.Core/Services/Export/*`: `TestExportService`, `TestMapperService`,
  `BlockExportProcessor`, `StimulusExportProcessor`, `TextExportProcessor`,
  `ItemSlideExportProcessor`, `FileManifestBuilder`, `ExportContext`,
  `ExportResult`, `RawByteExportProcessor`
- `ProjectPackageService`, `ImagePackageService`
- `IatTestValidator`, `BlockValidator`, `TrialValidator`,
  `InstructionScreenValidator`, `SimulusValidator`
- `DomainExtensions`

### Removed

- `TestPackage` / `TestPackageService`
- `IAT.Core/Class1.cs`
- `Serializable/Stimulus.cs` (domain stimulus is the source of truth)
- A handful of now-unused service interfaces that moved next to their
  implementations

---

## [5] - 2026-05-11

Source: `IAT-Design-WPF-master-5.zip` versus drop 4.

Deployment handshake as its own service, not a pile of request types.

### Added

- `TestDeploymentService`
- `DeploymentStage` enumeration
- `DeploymentManifestReceivedHandler`, `EncryptionKeyReceivedHandler`,
  `IATBeingDeployedHandler`, `RequestIATUploadHandler`
- `NoSuchIATDeploymentHandler`, `NoSuchIATResultRetrievalHandler`
- `DeploymentUpdate` serializable

### Removed

- `NoSuchIATHandler` (split into the two more specific handlers above)

---

## [4] - 2026-05-10

Source: `IAT-Design-WPF-master-4.zip` versus drop 3.

Instruction-screen type names settle. Config-file `Trial` appears.

### Added

- `IAT.Core/ConfigFile/Trial.cs`
- Domain `MockItemInstructionScreen`, `TextInstructionScreen`
  under the names the rest of the tree uses

### Removed

- `MockItemInstructionsScreen.cs`, `TextInstructionsScreen.cs`

### Changed

- Config-file instruction / layout / display-item types
- `LayoutCalculatorService`, `IFormattedText`

---

## [3] - 2026-05-09

Source: `IAT-Design-WPF-master-3.zip` versus drop 2.

Upload config becomes `IATConfigFile`. Packaging becomes `TestPackageService`
(the object drop 6 later breaks up).

### Added

- `IATConfigFile`
- `TestPackage` + `TestPackageService`
- `ResultRetrievalService` under `Services/Network/`

### Removed

- `SerializableIatConfig`
- `UploadConfigMapperService`
- `Services/ResultRetrievalService.cs` (moved into `Network/`)

---

## [2] - 2026-05-08

Source: `IAT-Design-WPF-master-2.zip` versus drop 1.

Network and layout stop living in the WPF shell. Domain grows
`Layout` / `FormattedText` / `TextStyle`.

### Added

- Domain `Layout`, `FormattedText`, `TextStyle`, `IFormattedText`
- `LayoutItem` enumeration
- `ImageGenerationService`, `KeyService` in `IAT.Core`
- Network services under `IAT.Core/Services/Network/`:
  `WebSocketService`, `ActivationService`, `EmailVerificationService`,
  `GetItemSlidesService`, `ResendEmailVerificationService`
- `LayoutViewModel` and `TestDesignerViewModel` under `Controls/`
- `KeyViewModel`
- `SerializableIatConfig`, `UploadConfigMapperService`

### Removed

- Shell `LayoutService` / `ILayoutService` / `ISaveFileService`
- Domain `LayoutConfiguration`, config `TestConfig`
- Network services from the `IAT.Core/Services/` root (they moved)
- `IAT.VIewModels/TestDesignerViewModel.cs` and the top-level
  `IAT.ViewModels/LayoutViewModel.cs`

---

## [1] - 2026-05-04

Source: `IAT-Design-WPF-master-1.zip`.

Baseline of the WPF rewrite. This is not a blank repo. The architectural
bets that the later drops spend three months paying off are already here.

### Already present

- .NET 10 WPF shell (`IAT Design WPF`), `IAT.Core`, `IAT.ViewModels` /
  misspelled `IAT.VIewModels`, `IAT.Views`, `xUnitIAT`
- Domain objects for blocks, trials, keys, text/image stimuli, surveys,
  instruction-screen types (some still under typo filenames), layout
  calculator. `IatTest` owns children; children reference peers by `Guid`.
  Domain objects are not supposed to touch network, UI, or disk
- MediatR handlers for handshake, activation, email verification,
  password, IAT-exists, item slides, results, transaction success/fail
- First `WebSocketService` (root of `IAT.Core/Services`, not `Network/`)
- Activation, email verification, item-slide, and result-retrieval
  services as separate types with the socket injected
- OPC package interfaces (`IProjectPackageService`) and local storage
- Dialog service with stock WPF confirmation / notification windows
- Excel XSLT templates for result workbooks
- XML config-file types the server consumes (`BeginIATBlock`,
  `KeyedInstructionScreen`, surveys, …)
- `TransactionState`, `Manifest`, `Handshake`, `EncryptedRSAKey`

### Not present yet

- Designer tabs as they ship in drop 18. Drop 1 is core + shell +
  leftover WinForms-shaped services (`LayoutService` in the WPF project)
- Modular export pipeline (that is drop 6)
- Generate 7-block IAT, combined keys, deploy-current-test (drops 27–29)

---

## Foundation notes (all drops)

These decisions hold across the sequence. They are not re-stated in every
section.

- Target framework is `net10.0-windows`. Pre–.NET 10 targets are gone
- Result data no longer carries legacy versioning fields (stripped during
  the August results rewrite)
- Handshake with the server is an AES challenge/response of a random string
- Errors prefer the main-window error banner once that control exists
  (drop 9)
- `Block.Name` is observable so renames update bound lists immediately

### Security

- Product activation via public/private AES key exchange (product key +
  verified email)
- Dedicated AES encrypt/decrypt service for sensitive payloads (crypto
  types arrive in drop 23; the policy is older)

---

## How this file was built

Neighbor diffs of `IAT-Design-WPF-master-{1…29}.zip` (file add/remove/hash,
plus the view-model / handler / network files that actually moved).

In-repo `CHANGELOG.md` inside those zips was not used as evidence for
*when* a feature landed — only as a hint for *what the author thought
they were doing*. File-only churn (comment-only edits, identical-length
timestamp touches, DI registration order with no new service) is omitted.

Drop 7 is recorded even though it equals drop 6, so the zip numbers stay
a contiguous index.
)
