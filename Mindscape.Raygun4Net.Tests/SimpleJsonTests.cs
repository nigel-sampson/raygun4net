﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mindscape.Raygun4Net.Messages;
using Mindscape.Raygun4Net.Tests.Model;
using NUnit.Framework;

namespace Mindscape.Raygun4Net.Tests
{
  [TestFixture]
  public class SimpleJsonTests
  {
    private CyclicObject _cyclicObject;
    private CyclicObject _cyclicArray;
    private CyclicObject _cyclicDictionary;
    private CyclicObject _cyclicGenericDictionary;
    private CyclicObject _deepCyclicObject;
    private CyclicObject _siblingObject;

    [SetUp]
    public void SetUp()
    {
      _cyclicObject = new CyclicObject();
      _cyclicObject.Child = _cyclicObject;

      _cyclicArray = new CyclicObject();
      _cyclicArray.Array[0] = _cyclicArray;

      _cyclicDictionary = new CyclicObject();
      _cyclicDictionary.Dictionary["Key"] = _cyclicDictionary;

      _cyclicGenericDictionary = new CyclicObject();
      _cyclicGenericDictionary.GenericDictionary["Key"] = _cyclicGenericDictionary;

      _deepCyclicObject = new CyclicObject();
      _deepCyclicObject.Child = new CyclicObject();
      _deepCyclicObject.Child.Array[0] = new CyclicObject();
      _deepCyclicObject.Child.Array[0].Dictionary["Key"] = _deepCyclicObject;

      _siblingObject = new CyclicObject();
      _siblingObject.Child = new CyclicObject();
      _siblingObject.Array[0] = _siblingObject.Child;
    }

    [Test]
    public void SerializeNonStringDictionary()
    {
      Dictionary<int, string> data = new Dictionary<int, string>();
      data[0] = "First!";

      RaygunMessage message = new RaygunMessage
      {
        OccurredOn = new DateTime(),
        Details = new RaygunMessageDetails
        {
          Error = new RaygunErrorMessage
          {
            Data = data
          }
        }
      };

      string messageString = SimpleJson.SerializeObject(message);

      Assert.AreEqual("{\"OccurredOn\":\"0001-01-01T00:00:00Z\",\"Details\":{\"Error\":{\"Data\":{\"0\":\"First!\"}}}}", messageString);
    }

    [Test]
    public void HandleCircularObjectStructure()
    {
      string json = SimpleJson.SerializeObject(_cyclicObject);
      Assert.AreEqual("{\"Child\":\"" + SimpleJson.CYCLIC_MESSAGE + "\",\"Array\":[null],\"Dictionary\":{},\"GenericDictionary\":{}}", json);
    }

    [Test]
    public void HandleCircularObjectStructureWithinArray()
    {
      string json = SimpleJson.SerializeObject(_cyclicArray);
      Assert.AreEqual("{\"Array\":[\"" + SimpleJson.CYCLIC_MESSAGE + "\"],\"Dictionary\":{},\"GenericDictionary\":{}}", json);
    }

    [Test]
    public void HandleCircularObjectStructureWithinDictionary()
    {
      string json = SimpleJson.SerializeObject(_cyclicDictionary);
      Assert.AreEqual("{\"Array\":[null],\"Dictionary\":{\"Key\":\"" + SimpleJson.CYCLIC_MESSAGE + "\"},\"GenericDictionary\":{}}", json);
    }

    [Test]
    public void HandleCircularObjectStructureWithinGenericDictionary()
    {
      string json = SimpleJson.SerializeObject(_cyclicGenericDictionary);
      Assert.AreEqual("{\"Array\":[null],\"Dictionary\":{},\"GenericDictionary\":{\"Key\":\"" + SimpleJson.CYCLIC_MESSAGE + "\"}}", json);
    }

    [Test]
    public void HandleDeepCircularObjectStructure()
    {
      string json = SimpleJson.SerializeObject(_deepCyclicObject);
      Assert.AreEqual("{\"Child\":{\"Array\":[{\"Array\":[null],\"Dictionary\":{\"Key\":\"" + SimpleJson.CYCLIC_MESSAGE + "\"},\"GenericDictionary\":{}}],\"Dictionary\":{},\"GenericDictionary\":{}},\"Array\":[null],\"Dictionary\":{},\"GenericDictionary\":{}}", json);
    }

    [Test]
    public void SerizlizeTheSameObjectWhenNotInACircularObjectStructure()
    {
      string json = SimpleJson.SerializeObject(_siblingObject);
      // The same object is repeated twice within the same parent, but there are siblings and don't form a cycle, so the json does not mention the cyclic message.
      // This is to test that objects are removed from the visited stack as the serializer traverses back through the object structure.
      Assert.AreEqual("{\"Child\":{\"Array\":[null],\"Dictionary\":{},\"GenericDictionary\":{}},\"Array\":[{\"Array\":[null],\"Dictionary\":{},\"GenericDictionary\":{}}],\"Dictionary\":{},\"GenericDictionary\":{}}", json);
    }
  }
}
