namespace VersionCore.CoreEngine
open System
open System.Reflection
open VersionSystem
open VersionCore.CoreEngine.RegistryCore

module AutoRegister =
    let autoRegisterAll () =
        let iface = typeof<IVersionCore>
        let asm = Assembly.GetAssembly(typeof<IVersionCore>)

        asm.GetTypes()
        |> Array.filter (fun t -> iface.IsAssignableFrom(t) && not t.IsInterface && not t.IsAbstract)
        |> Array.iter (fun t ->
            match Activator.CreateInstance(t) with
            | :? IVersionCore as instance -> register instance
            | _ -> ()
        )