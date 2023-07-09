using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading;

//From here: https://stackoverflow.com/questions/19666511/how-to-create-a-serializationbinder-for-the-binary-formatter-that-handles-the-mo
public class DeserializationTypeRemapBinder : SerializationBinder
{
    private class AssemblyMapping
    {
        public string OldAssemblyName { get; set; }
        public string NewAssemblyName { get; set; }
    }

    List<AssemblyMapping> assemblyMappings;

    public DeserializationTypeRemapBinder()
    {
        assemblyMappings = new List<AssemblyMapping>();
    }

    public void AddAssemblyMapping(string oldAssemblyName, string newAssemblyName)
    {
        assemblyMappings.Add(new AssemblyMapping()
        {
            OldAssemblyName = oldAssemblyName,
            NewAssemblyName = newAssemblyName,
        });
    }

    public override Type BindToType(string assemblyName, string typeName)
    {
        //Need to handle the fact that assemblyName will come in with version while input type mapping may not
        //Need to handle the fact that generics come in as mscorlib assembly as opposed to the assembly where the type is defined.
        //Need to handle the fact that some types won't even be defined by mapping. In this case we should revert to normal Binding... how do you do that?

        //

        string newAssemblyName = assemblyName;
        string newTypeName = typeName;
        foreach (AssemblyMapping mapping in assemblyMappings)
        {
            if (assemblyName.Contains(mapping.OldAssemblyName))
            {
                newAssemblyName = mapping.NewAssemblyName;
                break;
            }
        }

        foreach (AssemblyMapping mapping in assemblyMappings)
        {
            //Handles Generic Types
            if (typeName.Contains(mapping.OldAssemblyName))
            {
                newTypeName = typeName.Replace(mapping.OldAssemblyName, mapping.NewAssemblyName);
            }
        }

        Type type = Type.GetType(String.Format("{0}, {1}", newTypeName, newAssemblyName));
        return type;
    }
}
