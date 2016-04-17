﻿using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Diagnostics;

namespace iMobileDevice.Tests
{
    [TestClass]
    public class NativeStringArrayMarshalerTests
    {
        [TestMethod]
        public void TestRoundTrip()
        {
            // Create a string array worth +/- 10 MB of memory
            var values = new List<string>();
            values.Add(new string('0', 1024 * 1024));
            values.Add(new string('1', 1024 * 1024));
            values.Add(new string('2', 1024 * 1024));
            values.Add(new string('3', 1024 * 1024));
            values.Add(new string('4', 1024 * 1024));
            values.Add(new string('5', 1024 * 1024));
            values.Add(new string('6', 1024 * 1024));
            values.Add(new string('7', 1024 * 1024));
            values.Add(new string('8', 1024 * 1024));
            values.Add(new string('9', 1024 * 1024));

            var readonlyValues = new ReadOnlyCollection<string>(values);
            NativeStringArrayMarshaler marshaler = new NativeStringArrayMarshaler();

            GC.Collect();
            var p = Process.GetCurrentProcess();
            var initialMemory = p.PrivateMemorySize64;

            for (int i = 0; i < 75; i++)
            {
                var pointer = marshaler.MarshalManagedToNative(readonlyValues);
                var roundTrip = marshaler.MarshalNativeToManaged(pointer);
                marshaler.CleanUpNativeData(pointer);
            }

            GC.Collect();
            p.Refresh();

            var currentMemory = p.PrivateMemorySize64;
            var delta = currentMemory - initialMemory;

            // If more than 10 MB was leaked, set off the alarm bells
            // You can verify this works by commenting out NativeStringMarshaler.CleanUpNativeData
            if (delta > 10 * 1024 * 1024 * 8 /* 10 MB */)
            {
                Assert.Fail("Memory was leaked");
            }
        }
    }
}
