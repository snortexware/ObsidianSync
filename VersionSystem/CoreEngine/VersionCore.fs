namespace VersionCore.CoreEngine

open VersionSystem

module Registry =
    let private implementations = System.Collections.Generic.List<IVersionCore>()

    let register instance =
        implementations.Add(instance)

    let runAll () =
        for impl in implementations do
            impl.Run()