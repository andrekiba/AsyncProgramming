﻿using System;
using NUnit.Framework;

namespace AsyncAwaitBestPractices.UnitTests
{
    class Tests_WeakEventManager_EventHandler : BaseTest
    {
        [Test]
        public void WeakEventManager_HandleEvent_ValidImplementation()
        {
            //Arrange
            TestEvent += HandleTestEvent;
            bool didEventFire = false;

            void HandleTestEvent(object? sender, EventArgs e)
            {
                if (sender is null)
                    throw new ArgumentNullException(nameof(sender));

                Assert.IsNotNull(sender);
                Assert.AreEqual(this.GetType(), sender.GetType());

                Assert.IsNotNull(e);

                didEventFire = true;
                TestEvent -= HandleTestEvent;
            }

            //Act
            TestWeakEventManager.HandleEvent(this, new EventArgs(), nameof(TestEvent));

            //Assert
            Assert.IsTrue(didEventFire);
        }

        [Test]
        public void WeakEventManager_HandleEvent_NullSender()
        {
            //Arrange
            TestEvent += HandleTestEvent;
            bool didEventFire = false;

            void HandleTestEvent(object? sender, EventArgs e)
            {
                Assert.IsNull(sender);
                Assert.IsNotNull(e);

                didEventFire = true;
                TestEvent -= HandleTestEvent;
            }

            //Act
            TestWeakEventManager.HandleEvent(null, new EventArgs(), nameof(TestEvent));

            //Assert
            Assert.IsTrue(didEventFire);
        }

        [Test]
        public void WeakEventManager_HandleEvent_EmptyEventArgs()
        {
            //Arrange
            TestEvent += HandleTestEvent;
            bool didEventFire = false;

            void HandleTestEvent(object? sender, EventArgs e)
            {
                if (sender is null)
                    throw new ArgumentNullException(nameof(sender));

                Assert.IsNotNull(sender);
                Assert.AreEqual(this.GetType(), sender.GetType());

                Assert.IsNotNull(e);
                Assert.AreEqual(EventArgs.Empty, e);

                didEventFire = true;
                TestEvent -= HandleTestEvent;
            }

            //Act
            TestWeakEventManager.HandleEvent(this, EventArgs.Empty, nameof(TestEvent));

            //Assert
            Assert.IsTrue(didEventFire);
        }

        [Test]
        public void WeakEventManager_HandleEvent_NullEventArgs()
        {
            //Arrange
            TestEvent += HandleTestEvent;
            bool didEventFire = false;

            void HandleTestEvent(object? sender, EventArgs e)
            {
                if (sender is null)
                    throw new ArgumentNullException(nameof(sender));

                Assert.IsNotNull(sender);
                Assert.AreEqual(this.GetType(), sender.GetType());

                Assert.IsNull(e);

                didEventFire = true;
                TestEvent -= HandleTestEvent;
            }

            //Act
#pragma warning disable CS8625 //Cannot convert null literal to non-nullable reference type
            TestWeakEventManager.HandleEvent(this, null, nameof(TestEvent));
#pragma warning restore CS8625

            //Assert
            Assert.IsTrue(didEventFire);
        }

        [Test]
        public void WeakEventManager_HandleEvent_InvalidHandleEventName()
        {
            //Arrange
            TestEvent += HandleTestEvent;
            bool didEventFire = false;

            void HandleTestEvent(object? sender, EventArgs e) => didEventFire = true;

            //Act
            TestWeakEventManager.HandleEvent(this, new EventArgs(), nameof(TestStringEvent));

            //Assert
            Assert.False(didEventFire);
            TestEvent -= HandleTestEvent;
        }

        [Test]
        public void WeakEventManager_UnassignedEvent()
        {
            //Arrange
            bool didEventFire = false;

            TestEvent += HandleTestEvent;
            TestEvent -= HandleTestEvent;
            void HandleTestEvent(object? sender, EventArgs e) => didEventFire = true;

            //Act
            TestWeakEventManager.HandleEvent(null, null, nameof(TestEvent));

            //Assert
            Assert.IsFalse(didEventFire);
        }

        [Test]
        public void WeakEventManager_UnassignedEventManager()
        {
            //Arrange
            var unassignedEventManager = new WeakEventManager();
            bool didEventFire = false;

            TestEvent += HandleTestEvent;
            void HandleTestEvent(object? sender, EventArgs e) => didEventFire = true;

            //Act
            unassignedEventManager.HandleEvent(null, null, nameof(TestEvent));

            //Assert
            Assert.IsFalse(didEventFire);
            TestEvent -= HandleTestEvent;
        }

        [Test]
        public void WeakEventManager_AddEventHandler_NullHandler()
        {
            //Arrange

            //Act

            //Assert
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference
            Assert.Throws<ArgumentNullException>(() => TestWeakEventManager.AddEventHandler(null), "Value cannot be null.\nParameter name: handler");
#pragma warning restore CS8625
        }

        [Test]
        public void WeakEventManager_AddEventHandler_NullEventName()
        {
            //Arrange

            //Act

            //Assert
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference
            Assert.Throws<ArgumentNullException>(() => TestWeakEventManager.AddEventHandler(null, null), "Value cannot be null.\nParameter name: eventName");
#pragma warning restore CS8625
        }

        [Test]
        public void WeakEventManager_AddEventHandler_EmptyEventName()
        {
            //Arrange

            //Act

            //Assert
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference
            Assert.Throws<ArgumentNullException>(() => TestWeakEventManager.AddEventHandler(null, string.Empty), "Value cannot be null.\nParameter name: eventName");
#pragma warning restore CS8625
        }

        [Test]
        public void WeakEventManager_AddEventHandler_WhitespaceEventName()
        {
            //Arrange

            //Act

            //Assert
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference
            Assert.Throws<ArgumentNullException>(() => TestWeakEventManager.AddEventHandler(null, " "), "Value cannot be null.\nParameter name: eventName");
#pragma warning restore CS8625
        }

        [Test]
        public void WeakEventManager_RemoveventHandler_NullHandler()
        {
            //Arrange

            //Act

            //Assert
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference
            Assert.Throws<ArgumentNullException>(() => TestWeakEventManager.RemoveEventHandler(null), "Value cannot be null.\nParameter name: handler");
#pragma warning restore CS8625 
        }

        [Test]
        public void WeakEventManager_RemoveventHandler_NullEventName()
        {
            //Arrange

            //Act

            //Assert
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference
            Assert.Throws<ArgumentNullException>(() => TestWeakEventManager.RemoveEventHandler(null, null), "Value cannot be null.\nParameter name: eventName");
#pragma warning restore CS8625 
        }

        [Test]
        public void WeakEventManager_RemoveventHandler_EmptyEventName()
        {
            //Arrange

            //Act

            //Assert
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference
            Assert.Throws<ArgumentNullException>(() => TestWeakEventManager.RemoveEventHandler(null, string.Empty), "Value cannot be null.\nParameter name: eventName");
#pragma warning restore CS8625
        }

        [Test]
        public void WeakEventManager_RemoveventHandler_WhiteSpaceEventName()
        {
            //Arrange

            //Act

            //Assert
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference
            Assert.Throws<ArgumentNullException>(() => TestWeakEventManager.RemoveEventHandler(null, " "), "Value cannot be null.\nParameter name: eventName");
#pragma warning restore CS8625
        }

        [Test]
        public void WeakEventManager_HandleEvent_InvalidHandleEvent()
        {
            //Arrange
            TestEvent += HandleTestEvent;
            bool didEventFire = false;

            void HandleTestEvent(object? sender, EventArgs e) => didEventFire = true;

            //Act

            //Assert
            Assert.Throws<InvalidHandleEventException>(() => TestWeakEventManager.HandleEvent(nameof(TestEvent)));
            Assert.IsFalse(didEventFire);
            TestEvent -= HandleTestEvent;
        }
    }
}
