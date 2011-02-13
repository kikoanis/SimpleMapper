using Microsoft.VisualStudio.TestTools.UnitTesting;
using SimpleMapper;

namespace SimpleMapperTest
{
	[TestClass]
	public class SimpleMapperTest
	{
		[TestMethod]
		public void MappingFromaTobShouldBeCorrect()
		{
			var a = new A
			        	{
			        		Prep1 = "AnyString", 
			        		Prep2 = 5, 
			        		Prep3 = true, 
			        		Prep4 = 3.5, 
			        		Prep5 = 20.0m
			        	};
			var b = new SimpleMapper<A, B>().Exclude(s => s.Prep2).ForProperty(d => d.Prep55).AssignProperty(s => s.Prep5).Map(a);

			Assert.AreEqual(a.Prep1, b.Prep1);
			Assert.AreEqual(a.Prep3, b.Prep3);
			Assert.AreEqual(a.Prep4, b.Prep4);
			Assert.AreEqual(0, b.Prep2);  // excluded
			Assert.AreEqual(a.Prep5, b.Prep55); // exclusive mapping
			Assert.AreEqual(0.0m, b.Prep51);    // not mapped to any property

		}

		[TestMethod]
		public void MappingFromaTobUsingExistingObjectShouldBeCorrect()
		{
			var a = new A
			        	{
			        		Prep1 = "AnyString",
			        		Prep2 = 5,
			        		Prep3 = true,
			        		Prep4 = 3.5,
			        		Prep5 = 20.0m
			        	};

			var b = new B
			        	{
			        		Prep1 = "OtherString",
			        		Prep51 = 12.0m
			        	};
			new SimpleMapper<A, B>().UseExisting(b).ForProperty(d => d.Prep55).AssignProperty(s => s.Prep5).Map(a);

			Assert.AreEqual(a.Prep1, b.Prep1);
			Assert.AreNotEqual("OtherString", b.Prep1);
			Assert.AreEqual(a.Prep2, b.Prep2);
			Assert.AreEqual(a.Prep3, b.Prep3);
			Assert.AreEqual(a.Prep4, b.Prep4);
			Assert.AreEqual(a.Prep5, b.Prep55); // exclusive mapping
			Assert.AreEqual(12.0m, b.Prep51);    // original value

		}
	}
}