﻿using System;
using System.Threading.Tasks;

namespace AsyncAwaitBestPractices.UnitTests
{
    abstract class BaseValueTaskTest : BaseTest
    {
        protected new ValueTask NoParameterTask() => ValueTaskDelay(Delay);
        protected new ValueTask IntParameterTask(int delay) => ValueTaskDelay(delay);
        protected new ValueTask StringParameterTask(string text) => ValueTaskDelay(Delay);
        protected new ValueTask NoParameterImmediateNullReferenceExceptionTask() => throw new NullReferenceException();
        protected new ValueTask ParameterImmediateNullReferenceExceptionTask(int delay) => throw new NullReferenceException();

        protected async ValueTask ValueTaskDelay(int delay) => await Task.Delay(delay);

        protected new async ValueTask NoParameterDelayedNullReferenceExceptionTask()
        {
            await Task.Delay(Delay);
            throw new NullReferenceException();
        }

        protected new async ValueTask IntParameterDelayedNullReferenceExceptionTask(int delay)
        {
            await Task.Delay(delay);
            throw new NullReferenceException();
        }
    }
}
