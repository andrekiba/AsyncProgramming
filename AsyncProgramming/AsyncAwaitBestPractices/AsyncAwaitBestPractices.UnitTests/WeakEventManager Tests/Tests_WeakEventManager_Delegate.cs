﻿using System;
using System.ComponentModel;
using NUnit.Framework;

namespace AsyncAwaitBestPractices.UnitTests
{
    class Tests_WeakEventManager_Delegate : BaseTest, INotifyPropertyChanged
    {
        readonly WeakEventManager _propertyChangedWeakEventManager = new WeakEventManager();

        public event PropertyChangedEventHandler PropertyChanged
        {
            add => _propertyChangedWeakEventManager.AddEventHandler(value);
            remove => _propertyChangedWeakEventManager.RemoveEventHandler(value);
        }

        [Test]
        public void WeakEventManagerDelegate_HandleEvent_ValidImplementation()
        {
            //Arrange
            PropertyChanged += HandleDelegateTest;
            bool didEventFire = false;

            void HandleDelegateTest(object sender, PropertyChangedEventArgs e)
            {
                Assert.IsNotNull(sender);
                Assert.AreEqual(this.GetType(), sender.GetType());

                Assert.IsNotNull(e);

                didEventFire = true;
                PropertyChanged -= HandleDelegateTest;
            }

            //Act
            _propertyChangedWeakEventManager.HandleEvent(this, new PropertyChangedEventArgs("Test"), nameof(PropertyChanged));

            //Assert
            Assert.IsTrue(didEventFire);
        }

        [Test]
        public void WeakEventManagerDelegate_HandleEvent_NullSender()
        {
            //Arrange
            PropertyChanged += HandleDelegateTest;
            bool didEventFire = false;

            void HandleDelegateTest(object sender, PropertyChangedEventArgs e)
            {
                Assert.IsNull(sender);
                Assert.IsNotNull(e);

                didEventFire = true;
                PropertyChanged -= HandleDelegateTest;
            }

            //Act
            _propertyChangedWeakEventManager.HandleEvent(null, new PropertyChangedEventArgs("Test"), nameof(PropertyChanged));

            //Assert
            Assert.IsTrue(didEventFire);
        }

        [Test]
        public void WeakEventManagerDelegate_HandleEvent_InvalidEventArgs()
        {
            //Arrange
            PropertyChanged += HandleDelegateTest;
            bool didEventFire = false;

            void HandleDelegateTest(object sender, PropertyChangedEventArgs e) => didEventFire = true;

            //Act

            //Assert
            Assert.Throws<ArgumentException>(() => _propertyChangedWeakEventManager.HandleEvent(this, EventArgs.Empty, nameof(PropertyChanged)));
            Assert.IsFalse(didEventFire);
            PropertyChanged -= HandleDelegateTest;
        }

        [Test]
        public void WeakEventManagerDelegate_HandleEvent_NullEventArgs()
        {
            //Arrange
            PropertyChanged += HandleDelegateTest;
            bool didEventFire = false;

            void HandleDelegateTest(object sender, PropertyChangedEventArgs e)
            {
                Assert.IsNotNull(sender);
                Assert.AreEqual(this.GetType(), sender.GetType());

                Assert.IsNull(e);

                didEventFire = true;
                PropertyChanged -= HandleDelegateTest;
            }

            //Act
#pragma warning disable CS8625 //Cannot convert null literal to non-nullable reference type
            _propertyChangedWeakEventManager.HandleEvent(this, null, nameof(PropertyChanged));
#pragma warning restore CS8625 //Cannot convert null literal to non-nullable reference type

            //Assert
            Assert.IsTrue(didEventFire);
        }

        [Test]
        public void WeakEventManagerDelegate_HandleEvent_InvalidHandleEventEventName()
        {
            //Arrange
            PropertyChanged += HandleDelegateTest;
            bool didEventFire = false;

            void HandleDelegateTest(object sender, PropertyChangedEventArgs e) => didEventFire = true;

            //Act
            _propertyChangedWeakEventManager.HandleEvent(this, new PropertyChangedEventArgs("Test"), nameof(TestStringEvent));

            //Assert
            Assert.False(didEventFire);
            PropertyChanged -= HandleDelegateTest;
        }

        [Test]
        public void WeakEventManagerDelegate_UnassignedEvent()
        {
            //Arrange
            bool didEventFire = false;

            PropertyChanged += HandleDelegateTest;
            PropertyChanged -= HandleDelegateTest;
            void HandleDelegateTest(object sender, PropertyChangedEventArgs e) => didEventFire = true;

            //Act
#pragma warning disable CS8625 //Cannot convert null literal to non-nullable reference type
            _propertyChangedWeakEventManager.HandleEvent(null, null, nameof(PropertyChanged));
#pragma warning restore CS8625 //Cannot convert null literal to non-nullable reference type

            //Assert
            Assert.IsFalse(didEventFire);
        }

        [Test]
        public void WeakEventManagerDelegate_UnassignedEventManager()
        {
            //Arrange
            var unassignedEventManager = new WeakEventManager();
            bool didEventFire = false;

            PropertyChanged += HandleDelegateTest;
            void HandleDelegateTest(object sender, PropertyChangedEventArgs e) => didEventFire = true;

            //Act
            unassignedEventManager.HandleEvent(null, null, nameof(PropertyChanged));

            //Assert
            Assert.IsFalse(didEventFire);
            PropertyChanged -= HandleDelegateTest;
        }

        [Test]
        public void WeakEventManagerDelegate_HandleEvent_InvalidHandleEvent()
        {
            //Arrange
            PropertyChanged += HandleDelegateTest;
            bool didEventFire = false;

            void HandleDelegateTest(object sender, PropertyChangedEventArgs e) => didEventFire = true;

            //Act

            //Assert
            Assert.Throws<InvalidHandleEventException>(() => _propertyChangedWeakEventManager.HandleEvent(nameof(PropertyChanged)));
            Assert.IsFalse(didEventFire);
            PropertyChanged -= HandleDelegateTest;
        }

        [Test]
        public void WeakEventManagerDelegate_AddEventHandler_NullHandler()
        {
            //Arrange

            //Act

            //Assert
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference
            Assert.Throws<ArgumentNullException>(() => _propertyChangedWeakEventManager.AddEventHandler(null), "Value cannot be null.\nParameter name: handler");
#pragma warning restore CS8625
        }

        [Test]
        public void WeakEventManagerDelegate_AddEventHandler_NullEventName()
        {
            //Arrange

            //Act

            //Assert
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference
            Assert.Throws<ArgumentNullException>(() => _propertyChangedWeakEventManager.AddEventHandler(null, null), "Value cannot be null.\nParameter name: eventName");
#pragma warning restore CS8625
        }

        [Test]
        public void WeakEventManagerDelegate_AddEventHandler_EmptyEventName()
        {
            //Arrange

            //Act

            //Assert
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference
            Assert.Throws<ArgumentNullException>(() => _propertyChangedWeakEventManager.AddEventHandler(null, string.Empty), "Value cannot be null.\nParameter name: eventName");
#pragma warning restore CS8625
        }

        [Test]
        public void WeakEventManagerDelegate_AddEventHandler_WhitespaceEventName()
        {
            //Arrange

            //Act

            //Assert
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference
            Assert.Throws<ArgumentNullException>(() => _propertyChangedWeakEventManager.AddEventHandler(null, " "), "Value cannot be null.\nParameter name: eventName");
#pragma warning restore CS8625
        }

        [Test]
        public void WeakEventManagerDelegate_RemoveventHandler_NullHandler()
        {
            //Arrange

            //Act

            //Assert
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference
            Assert.Throws<ArgumentNullException>(() => _propertyChangedWeakEventManager.RemoveEventHandler(null), "Value cannot be null.\nParameter name: handler");
#pragma warning restore CS8625 
        }

        [Test]
        public void WeakEventManagerDelegate_RemoveventHandler_NullEventName()
        {
            //Arrange

            //Act

            //Assert
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference
            Assert.Throws<ArgumentNullException>(() => _propertyChangedWeakEventManager.RemoveEventHandler(null, null), "Value cannot be null.\nParameter name: eventName");
#pragma warning restore CS8625
        }

        [Test]
        public void WeakEventManagerDelegate_RemoveventHandler_EmptyEventName()
        {
            //Arrange

            //Act

            //Assert
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference
            Assert.Throws<ArgumentNullException>(() => _propertyChangedWeakEventManager.RemoveEventHandler(null, string.Empty), "Value cannot be null.\nParameter name: eventName");
#pragma warning restore CS8625
        }

        [Test]
        public void WeakEventManagerDelegate_RemoveventHandler_WhiteSpaceEventName()
        {
            //Arrange

            //Act

            //Assert
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference
            Assert.Throws<ArgumentNullException>(() => _propertyChangedWeakEventManager.RemoveEventHandler(null, " "), "Value cannot be null.\nParameter name: eventName");
#pragma warning restore CS8625
        }
    }
}
