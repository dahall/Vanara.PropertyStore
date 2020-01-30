using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using Vanara;

namespace UnitTests
{
	public class Tests
	{
		[Test]
		public void Direct2Test()
		{
			var r = new RealSysSvc();
			var s = new MockSysSvc(r, true);

			Assert.That(r.MinValue, Is.EqualTo(s.PropertyStore["MinValue"]));

			const int newVal = 255;
			Assert.That(() => s.PropertyStore["MinValue"] = newVal, Throws.Nothing);
			Assert.That(s.PropertyStore["MinValue"], Is.EqualTo(newVal));
			Assert.That(r.MinValue, Is.EqualTo(newVal));
		}

		[Test]
		public void DirectTest()
		{
			var r = new RealSysSvc();
			var s = new MockSysSvc(r, true);

			Assert.That(r.CurrentDirectory, Is.EqualTo(s.CurrentDirectory));

			const string newDir = "C:\\";
			Assert.That(() => s.CurrentDirectory = newDir, Throws.Nothing);
			Assert.That(s.CurrentDirectory, Is.EqualTo(newDir));
			Assert.That(r.CurrentDirectory, Is.EqualTo(newDir));
		}

		[Test]
		public void ReadOnlyTest()
		{
			var r = new RealSysSvc();
			var s = new MockSysSvc(r, false);

			Assert.That(r.Today, Is.EqualTo(s.PropertyStore["Today"]));

			Assert.That(() => s.PropertyStore["Today"] = DateTime.Now, Throws.InvalidOperationException);
		}

		[Test]
		public void Staged2Test()
		{
			var r = new RealSysSvc();
			var s = new MockSysSvc(r, false);

			Assert.That(r.MinValue, Is.EqualTo(s.PropertyStore["MinValue"]));

			const int newVal = 255;
			Assert.That(() => s.PropertyStore["MinValue"] = newVal, Throws.Nothing);
			Assert.That(s.PropertyStore["MinValue"], Is.EqualTo(newVal));
			Assert.That(r.MinValue, Is.Not.EqualTo(newVal));
			Assert.That(() => s.PropertyStore.Commit(), Throws.Nothing);
			Assert.That(r.MinValue, Is.EqualTo(newVal));
		}

		[Test]
		public void StagedTest()
		{
			var r = new RealSysSvc();
			var s = new MockSysSvc(r, false);

			Assert.That(r.CurrentDirectory, Is.EqualTo(s.CurrentDirectory));

			const string newDir = "C:\\";
			Assert.That(() => s.CurrentDirectory = newDir, Throws.Nothing);
			Assert.That(s.CurrentDirectory, Is.EqualTo(newDir));
			Assert.That(r.CurrentDirectory, Is.Not.EqualTo(newDir));
			Assert.That(() => s.PropertyStore.Commit(), Throws.Nothing);
			Assert.That(r.CurrentDirectory, Is.EqualTo(newDir));
		}

		[Test]
		public void UndefinedPropTest()
		{
			var r = new RealSysSvc();
			var s = new MockSysSvc(r, false);

			object o = null;
			Assert.That(() => o = s.PropertyStore["Oggi"], Throws.TypeOf<KeyNotFoundException>());

			if (s.PropertyStore is PropertyStore ps)
				ps.NoDescriptorValidation = true;
			Assert.That(() => o = s.PropertyStore["Oggi"], Throws.Nothing);
			Assert.That(o, Is.Null);
			Assert.That(() => s.PropertyStore["Oggi"] = DateTime.Today, Throws.Nothing);
			Assert.That(() => o = s.PropertyStore["Oggi"], Throws.Nothing);
			Assert.That(o, Is.EqualTo(DateTime.Today));
		}

		[Test]
		public void SetLoadTest()
		{
			using var str = new System.IO.MemoryStream(System.Text.Encoding.UTF8.GetBytes(jsonprops), false);
			var pset = new PropertyDescriptorSet();
			pset.LoadAsync(str).Wait();
			Assert.That(pset.Count, Is.EqualTo(4));
			Assert.That(pset.Contains("MaxValue"));
		}

		[Test]
		public void SetPersistTest()
		{
			var r = new RealSysSvc();
			var s = new MockSysSvc(r, false);

			var bytes = new byte[4096];
			using var mst = new MemoryStream(bytes, true);
			s.PropertyStore.PropertyDescriptors.PersistAsync(mst).Wait();
			mst.Flush();

			var str = Encoding.UTF8.GetString(bytes, 0, Array.IndexOf(Encoding.UTF8.GetChars(bytes), '\u0000') + 2);
			Assert.That(str, Contains.Substring("\"canWrite\": false"));
			Assert.That(str, Contains.Substring("System.DateTime"));
			TestContext.Write(str);
		}

		[Test]
		public void PersistTest()
		{
			var r = new RealSysSvc();
			var s = new MockSysSvc(r, false);

			s.CurrentDirectory = Environment.SystemDirectory;
			s.PropertyStore["MinValue"] = 1;
			s.PropertyStore["MaxValue"] = 10;
			s.PropertyStore["IntList"] = new List<int>() { 1, 2 };

			var bytes = new byte[4096];
			using (var mst = new MemoryStream(bytes, true))
			{
				s.PropertyStore.PersistAsync(mst).Wait();
				Assert.That(mst.Length, Is.GreaterThan(0));
				mst.Flush();
			}

			var s2 = new MockSysSvc(r, false);
			using (var mst = new MemoryStream(bytes, false))
				s2.PropertyStore.LoadAsync(mst).Wait();
			Assert.That(s2.CurrentDirectory, Is.EqualTo(s.CurrentDirectory));
			Assert.That(s2.PropertyStore["MinValue"], Is.EqualTo(s.PropertyStore["MinValue"]));
			Assert.That(s2.PropertyStore["MaxValue"], Is.EqualTo(s.PropertyStore["MaxValue"]));
			Assert.That(s2.PropertyStore["Ints"], Is.EquivalentTo((System.Collections.IEnumerable)s.PropertyStore["Ints"]));
			Assert.That(s2.PropertyStore["IntList"], Is.EquivalentTo((System.Collections.IEnumerable)s.PropertyStore["IntList"]));
		}

		private class MockSysSvc : IPropertyProvider
		{
			private PropertyStore props;
			private RealSysSvc real;

			public MockSysSvc(RealSysSvc svc, bool passthrough)
			{
				real = svc;
				props = new PropertyStore(PropGet, PropSet, PropReset, passthrough);

				//props.CollectionChanged += OnPropertyCollectionChanged;
				//props.PropertyChanged += OnPropertyChanged;
				//props.PropertyChanging += OnPropertyChanging;

				// Load propertyset entries (manual, but could load from json)
				(props.PropertyDescriptors as PropertyDescriptorSet)?.LoadFromType(typeof(RealSysSvc));
			}

			public string CurrentDirectory
			{
				get => props.GetPropertyValue<string>();
				set => props.SetPropertyValue(value);
			}

			public IPropertyStore PropertyStore => props;

			//private void OnPropertyChanging(object sender, System.ComponentModel.PropertyChangingEventArgs e) { }
			//private void OnPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) { }
			//private void OnPropertyCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) { }
			//public override bool TryGetMember(GetMemberBinder binder, out object result) => props.TryGetValue(binder.Name, out result);
			//public override bool TrySetMember(SetMemberBinder binder, object value) { props[binder.Name] = value; return true; }

			public void MyCommand(string value) { }

			private bool PropGet(string propName, out object value)
			{
				value = null;
				var pi = real.GetType().GetProperty(propName, BindingFlags.Instance | BindingFlags.GetProperty | BindingFlags.Public);
				if (pi is null)
					return false;
				value = pi.GetValue(real);
				return true;
			}

			private void PropReset(string obj)
			{
			}

			private void PropSet(string propName, object value)
			{
				var pi = real.GetType().GetProperty(propName, BindingFlags.Instance | BindingFlags.SetProperty | BindingFlags.Public);
				if (pi is null)
					throw new KeyNotFoundException();
				pi.SetValue(real, value);
			}
		}

		private class RealSysSvc
		{
			public string CurrentDirectory { get; set; } = Environment.CurrentDirectory;
			public int MaxValue { get; set; } = int.MaxValue;
			public int MinValue { get; set; } = 0;
			public DateTime Today => DateTime.Today;
			public int[] Ints { get; set; } = new[] { 3, 2, 1 };
			public List<int> IntList { get; set; } = new List<int>(new[] { 4, 2, 1 });
		}

		const string jsonprops = @"
{
  ""propertyDescriptors"": [
	{
	  ""name"": ""CurrentDirectory"",
	  ""key"": ""5Dd5318e-e3BD-AAa2-E18D-ED04c9cBbFDE"",
	  ""type"": ""System.String"",
	  ""typeInfo"": {
		""canRead"": true,
		""canWrite"": true,
		""groupingRange"": ""discrete"",
		""aggregationType"": ""default""
	  },
	  ""displayInfo"": {
		""displayFormatString"": """",
		""viewFlags"": ""centerAlign"",
		""drawControl"": ""staticText"",
		""editControl"": ""text"",
		""filterControl"": ""default""
	  },
	  ""labelInfo"": {
		""label"": ""Project Name"",
		""invitationText"": ""Project name:"",
		""hideLabel"": false
	  }
	},
	{
	  ""name"": ""MaxValue"",
	  ""key"": ""aD90231B-aC7E-4344-dFeA-af26DB95Cb8A"",
	  ""type"": ""System.Int32"",
	  ""typeInfo"": {
		""canRead"": true,
		""canWrite"": true,
		""groupingRange"": ""dynamic"",
		""aggregationType"": ""max""
	  },
	  ""displayInfo"": {
		""displayFormatString"": """",
		""viewFlags"": ""default"",
		""drawControl"": ""default"",
	  },
	  ""labelInfo"": {
		""label"": ""Maximum"",
		""invitationText"": ""Max:"",
		""hideLabel"": false
	  },
	  ""relatedPropertyNames"": [
		""MinValue""
	  ]
	},
	{
	  ""name"": ""MinValue"",
	  ""key"": ""0716BaE1-D3EA-89E1-1CA2-8d2EbaCFc672"",
	  ""type"": ""System.Int32"",
	  ""typeInfo"": {
		""canRead"": true,
		""canWrite"": true,
		""groupingRange"": ""dynamic"",
		""aggregationType"": ""max""
	  },
	  ""labelInfo"": {
		""label"": ""Minimum"",
		""invitationText"": ""Min:"",
		""hideLabel"": false
	  },
	  ""relatedPropertyNames"": [
		""MaxValue""
	  ]
	},
	{
	  ""name"": ""Today"",
	  ""key"": ""4056ee8a-143c-7CF5-FC30-9EFc6485daFb"",
	  ""type"": ""System.DateTime"",
	  ""typeInfo"": {
		""canRead"": true,
		""canWrite"": false,
		""groupingRange"": ""date"",
		""aggregationType"": ""dateRange""
	  },
	  ""displayInfo"": {
		""displayFormatString"": ""U"",
		""viewFlags"": ""default"",
		""drawControl"": ""staticText"",
		""filterControl"": ""calendar""
	  },
	  ""labelInfo"": {
		""label"": ""Today""
	  }
	}
  ]
}
";
	}
}