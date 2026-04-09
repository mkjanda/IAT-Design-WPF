using IAT.Core.Enumerations;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
using System.Xml.Schema;
using System.IO.Packaging;

namespace IAT.Core.Models
{
    /// <summary>
    /// Observes changes to a GUID value and provides notification support for GUID updates.
    /// </summary>
    /// <remarks>Implements the IObserver<Guid> interface to receive updates from an observable GUID source.
    /// Supports resource management through IDisposable. Typically used to track and respond to changes in GUID values
    /// within an observable pattern.</remarks>
    [XmlRoot("ValueObserver")]
    public class ValueObserver<T> : IObserver<T>, IDisposable
    {
        /// <summary>
        /// Gets or sets the URI associated with this instance.
        /// </summary>
        [XmlElement("Uri", Form = XmlSchemaForm.Unqualified, IsNullable = true)]
        public Uri? Uri => PackUriHelper.CreatePartUri(new Uri($"{typeof(ValueObserver<T>).ToString()}/{Id}.xml", UriKind.Relative));

        /// <summary>
        /// Gets the part type associated with the package.
        /// </summary>
        [XmlElement("PartType", Form = XmlSchemaForm.Unqualified)]
        public PartType PackagePartType => PartType.ValueObserver;

        /// <summary>
        /// Gets or sets the unique identifier for the object.
        /// </summary>
        [XmlElement("Id", Form = XmlSchemaForm.Unqualified)]
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// Gets or sets the current subscription to the observable sequence.
        /// </summary>
        [XmlIgnore]
        private IDisposable? Subscription { get; set; } = null;

        /// <summary>
        /// Gets the unique identifier value represented by this instance.
        /// </summary>
        [XmlElement("Value", Form = XmlSchemaForm.Unqualified, IsNullable = true)]
        public T? Value { get; private set; }

        /// <summary>
        /// Initializes a new instance of the GuidObserver class.
        /// </summary>
        public ValueObserver() { }

        /// <summary>
        /// Initializes a new instance of the GuidObserver class and subscribes to updates from the specified
        /// ObservableGuid.
        /// </summary>
        /// <remarks>The constructor immediately subscribes the observer to the provided ObservableGuid
        /// and sets the initial Value property to the current value of the ObservableGuid. The observer will receive
        /// updates as the ObservableGuid changes.</remarks>
        /// <param name="guid">The ObservableGuid instance to observe for value changes. Cannot be null.</param>
        public ValueObserver(IObservable<T> value)
        {
            Subscription = value.Subscribe(this);
        }

        /// <summary>
        /// Receives a new GUID value and updates the current value accordingly.
        /// </summary>
        /// <param name="value">The new GUID value to set as the current value.</param>
        public void OnNext(T value)
        {
            Value = value ?? default;
        }
        /// <summary>
        /// Handles an error that has occurred during processing.
        /// </summary>
        /// <param name="ex">The exception that describes the error condition.</param>
        /// <exception cref="NotImplementedException">The method is not implemented.</exception>
        public void OnError(Exception ex) { throw new NotImplementedException(); }

        /// <summary>
        /// Notifies the observer that the provider has finished sending push-based notifications.
        /// </summary>
        /// <exception cref="NotImplementedException">The method is not implemented.</exception>
        public void OnCompleted() { throw new NotImplementedException(); }

        /// <summary>
        /// Releases all resources used by the current instance.
        /// </summary>
        /// <remarks>Call this method when the instance is no longer needed to free resources promptly.
        /// After calling this method, the instance should not be used.</remarks>
        public void Dispose()
        {
            Subscription?.Dispose();
        }

    }
}
