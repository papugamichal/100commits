# 100commits
See more info here: https://100commitow.pl/ 


### Ideas to implement and test:
1. Value Objets:
   - [ ] As a base class
   - [ ] As a record
   - [ ] Storing in database (setup simple schema)
    
2. [x] Testing with `TestContainers`:
   https://dotnet.testcontainers.org/ 
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
6. gRPC - AirQuality Demo Applications
   - [x] Setup projects
   - [x] Air station streaming to the server (client streaming)
   - [x] Server streaming to user GUI app (server streaming)
   - [x] Client select station to stream data (client streaming - server streaming)
   - [ ] Server data persistance (in-memory)
   - [ ] Server data persistance (entity framework)
7. UI localization with IStringLocalizer