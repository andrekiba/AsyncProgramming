﻿using System;
using System.Threading.Tasks;

using NUnit.Framework;

using AsyncAwaitBestPractices.MVVM;

namespace AsyncAwaitBestPractices.UnitTests
{
    class Tests_AsyncValueCommand : BaseValueTaskTest
    {
        [Test]
        public void AsyncValueCommandNullExecuteParameter()
        {
            //Arrange

            //Act

            //Assert
#pragma warning disable CS8625 //Cannot convert null literal to non-nullable reference type
            Assert.Throws<ArgumentNullException>(() => new AsyncValueCommand(null));
#pragma warning restore CS8625
        }

        [Test]
        public void AsyncValueCommandT_NullExecuteParameter()
        {
            //Arrange

            //Act

            //Assert
#pragma warning disable CS8625 //Cannot convert null literal to non-nullable reference type
            Assert.Throws<ArgumentNullException>(() => new AsyncValueCommand<object>(null));
#pragma warning restore CS8625
        }

        [TestCase(500)]
        [TestCase(default)]
        public async Task AsyncValueCommandExecuteAsync_IntParameter_Test(int parameter)
        {
            //Arrange
            AsyncValueCommand<int> command = new AsyncValueCommand<int>(IntParameterTask);

            //Act
            await command.ExecuteAsync(parameter);

            //Assert

        }

        [TestCase("Hello")]
        [TestCase(default)]
        public async Task AsyncValueCommandExecuteAsync_StringParameter_Test(string parameter)
        {
            //Arrange
            AsyncValueCommand<string> command = new AsyncValueCommand<string>(StringParameterTask);

            //Act
            await command.ExecuteAsync(parameter);

            //Assert

        }

        [Test]
        public void AsyncValueCommandParameter_CanExecuteTrue_Test()
        {
            //Arrange
            AsyncValueCommand<int> command = new AsyncValueCommand<int>(IntParameterTask, CanExecuteTrue);

            //Act

            //Assert

            Assert.IsTrue(command.CanExecute(null));
        }

        [Test]
        public void AsyncValueCommandParameter_CanExecuteFalse_Test()
        {
            //Arrange
            AsyncValueCommand<int> command = new AsyncValueCommand<int>(IntParameterTask, CanExecuteFalse);

            //Act

            //Assert
            Assert.False(command.CanExecute(null));
        }

        [Test]
        public void AsyncValueCommandNoParameter_CanExecuteTrue_Test()
        {
            //Arrange
            AsyncValueCommand command = new AsyncValueCommand(NoParameterTask, CanExecuteTrue);

            //Act

            //Assert
            Assert.IsTrue(command.CanExecute(null));
        }

        [Test]
        public void AsyncValueCommandNoParameter_CanExecuteFalse_Test()
        {
            //Arrange
            AsyncValueCommand command = new AsyncValueCommand(NoParameterTask, CanExecuteFalse);

            //Act

            //Assert
            Assert.False(command.CanExecute(null));
        }


        [Test]
        public void AsyncValueCommandCanExecuteChanged_Test()
        {
            //Arrange
            bool canCommandExecute = false;
            bool didCanExecuteChangeFire = false;

            AsyncValueCommand command = new AsyncValueCommand(NoParameterTask, commandCanExecute);
            command.CanExecuteChanged += handleCanExecuteChanged;

            void handleCanExecuteChanged(object? sender, EventArgs e) => didCanExecuteChangeFire = true;
            bool commandCanExecute(object? parameter) => canCommandExecute;

            Assert.False(command.CanExecute(null));

            //Act
            canCommandExecute = true;

            //Assert
            Assert.True(command.CanExecute(null));
            Assert.False(didCanExecuteChangeFire);

            //Act
            command.RaiseCanExecuteChanged();

            //Assert
            Assert.True(didCanExecuteChangeFire);
            Assert.True(command.CanExecute(null));
        }
    }
}
