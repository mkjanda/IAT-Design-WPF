# Changelog
All notable changes to IAT Design (WPF) will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/), and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]
### Added
###### --Support for .NET 10.0
###### --Added extension methods for model objects to handle logic that was previously contained within the model objects themselves. This allows for better separation of concerns and makes the model objects more lightweight.
###### --Added support for receiving serializable objects over the network by unboxing them to be deserialized anonymously. This allows for greater flexibility in handling data received from the network without needing to define specific types for deserialization.
###### --Added a service for handling network communication and data processing, which can be used to manage the flow of data between the application and external sources. This includes services for sending and receiving data, as well as processing and transforming data as needed. This service will be able handle multiple types of exchanges.
### Changed
###### --Updated dependencies to latest versions
###### --Modified the architecture to better support the new features and improvements, including changes to the way data is handled and processed within the application. This may include changes to the structure of the codebase, as well as updates to existing components to improve performance and maintainability.
###### --Refactored code to improve readability and maintainability, including changes to variable names, function signatures, and overall code organization. This may also include the removal of deprecated code and the addition of new comments and documentation to help developers understand the changes made.
###### --Updated documentation to reflect changes and new features, including updates to existing documentation as well as the addition of new documentation for any new features or changes made. This may include updates to user guides, API documentation, and other relevant materials to ensure that users and developers have access to accurate and up-to-date information about the application.
###### --Added support for receiving serializable objects over the network by unboxing them to be deserialized anonymously. This allows for greater flexibility in handling data received from the network without needing to define specific types for deserialization.
###### --Added a service for handling network communication and data processing, which can be used to manage the flow of data between the application and external sources. This includes services for sending and receiving data, as well as processing and transforming data as needed. This service will be able handle multiple types of exchanges.
### Deprecated
### Removed
### Fixed
### Security
