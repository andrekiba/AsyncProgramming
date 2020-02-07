﻿using System;
using NUnit.Framework;

namespace AsyncAwaitBestPractices.UnitTests
{
    class Tests_WeakEventManager_EventHandlerT : BaseTest
    {
        [Test]
        public void WeakEventManagerTEventArgs_HandleEvent_ValidImplementation()
        {
            //Arrange
            TestStringEvent += HandleTestEvent;

            const string stringEventArg = "Test";
            bool didEventFire = false;

            void HandleTestEvent(object? sender, string? e)
            {
                if (sender is null || e is null)
                    throw new ArgumentNullException(nameof(sender));

                Assert.IsNotNull(sender);
                Assert.AreEqual(this.GetType(), sender.GetType());

                Assert.IsNotNull(e);
                Assert.AreEqual(stringEventArg, e);

                didEventFire = true;
                TestStringEvent -= HandleTestEvent;
            }

            //Act
            TestStringWeakEventManager.HandleEvent(this, stringEventArg, nameof(TestStringEvent));

            //Assert
            Assert.IsTrue(didEventFire);
        }

        [Test]
        public void WeakEventManageTEventArgs_HandleEvent_NullSender()
        {
            //Arrange
            TestStringEvent += HandleTestEvent;

            const string stringEventArg = "Test";

            bool didEventFire = false;

            void HandleTestEvent(object? sender, string e)
            {
                Assert.IsNull(sender);

                Assert.IsNotNull(e);
                Assert.AreEqual(stringEventArg, e);

                didEventFire = true;
                TestStringEvent -= HandleTestEvent;
            }

            //Act
            TestStringWeakEventManager.HandleEvent(null, stringEventArg, nameof(TestStringEvent));

            //Assert
            Assert.IsTrue(didEventFire);
        }

        [Test]
        public void WeakEventManagerTEventArgs_HandleEvent_NullEventArgs()
        {
            //Arrange
            TestStringEvent += HandleTestEvent;
            bool didEventFire = false;

            void HandleTestEvent(object? sender, string e)
            {
                if (sender is null)
                    throw new ArgumentNullException(nameof(sender));

                Assert.IsNotNull(sender);
                Assert.AreEqual(this.GetType(), sender.GetType());

                Assert.IsNull(e);

                didEventFire = true;
                TestStringEvent -= HandleTestEvent;
            }

            //Act
#pragma warning disable CS8625 //Cannot convert null literal to non-nullable reference type
            TestStringWeakEventManager.HandleEvent(this, null, nameof(TestStringEvent));
#pragma warning restore CS8625

            //Assert
            Assert.IsTrue(didEventFire);
        }

        [Test]
        public void WeakEventManagerTEventArgs_HandleEvent_InvalidHandleEvent()
        {
            //Arrange
            TestStringEvent += HandleTestEvent;

            bool didEventFire = false;

            void HandleTestEvent(object? sender, string e) => didEventFire = true;

            //Act
            TestStringWeakEventManager.HandleEvent(this, "Test", nameof(TestEvent));

            //Assert
            Assert.False(didEventFire);
            TestStringEvent -= HandleTestEvent;
        }

        [Test]
        public void WeakEventManager_NullEventManager()
        {
            //Arrange
            WeakEventManager? unassignedEventManager = null;

            //Act

            //Assert
#pragma warning disable CS8602 //Dereference of a possible null reference
            Assert.Throws<NullReferenceException>(() => unassignedEventManager.HandleEvent(null, null, nameof(TestEvent)));
#pragma warning restore CS8602
        }

        [Test]
        public void WeakEventManagerTEventArgs_UnassignedEventManager()
        {
            //Arrange
            var unassignedEventManager = new WeakEventManager<string>();
            bool didEventFire = false;

            TestStringEvent += HandleTestEvent;
            void HandleTestEvent(object? sender, string e) => didEventFire = true;

            //Act
#pragma warning disable CS8625 //Cannot convert null literal to non-nullable reference type
            unassignedEventManager.HandleEvent(null, null, nameof(TestStringEvent));
#pragma warning restore CS8625

            //Assert
            Assert.IsFalse(didEventFire);
            TestStringEvent -= HandleTestEvent;
        }

        [Test]
        public void WeakEventManagerTEventArgs_UnassignedEvent()
        {
            //Arrange
            bool didEventFire = false;

            TestStringEvent += HandleTestEvent;
            TestStringEvent -= HandleTestEvent;
            void HandleTestEvent(object? sender, string e) => didEventFire = true;

            //Act
            TestStringWeakEventManager.HandleEvent(this, "Test", nameof(TestStringEvent));

            //Assert
            Assert.IsFalse(didEventFire);
        }

        [Test]
        public void WeakEventManagerT_AddEventHandler_NullHandler()
        {
            //Arrange

            //Act

            //Assert
#pragma warning disable CS8625 //Cannot convert null literal to non-nullable reference type
            Assert.Throws<ArgumentNullException>(() => TestStringWeakEventManager.AddEventHandler((EventHandler<string>?)null), "Value cannot be null.\nParameter name: handler");
#pragma warning restore CS8625
        }

        [Test]
        public void WeakEventManagerT_AddEventHandler_NullEventName()
        {
            //Arrange

            //Act

            //Assert
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference
            Assert.Throws<ArgumentNullException>(() => TestStringWeakEventManager.AddEventHandler(s => { var temp = s; }, null), "Value cannot be null.\nParameter name: eventName");
#pragma warning restore CS8625
        }

        [Test]
        public void WeakEventManagerT_AddEventHandler_EmptyEventName()
        {
            //Arrange

            //Act

            //Assert
            Assert.Throws<ArgumentNullException>(() => TestStringWeakEventManager.AddEventHandler(s => { var temp = s; }, string.Empty), "Value cannot be null.\nParameter name: eventName");
        }

        [Test]
        public void WeakEventManagerT_AddEventHandler_WhiteSpaceEventName()
        {
            //Arrange

            //Act

            //Assert
            Assert.Throws<ArgumentNullException>(() => TestStringWeakEventManager.AddEventHandler(s => { var temp = s; }, " "), "Value cannot be null.\nParameter name: eventName");
        }

        [Test]
        public void WeakEventManagerT_RemoveventHandler_NullHandler()
        {
            //Arrange

            //Act

            //Assert
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference
            Assert.Throws<ArgumentNullException>(() => TestStringWeakEventManager.RemoveEventHandler((EventHandler<string>?)null), "Value cannot be null.\nParameter name: handler");
#pragma warning restore CS8625
        }


        [Test]
        public void WeakEventManagerT_RemoveventHandler_NullEventName()
        {
            //Arrange

            //Act

            //Assert
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference
            Assert.Throws<ArgumentNullException>(() => TestStringWeakEventManager.AddEventHandler(s => { var temp = s; }, null), "Value cannot be null.\nParameter name: eventName");
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference
        }

        [Test]
        public void WeakEventManagerT_RemoveventHandler_EmptyEventName()
        {
            //Arrange

            //Act

            //Assert
            Assert.Throws<ArgumentNullException>(() => TestStringWeakEventManager.AddEventHandler(s => { var temp = s; }, string.Empty), "Value cannot be null.\nParameter name: eventName");
        }

        [Test]
        public void WeakEventManagerT_RemoveventHandler_WhiteSpaceEventName()
        {
            //Arrange

            //Act

            //Assert
            Assert.Throws<ArgumentNullException>(() => TestStringWeakEventManager.AddEventHandler(s => { var temp = s; }, string.Empty), "Value cannot be null.\nParameter name: eventName");
        }

        [Test]
        public void WeakEventManagerT_HandleEvent_InvalidHandleEvent()
        {
            //Arrange
            TestStringEvent += HandleTestStringEvent;
            bool didEventFire = false;

            void HandleTestStringEvent(object? sender, string e) => didEventFire = true;

            //Act

            //Assert
            Assert.Throws<InvalidHandleEventException>(() => TestStringWeakEventManager.HandleEvent("", nameof(TestStringEvent)));
            Assert.IsFalse(didEventFire);
            TestStringEvent -= HandleTestStringEvent;
        }
    }
}
