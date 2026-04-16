# Changelog
All notable changes to IAT Design (WPF) will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/), and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]
### Added
###### --Support for .NET 10.0
###### --Added extension methods for model objects to handle logic that was previously contained within the model objects themselves. This allows for better separation of concerns and makes the model objects more lightweight.
###### --Added support for receiving serializable objects over the network by unboxing them to be deserialized anonymously. This allows for greater flexibility in handling data received from the network without needing to define specific types for deserialization.
###### --Added a service for handling network communication and data processing, which can be used to manage the flow of data between the application and external sources. This includes services for sending and receiving data, as well as processing and transforming data as needed. This service will be able handle multiple types of exchanges.
###### --Added support for receiving serializable objects over the network by unboxing them to be deserialized anonymously. This allows for greater flexibility in handling data received from the network without needing to define specific types for deserialization.	
###### --Added classes for a model and service to manage the layout of the application, including support for different types of layouts and the ability to customize the layout based on user preferences. 
###### --Added a package service to handle the saving and loading of IAT test configurations as OPC (Open Packaging Conventions) files, which can include embedded images and JSON data. This allows for easy distribution and sharing of IAT tests while ensuring that all necessary resources are included in a single package.
###### --Added validation functionality to the IAT test configuration process, which can be used to ensure that all necessary components and resources are included in the test configuration and that the configuration is valid before it is saved or executed. This may include checks for missing or invalid data, as well as checks for compatibility with the application and its dependencies.
### Changed
###### --Updated dependencies to latest versions
###### --Modified the architecture to better support the new features and improvements, including changes to the way data is handled and processed within the application. This may include changes to the structure of the codebase, as well as updates to existing components to improve performance and maintainability.
###### --Refactored code to improve readability and maintainability, including changes to variable names, function signatures, and overall code organization. This may also include the removal of deprecated code and the addition of new comments and documentation to help developers understand the changes made.
###### --Updated documentation to reflect changes and new features, including updates to existing documentation as well as the addition of new documentation for any new features or changes made. This may include updates to user guides, API documentation, and other relevant materials to ensure that users and developers have access to accurate and up-to-date information about the application.
###### --Migrated core Model Objects to comply with modern C# practices. The top level object now contains references to all domain model child objects while they contain only Guids.
###### --Removed any code from domain model objects that causes side effects outside the onject itself, including logic that modifies observable variables contained withint the model objects. 
###### --No domain modeel obect has access to network, display, or the file system any longer.
###### --Rewrote the domain models for the IAT to be more lightweight and focused on representing the data and state of the application, without containing any logic that modifies observable variables or has side effects outside of the object itself. This allows for better separation of concerns and makes the domain models easier to maintain and test.	
### Deprecated
###### --Deprecated support for older versions of .NET, as the application now requires .NET 10.0 or later to take advantage of the latest features and improvements. This may require users to update their development environment and dependencies to ensure compatibility with the new version of the application.
###### --Deprecated the old implementation of the layout system, which was based on WinForms patterns and was not well-suited to the needs of the application. This may require users to adapt to new workflows and processes when working with the application, but will ultimately result in a more efficient and scalable architecture that better supports the needs of the application and its users.
###### --Deprecated the old image caching system, which was previously used to manage the loading and display of images within the application. This may require users to adapt to new workflows and processes when working with images in the application, but will ultimately result in a more efficient and scalable architecture that better supports the needs of the application and its users.
###### --Depreciated the old package management system, which was previously used to handle the saving and loading of IAT test configurations as OPC file to comply with the modern domain model / service design paradigm.
### Removed
###### --Removed support for older versions of .NET, as the application now requires .NET 10.0 or later to take advantage of the latest features and improvements. This may require users to update their development environment and dependencies to ensure compatibility with the new version of the application.
###### --Removed deprecated code and features that are no longer necessary or relevant, as part of the ongoing effort to improve the performance and maintainability of the application. This may include the removal of old APIs, functions, or components that have been replaced by newer and more efficient alternatives.
###### --Dismanted the old architecture to make way for a new, more efficient and scalable architecture that better supports the needs of the application and its users. This may involve significant changes to the codebase and may require users to adapt to new workflows and processes when working with the application.
###### --Older classes that followed WinForms patterns are being removed as they are rewritten. Those that remain do so as reference for newer implementations.
###### --Removed LayoutElement class becausee of overlap with LayoutItem.
###### --Removed the old exception based validation system, which was previously used to validate IAT test configurations and other aspects of the application. This may require users to adapt to new workflows and processes when working with the application, but will ultimately result in a more efficient and scalable architecture that better supports the needs of the application and its users.
### Fixed
### Security
###### --Enhanced the security of product activation to a public/private AES key exchange. Product activation is neccesitated by allowing the user to upload images to the server for display. A product key and verified email address are essential to organizational self-protection.