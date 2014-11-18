AutofacBuilder
==============

An autofac quickstart component builder

Usage
=====

1. Add the project reference AutofacBuilder into your project
2. Add all your projects in the function AutofacBuilder.ContainerManager.BuildContainerInternal in the following format

   builder.RegisterFromAssembly("ApplicationBuilder.Logger", "ApplicationBuilder.Logger.GlobalLogger");
   
   Where "ApplicationBuilder.Logger" is the name of your assembly and "ApplicationBuilder.Logger.GlobalLogger" is the optional base class of all your components. If you specify a class name only components inheriting from this class will be added.
  
3. Once you have all your required assemblies included.
   Just call one of these three functions
   
   ContainerManager.BuilderContainer
   ContainerManager.BuilderContainerWithApi
   ContainerManager.BuilderContainerWithWeb
   
   based on what type of application you are running.
   
4. Enjoy