using IAT.Core.Enumerations;
using System;
using System.Collections.Generic;
using System.IO.Packaging;
using System.Text;
using System.Xml.Linq;
using System.Xml.Serialization;
using System.Xml.Schema;
using static IAT.Core.Enumerations.PartType;
using java.util;

namespace IAT.Core.Models
{
    /// <summary>
    /// Represents an observable GUID value that notifies subscribed observers when its value changes.
    /// </summary>
    /// <remarks>Implements the IObservable<Guid> interface, allowing observers to subscribe and receive
    /// notifications whenever the GUID value is updated. This class is typically used in scenarios where changes to a
    /// GUID value need to be tracked or propagated to multiple listeners. Thread safety is not guaranteed; external
    /// synchronization may be required if accessed from multiple threads.</remarks>
    public class ObservableValue<T> : IObservable<T>, IPackagePart
    {
        /// <summary>
        /// Gets or sets the URI associated with this instance.
        /// </summary>
        [XmlElement("Uri", Form = XmlSchemaForm.Unqualified, IsNullable = true)]
        public Uri? Uri { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier for the object.
        /// </summary>
        [XmlElement("Id", Form = XmlSchemaForm.Unqualified)]
        public Guid Id { get; set; } = Guid.NewGuid();

        
        /// <summary>
        /// Gets the type of the package part represented by this instance.
        /// </summary>
        [XmlElement("PackagePartType", Form = XmlSchemaForm.Unqualified)]
        public PartType PackagePartType => PartType.ObservableValue;

        /// <summary>
        /// Gets or sets the unique identifier that was observed.
        /// </summary>
        [XmlElement("ObservedGuid", Form = XmlSchemaForm.Unqualified)]
        private T? ObservedValue { get; set; } = default;


        private readonly List<IObserver<T>> Observers;

        /// <summary>
        /// Gets or sets the current GUID value being observed.
        /// </summary>
        /// <remarks>Setting this property notifies all registered observers of the new value. Observers
        /// are notified immediately after the value is set.</remarks>
        [XmlIgnore]
        public T? Value
        {
            get => ObservedValue ?? default;
            set
            {
                if (value == null)
                    return;
                ObservedValue = value;
                foreach (var observer in Observers)
                    observer.OnNext(ObservedValue);
            }
        }

        /// <summary>
        /// Subscribes the specified observer to receive notifications of GUID values.
        /// </summary>
        /// <remarks>If the observer is already subscribed, it will not be added again. Disposing the
        /// returned IDisposable will remove the observer from the subscription list.</remarks>
        /// <param name="observer">The observer that will receive notifications. Cannot be null.</param>
        /// <returns>An IDisposable that can be used to unsubscribe the observer from receiving notifications.</returns>
        public IDisposable Subscribe(IObserver<T> observer)
        {
            if (!Observers.Contains(observer))
                Observers.Add(observer);
            observer.OnNext(ObservedValue ?? default);
            return new Unsubscriber(Observers, observer);
        }

        /// <summary>
        /// Initializes a new instance of the ObservableGuid class.
        /// </summary>
        public ObservableValue() 
        {
            Observers = new List<IObserver<T>>();
        }

        /// <summary>
        /// Initializes a new instance of the ObservableGuid class with the specified GUID value.
        /// </summary>
        /// <param name="value">The GUID value to be observed and stored by this instance.</param>
        public ObservableValue(T value)
        {
            Observers = new List<IObserver<T>>();
            ObservedValue = value;
        }

        /// <summary>
        /// Private class that implements IDisposable to handle unsubscribing observers.
        /// </summary>
        private class Unsubscriber : IDisposable
        {
            private List<IObserver<T>> _observers;
            private IObserver<T> _observer;

            public Unsubscriber(List<IObserver<T>> observers, IObserver<T> observer)
            {
                _observers = observers;
                _observer = observer;
            }

            public void Dispose()
            {
                if (_observer != null && _observers.Contains(_observer))
                {
                    _observers.Remove(_observer);
                }
            }
        }
    }
}
