﻿using System;
using NUnit.Framework;

namespace AsyncAwaitBestPractices.UnitTests
{
    class Tests_WeakEventManager_Action : BaseTest
    {
        readonly WeakEventManager _actionEventManager = new WeakEventManager();

        public event Action ActionEvent
        {
            add => _actionEventManager.AddEventHandler(value);
            remove => _actionEventManager.RemoveEventHandler(value);
        }

        [Test]
        public void WeakEventManagerAction_HandleEvent_ValidImplementation()
        {
            //Arrange
            ActionEvent += HandleDelegateTest;
            bool didEventFire = false;

            void HandleDelegateTest()
            {
                didEventFire = true;
                ActionEvent -= HandleDelegateTest;
            }

            //Act
            _actionEventManager.HandleEvent(nameof(ActionEvent));

            //Assert
            Assert.IsTrue(didEventFire);
        }

        [Test]
        public void WeakEventManagerAction_HandleEvent_InvalidHandleEventEventName()
        {
            //Arrange
            ActionEvent += HandleDelegateTest;
            bool didEventFire = false;

            void HandleDelegateTest() => didEventFire = true;

            //Act
            _actionEventManager.HandleEvent(nameof(TestStringEvent));

            //Assert
            Assert.False(didEventFire);
            ActionEvent -= HandleDelegateTest;
        }

        [Test]
        public void WeakEventManagerAction_UnassignedEvent()
        {
            //Arrange
            bool didEventFire = false;

            ActionEvent += HandleDelegateTest;
            ActionEvent -= HandleDelegateTest;
            void HandleDelegateTest() => didEventFire = true;

            //Act
            _actionEventManager.HandleEvent(nameof(ActionEvent));

            //Assert
            Assert.IsFalse(didEventFire);
        }

        [Test]
        public void WeakEventManagerAction_UnassignedEventManager()
        {
            //Arrange
            var unassignedEventManager = new WeakEventManager();
            bool didEventFire = false;

            ActionEvent += HandleDelegateTest;
            void HandleDelegateTest() => didEventFire = true;

            //Act
            unassignedEventManager.HandleEvent(nameof(ActionEvent));

            //Assert
            Assert.IsFalse(didEventFire);
            ActionEvent -= HandleDelegateTest;
        }

        [Test]
        public void WeakEventManagerAction_HandleEvent_InvalidHandleEvent()
        {
            //Arrange
            ActionEvent += HandleDelegateTest;
            bool didEventFire = false;

            void HandleDelegateTest() => didEventFire = true;

            //Act

            //Assert
            Assert.Throws<InvalidHandleEventException>(() => _actionEventManager.HandleEvent(this, EventArgs.Empty, nameof(ActionEvent)));
            Assert.IsFalse(didEventFire);
            ActionEvent -= HandleDelegateTest;
        }

        [Test]
        public void WeakEventManagerAction_AddEventHandler_NullHandler()
        {
            //Arrange

            //Act

            //Assert
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference
            Assert.Throws<ArgumentNullException>(() => _actionEventManager.AddEventHandler(null), "Value cannot be null.\nParameter name: handler");
#pragma warning restore CS8625 
        }

        [Test]
        public void WeakEventManagerAction_AddEventHandler_NullEventName()
        {
            //Arrange

            //Act

            //Assert
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference
            Assert.Throws<ArgumentNullException>(() => _actionEventManager.AddEventHandler(null, null), "Value cannot be null.\nParameter name: eventName");
#pragma warning restore CS8625
        }

        [Test]
        public void WeakEventManagerAction_AddEventHandler_EmptyEventName()
        {
            //Arrange

            //Act

            //Assert
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference
            Assert.Throws<ArgumentNullException>(() => _actionEventManager.AddEventHandler(null, string.Empty), "Value cannot be null.\nParameter name: eventName");
#pragma warning restore CS8625
        }

        [Test]
        public void WeakEventManagerAction_AddEventHandler_WhitespaceEventName()
        {
            //Arrange

            //Act

            //Assert
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference
            Assert.Throws<ArgumentNullException>(() => _actionEventManager.AddEventHandler(null, " "), "Value cannot be null.\nParameter name: eventName");
#pragma warning restore CS8625
        }

        [Test]
        public void WeakEventManagerAction_RemoveventHandler_NullHandler()
        {
            //Arrange

            //Act

            //Assert
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference
            Assert.Throws<ArgumentNullException>(() => _actionEventManager.RemoveEventHandler(null), "Value cannot be null.\nParameter name: handler");
#pragma warning restore CS8625
        }

        [Test]
        public void WeakEventManagerAction_RemoveventHandler_NullEventName()
        {
            //Arrange

            //Act

            //Assert
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference
            Assert.Throws<ArgumentNullException>(() => _actionEventManager.RemoveEventHandler(null, null), "Value cannot be null.\nParameter name: eventName");
#pragma warning restore CS8625
        }

        [Test]
        public void WeakEventManagerAction_RemoveventHandler_EmptyEventName()
        {
            //Arrange

            //Act

            //Assert
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference
            Assert.Throws<ArgumentNullException>(() => _actionEventManager.RemoveEventHandler(null, string.Empty), "Value cannot be null.\nParameter name: eventName");
#pragma warning restore CS8625
        }

        [Test]
        public void WeakEventManagerAction_RemoveventHandler_WhiteSpaceEventName()
        {
            //Arrange

            //Act

            //Assert
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference
            Assert.Throws<ArgumentNullException>(() => _actionEventManager.RemoveEventHandler(null, " "), "Value cannot be null.\nParameter name: eventName");
#pragma warning restore CS8625
        }
    }
}
