module UserInput
open System;
open System.Timers

type Message() =
        [<Literal>]
        let message = @"Informações antes de iniciar: Lembre-se de criar o projeto em seu console do google 
        e criar sua autenticação com Auth 2.0, baixar o arquivo json e nomealo como 'client_secret.json',
        e colocalo na sua pasta de documentos. Após isso basta digitar o nome do projeto na próxima instrução. 
        Para confirmar que entendeu digite 'S'"
        do printfn message
type UserInput() =
    let mutable userProjectName = ""
    let mutable userPassword = ""
    let timer = new Timer(60000.0);
    let mutable timeout = false;
    do
        timer.Elapsed.Add(fun _ ->
              timeout <- true
              )
        timer.Start()

    let mutable confirmation = ""
    do 
        while  confirmation.ToUpper() <> "S" && not timeout do
            do 
                Console.Clear()
                Message() |> ignore
                confirmation <- Console.ReadLine();

    do
       if String.IsNullOrWhiteSpace(confirmation) && timeout then 
                    Console.Clear()
                    printf "Tempo esgotado, fechando aplicação..." 
                    Environment.Exit(0)
    do
        Console.Clear()
        printfn "Digite o nome do projeto:"
        userProjectName <- Console.ReadLine();
        Console.Clear()
        
    member this.UserProjetoName = userProjectName
    member this.UserPassword = userPassword  
    




