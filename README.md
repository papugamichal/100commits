[![.NET](https://github.com/papugamichal/100commits/actions/workflows/dotnet.yml/badge.svg)](https://github.com/papugamichal/100commits/actions/workflows/dotnet.yml)

# 100commits
See more info here: https://100commitow.pl/

### Ideas to implement and test:
1. Value Objets:
   - [ ] As a base class
   - [ ] As a record
   - [ ] Storing in database (setup simple schema)
    
2. [x] Testing with `TestContainers` (https://dotnet.testcontainers.org/):
   - [x] Read documentation
   - [x] Setup first test (ContainerBuilder)
   - [x] Setup test with Postgresql container
3. Entity Framework:
   - [ ] Tbd
   - [ ] Dapper
4. Docker:
   - [ ] Setup startup project in docker container 
   - [ ] Setup database container for the project
5. GitHub Actions:
   - [ ] Setup action to automatically make commit into Readme file with summary of all commits made that day  
   - [x] Setup action to build & run tests on push
6. gRPC - AirQuality Demo Applications
   - [x] Setup projects
   - [x] Air station streaming to the server (client streaming)
   - [x] Server streaming to user GUI app (server streaming)
   - [x] Client select station to stream data (client streaming - server streaming)
   - [x] Server data persistance (in-memory)
   - [ ] Server data persistance (entity framework)
7. UI localization with IStringLocalizer:
   - [ ] AirQ.Client UI (simple graph visualization + updates)
8. Idea: gRPC test integration test fixture for client/server
   - [ ] Test fixture
9. Architecture:
   -[x] ArchUnitNET
10. Event bus to synchronize Blazor components (Blazor WebServer)
    - [x] Component can subscribe for <TEvent> (with sync delegate)
    - [x] Component can subscribe for <TEvent> (with async delegate)
    - [x] Subscription is unregistered when token is disposed
    - [x] Adapter: In-Memory (subscription as a token)
    - [ ] Adapter: In-Memory (subscription as a stream) [NEED REFINEMENT]
    - [ ] Adapter: RabbitMq
11. TBD
    - [ ]
    - [ ]
    - [ ]
    - [ ]
    - [ ]
    - [ ]