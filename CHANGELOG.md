# Changelog
All notable changes to IAT Design (WPF) will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/), and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]
### Added
- Support for .NET 10.0
- Added extension methods for model objects to handle logic that was previously contained within the model objects themselves. This allows for better separation of concerns and makes the model objects lightweight.
- Added support for receiving serializable objects over the network by unboxing them to be deserialized anonymously. This allows for greater flexibility in handling data received from the network without needing to define specific types for deserialization.
- Added WebSocketService, a service for handling network communication and data processing, which can be used to manage the flow of data between the application and external sources. This includes services for sending and receiving data, as well as processing and transforming data as needed. This service will be able to handle multiple types of exchanges.
- Added classes for a model and service to manage the layout of the application, including support for different types of layouts and the ability to customize the layout based on user preferences. 
- Added a package service to handle the saving and loading of IAT test configurations as OPC (Open Packaging Conventions) files, which can include embedded images and JSON data. This allows for easy distribution and sharing of IAT tests while ensuring that all necessary resources are included in a single package.
- Added validation functionality to the IAT test configuration process, which can be used to ensure that all necessary components and resources are included in the test configuration and that the configuration is valid before it is saved or executed. This may include checks for missing or invalid data, as well as checks for compatibility with the application and its dependencies.
- Added unit tests to ensure the correctness and reliability of the new features and changes made to the application. This may include tests for individual components, as well as integration tests to ensure that the different parts of the application work together as expected. The addition of unit tests will help to improve the overall quality and maintainability of the codebase.
- Completed the first set of domain model objects for the IAT, which are designed to represent the data and state of the application in a way that is easy to understand and work with. These domain model objects will serve as the foundation for the application's data management and processing and will be used throughout the application to represent various aspects of the IAT test configuration and execution.
- Added model objects for the IAT, specifically those that are serialized to XML and uploaded to the server for transformation into the HTML and JavaScript that administer the test. These model objects are designed to represent the data and state of the IAT test configuration in a way that is easy to understand and work with, and will be used throughout the application to manage the IAT test configuration and execution
- Added functionality to the web service to negotiate and handle the retrieval of result data and associated test configuration data from the server.
- Added functionality to the web service to negotiate and handle the retrieval of image data from the server.
- Added functionality to the web service to negotiate and handle the retrieval of test configuration data from the server.
- Dialog service added to handle the display of dialogs within the application, including support for various types of dialogs such as confirmation dialogs, error dialogs, and informational dialogs. This service can be used to provide a consistent and user-friendly way to interact with users and provide feedback on their actions within the application.
- Added Mediator pattern to the application to facilitate communication between different components and services, allowing for better separation of concerns and improved maintainability. 
- Image generation service added to handle the creation and manipulation of images used in the IAT test, including support for various types of image processing and generation techniques. This service can be used to create custom images for the IAT test based on user input or predefined templates, as well as to manipulate existing images as needed for the test configuration.
- Product activation added to ensure that users have a valid product key and verified email address before they can access certain features of the application, such as uploading images to the server for display in the IAT test. 
- Added preparation of the test for deployment, including the generation of necessary files and resources, as well as the packaging of the test configuration and associated data for deployment to the server. 
- Added a collection of services in IAT.Core.Services.Export to replace the TestPackage and TestExportService "god objects." These services are responsible for handling the export of test configurations and associated data in a modular and maintainable way, allowing for better separation of concerns and improved code organization. The services may include functionality for generating the necessary files and resources for deployment, as well as packaging the test configuration and associated data into a format that can be easily deployed to the server.
- Added generation of item slides to the test preparation process, which can be used to create the necessary slides for the IAT test based on the test configuration and associated data. 
### Changed
- Updated dependencies to latest versions
- Modified the architecture to better support the new features and improvements, including changes to the way data is handled and processed within the application. 
- Refactored code to improve readability and maintainability, including changes to variable names, function signatures, and overall code organization. 
- Updated documentation to reflect changes and new features, including updates to existing documentation as well as the addition of new documentation for any new features or changes made. 
- Migrated core Model Objects to comply with modern C# practices. The top level object now contains references to all domain model child objects while they contain only Guids.
- Removed any code from domain model objects that causes side effects outside the  object itself, including logic that modifies observable variables contained within the model objects. 
- No domain model object has access to network, display, or the file system any longer.
- Segregated functionality from the model objects that describe the test configuration into extension methods and services, which can be used to handle logic and functionality that is related to the model objects but does not belong within the model objects themselves. This allows for better separation of concerns and makes the model objects lightweight and focused on representing the data and state of the application.
- Extracted the logic for handling network communication and data processing into a separate service, which can be used to manage the flow of data between the application and external sources. This allows for better separation of concerns and makes the codebase more modular and maintainable.
- Extricated the logic of specific client-server interactions from the web service "god object" which, as described above, focuses strictly on network communication and disbursement of events through Mediator.
- Mostly unrelated client-server interactions now form their own services and have the web service injected into them. 
- The way that result data is structured and managed has been updated, including the removal of versioning from the result data to conform to modern practices and to simplify the data structure. 
- Changed the way that dialogs are handled within the application, including updates to the dialog service and the way that different types of dialogs are displayed and managed. 
- Test deployment has been updated to include the generation of necessary files and resources, as well as the packaging of the test configuration and associated data for deployment to the server. 
### Deprecated
### Removed
- Removed support for older versions of .NET, as the application now requires .NET 10.0 or later to take advantage of the latest features and improvements. 
- Removed deprecated code and features that are no longer necessary or relevant, as part of the ongoing effort to improve the performance and maintainability of the application. 
- Older classes that followed WinForms patterns are being removed as they are rewritten. Those that remain do so as reference for newer implementations.
- Removed LayoutElement class because of overlap with LayoutItem.
- Removed the old image caching system, which was previously used to manage the loading and display of images within the application. 
- Removed versioning from the result data in order to conform to modern practices and to simplify the data structure. 
- Removed the old layout system, which was based on WinForms patterns and was not well-suited to the needs of the application. 
- The old TestPackage and TestExportService "god objects" have been removed and replaced with a collection of services in IAT.Core.Services.Export that handle the export of test configurations and associated data in a modular and maintainable way.
### Fixed
- Repaired the IAT.Core.Services.ImageGenerationService following a reworking of data flow surroundning test packaaging.
### Security
- Enhanced the security of product activation to a public/private AES key exchange. Product activation is necessitated by allowing the user to upload images to the server for display. A product key and verified email address are essential to organizational self-protection.
- Altered the handshaking algorithm with the server. It is now limited to a challenge/response exchange revolving around the AES encryption of a random string. This eliminates a web socket transaction from the process, and cuts "security for the sake of security."
- Added a new service to handle the encryption and decryption of data using AES encryption, which can be used to secure sensitive data and communications within the application. This may include support for generating and managing encryption keys, as well as functions for encrypting and decrypting data as needed. The addition of this service will help to improve the overall security of the application and protect user data from unauthorized access.
