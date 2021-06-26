# SoftwareEngineering

Serialisation of Spatial Pooler

This repository is the open source implementation of the Hierarchical Temporal Memory in C#/.NET Core. This repository contains set of libraries around **NeoCortext** API .NET Core library. **NeoCortex** API focuses implementation of _Hierarchical Temporal Memory Cortical Learning Algorithm_. Current version is first implementation of this algorithm on .NET platform. It includes the **Spatial Pooler**, **Temporal Pooler**, various encoders and **CorticalNetwork**  algorithms.

**Spatial Pooler** does not support serialisation.

Serialisation and Deserialisation of Spatial Pooler is implemented using JSON serialisation technique in .NET environment. Storing all the properties of Spatial Pooler class into a .json file. It is done by integrating Serialisation and Deserialisation method in Spatial Pooler Class, then identifying complex class properties and integrating Member Serialisation attribute for relevant classes and performing UnitTest to validate Serialisation functionality.
