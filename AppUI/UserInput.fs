module UserInput
open System;
open System.Timers

type Message() =
        [<Literal>]
        let message = @"Informações antes de iniciar: Lembre-se de criar o projeto em seu console do google 
        e criar sua autenticação com Auth 2.0, baixar o arquivo json e nomealo como 'client_secret.json',
        e colocalo na sua pasta de downloads. Após isso basta digitar o nome do projeto na próxima instrução. 
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
        
    do
        Console.Clear()
        printf "(Obs: as senhas não são salvas no banco de dados, em caso de esquecer terá que refazer o proceso de login):"
        printfn $"Digite uma senha de segurança:{Environment.NewLine}"
        userPassword <- Console.ReadLine();
        Console.Clear()

    member this.UserProjetoName = userProjectName
    member this.UserPassword = userPassword  
    




