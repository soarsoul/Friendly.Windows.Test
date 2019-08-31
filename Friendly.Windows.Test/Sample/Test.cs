using System.Collections.Generic;
using Codeer.Friendly.Dynamic;
using Codeer.Friendly.Windows;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using Codeer.Friendly;

namespace Sample
{
    [TestClass]
    public class Test
    {
        WindowsAppFriend _app;
    
        [TestInitialize]
        public void TestInitialize()
        {
            //attach to target process!
            var path = Path.GetFullPath("../../../Target/bin/Debug/Target.exe");
            _app = new WindowsAppFriend(Process.Start(path));
        }

        [TestCleanup]
        public void TestCleanup()
        {
            Process process = Process.GetProcessById(_app.ProcessId);
            _app.Dispose();
            process.CloseMainWindow();
        }

        [TestMethod]
        public void Manipulate()
        {
            //static method
            //PresentationFramework.dll need add otherwise Application can't use
            dynamic window = _app.Type<Application>().Current.MainWindow;

            //instance method
            string value = window.MyFunc(5);
            Assert.AreEqual("5", value);

            //instance property
            window.DataContext.TextData = "abc";

            //instance field
            string text = window._textBox.Text;
            Assert.AreEqual("abc",text);

            //new instance in target process
            var addText = _app.Type<TextBox>()();
            //reference to target process
            window.Content.Children.Add(addText);
        }

        [TestMethod]
        public void AsyncTest()
        {
            dynamic window = _app.Type<Application>().Current.MainWindow;

            var async = new Async();
            var text = window.MyFunc(async, 5);

            //you can check whether it has completed.
            if (async.IsCompleted)
            {  
            }

            // When the operation finishes, the value will be available.
            async.WaitForCompletion();
            string textValue = (string) text;
            Assert.Equals("5", textValue);
        }

        [TestMethod]
        public void CopyTest()
        {
            Dictionary<int, string> dic = new Dictionary<int, string>();
            dic.Add(1, "1");

            // Object is serialized and a copy will be sent to the target process 
            dynamic dicInTarget = _app.Copy(dic);

            // Null is useful for out arguments
            dynamic value = _app.Null();
            dicInTarget.TryGetValue(1, value);
            Assert.AreEqual("1", (string)value);
        }

        [TestMethod]
        public void CastBehaviorTest()
        {
            dynamic window = _app.Type<Application>().Current.MainWindow;

            // referenced object exists in target process' memory.
            dynamic reference = window._textBox.Text;

            // when you perform a cast, it will marshaled from the target process.
            string text = reference;

            // ok
            string cast = (string)reference;

            // No good. Result is false
            bool isString = reference is string;

            // No good. Result is null
            string textAs = reference as string;

            // ok
            string.IsNullOrEmpty((string)reference);

            // No good. Throws an exception
            string.IsNullOrEmpty(reference);
        }

        [TestMethod]
        public void DllInjectionTest()
        {
            dynamic window = _app.Type<Application>().Current.MainWindow;
            dynamic textBox = window._textBox;

            //The code let target process load current assembly.
            WindowsAppExpander.LoadAssembly(_app, GetType().Assembly);

            //You can use class defined in current assembly
            dynamic observer = _app.Type<Observer>()(textBox);

            //Check change text
            textBox.Text = "abc";
            Assert.IsTrue((bool)observer.TextChanged);
        }
    }

    class Observer
    {
        internal bool TextChanged { get; set; }
        internal Observer(TextBox textBox)
        {
            textBox.TextChanged += delegate { TextChanged = true; };
        }
    }
}
