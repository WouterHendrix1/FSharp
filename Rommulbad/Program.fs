open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.DependencyInjection
open Giraffe
open Application
open Services
open Thoth.Json.Giraffe
open Thoth.Json.Net
open DatabaseInterface

let configureApp (app: IApplicationBuilder) =
    // Add Giraffe to the ASP.NET Core pipeline
    app.UseGiraffe Routes.routes

let configureServices (services: IServiceCollection) =
    // Add Giraffe dependencies
    services
        .AddGiraffe()
        .AddSingleton<IDatabase>(MemoryDatabase())        
        .AddSingleton<SessionService>(fun serviceProvider -> 
            let store = serviceProvider.GetService<IDatabase>()
            SessionService(store)
        )
        .AddSingleton<CandidateService>(fun serviceProvider -> 
            let store = serviceProvider.GetService<IDatabase>()
            CandidateService(store)
        )
        .AddSingleton<GuardianService>(fun serviceProvider -> 
            let store = serviceProvider.GetService<IDatabase>()
            GuardianService(store)
        )
        .AddSingleton<Json.ISerializer>(ThothSerializer(skipNullField = false, caseStrategy = CaseStrategy.CamelCase))
    |> ignore

[<EntryPoint>]
let main _ =
    Host
        .CreateDefaultBuilder()
        .ConfigureWebHostDefaults(fun webHostBuilder ->
            webHostBuilder.Configure(configureApp).ConfigureServices(configureServices)
            |> ignore)
        .Build()
        .Run()

    0
