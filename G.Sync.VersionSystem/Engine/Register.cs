using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace G.Sync.VersionSystem.Engine
{
    public class RegisterFile
    {
        private void AutoRegisterAll()
        {
            var ifc = typeof(IVersion);
            var assemblies = Assembly.GetAssembly(ifc);
            var types = 
                assemblies.GetTypes().Where(t => ifc.IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);

            var process = new Process();

            foreach (var type in types)
            {
                var instance = Activator.CreateInstance(type) as IVersion;

                process.AddVersion(instance, type.Name);
            }
        }
    }
}
