﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Diagnostics.Runtime.Tests
{
    [TestClass]
    public class ValueTests
    {
        [TestMethod]
        public void PrimitiveVariableConversionTest()
        {
            using (DataTarget dt = TestTargets.LocalVariables.LoadFullDump())
            {
                ClrRuntime runtime = dt.ClrVersions.Single().CreateRuntime();
                ClrThread thread = runtime.GetMainThread();


                ClrStackFrame frame = thread.GetFrame("Inner");

                ClrValue value = frame.GetLocal("b");
                Assert.AreEqual(true, value.AsBoolean());

                value = frame.GetLocal("c");
                Assert.AreEqual('c', value.AsChar());

                value = frame.GetLocal("s");
                Assert.AreEqual("hello world", value.AsString());


                frame = thread.GetFrame("Middle");

                value = frame.GetLocal("b");
                Assert.AreEqual(0x42, value.AsByte());
                
                value = frame.GetLocal("sb");
                Assert.AreEqual(0x43, value.AsSByte());

                value = frame.GetLocal("sh");
                Assert.AreEqual(0x4242, value.AsInt16());

                value = frame.GetLocal("ush");
                Assert.AreEqual(0x4243, value.AsUInt16());

                value = frame.GetLocal("i");
                Assert.AreEqual(0x42424242, value.AsInt32());

                value = frame.GetLocal("ui");
                Assert.AreEqual(0x42424243u, value.AsUInt32());


                frame = thread.GetFrame("Outer");

                value = frame.GetLocal("f");
                Assert.AreEqual(42.0f, value.AsFloat());

                value = frame.GetLocal("d");
                Assert.AreEqual(43.0, value.AsDouble());

                value = frame.GetLocal("ptr");
                Assert.AreEqual(new IntPtr(0x42424242), value.AsIntPtr());

                value = frame.GetLocal("uptr");
                Assert.AreEqual(new UIntPtr(0x43434343), value.AsUIntPtr());
            }
        }

        [TestMethod]
        public void ObjectVariableTest()
        {
            using (DataTarget dt = TestTargets.LocalVariables.LoadFullDump())
            {
                ClrRuntime runtime = dt.ClrVersions.Single().CreateRuntime();
                ClrHeap heap = runtime.GetHeap();
                ClrThread thread = runtime.GetMainThread();
                ClrStackFrame frame = thread.GetFrame("Main");

                ClrValue value = frame.GetLocal("o");
                ClrObject obj = value.AsObject();
                Assert.IsTrue(obj.IsValid);
                Assert.IsFalse(obj.IsNull);
                Assert.AreEqual("Foo", obj.Type.Name);
                Assert.AreSame(obj.Type, value.Type);
                Assert.AreSame(heap.GetObjectType(obj.Address), value.Type);
            }
        }



        [TestMethod]
        public void GetFieldTests()
        {
            using (DataTarget dt = TestTargets.LocalVariables.LoadFullDump())
            {
                ClrRuntime runtime = dt.ClrVersions.Single().CreateRuntime();
                ClrHeap heap = runtime.GetHeap();
                ClrThread thread = runtime.GetMainThread();
                ClrStackFrame frame = thread.GetFrame("Main");

                ClrValue value = frame.GetLocal("o");
                Assert.AreEqual("Foo", value.Type.Name);  // Ensure we have the right object.

                ClrObject obj = value.AsObject();
                Assert.IsTrue(obj.GetBooleanField("b"));

                ClrValue val = obj.GetField("st").GetField("middle").GetField("inner");
                Assert.IsTrue(val.GetField("b").AsBoolean());
                Assert.IsTrue(val.GetBooleanField("b"));
                
                obj = obj.GetField("st").GetField("middle").GetObjectField("inner");
                Assert.IsTrue(obj.GetField("b").AsBoolean());
                Assert.IsTrue(obj.GetBooleanField("b"));
            }
        }

        [TestMethod]
        public void StructVariableTest()
        {
            using (DataTarget dt = TestTargets.LocalVariables.LoadFullDump())
            {
                ClrRuntime runtime = dt.ClrVersions.Single().CreateRuntime();
                ClrHeap heap = runtime.GetHeap();
                ClrThread thread = runtime.GetMainThread();
                ClrStackFrame frame = thread.GetFrame("Main");

                ClrValue value = frame.GetLocal("s");
                Assert.AreEqual("Struct", value.Type.Name);

                CheckStruct(value);
            }
        }

        private static void CheckStruct(ClrValue value)
        {
            Assert.AreEqual(42, value.GetField("i").AsInt32());
            Assert.AreEqual("string", value.GetField("s").AsString());
            Assert.AreEqual(true, value.GetField("b").AsBoolean());
            Assert.AreEqual(4.2f, value.GetField("f").AsFloat());
            Assert.AreEqual(8.4, value.GetField("d").AsDouble());
            Assert.AreEqual("System.Object", value.GetField("o").AsObject().Type.Name);
        }

        [TestMethod]
        public void InteriorStructTest()
        {
            using (DataTarget dt = TestTargets.LocalVariables.LoadFullDump())
            {
                ClrRuntime runtime = dt.ClrVersions.Single().CreateRuntime();
                ClrHeap heap = runtime.GetHeap();
                ClrThread thread = runtime.GetMainThread();
                ClrStackFrame frame = thread.GetFrame("Main");

                ClrValue value = frame.GetLocal("s");
                Assert.AreEqual("Struct", value.Type.Name);
                CheckStruct(value);

                value = value.GetField("middle");
                CheckStruct(value);

                value = value.GetField("inner");
                CheckStruct(value);
            }
        }
    }
}