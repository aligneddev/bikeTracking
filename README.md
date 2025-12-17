# Bike Tracking Application

Created with SpecKit and Specification Driven Development (SDD) principles to demonstrate the using SpecKit. I will start with a PowerPoint presentation. TODO: add link to presentation.

https://github.com/github/spec-kit/blob/main/spec-driven.md#core-principles


## Starting Point

I created the `startingPoint-preConstitution` branch with `aspire new` chosing the Blazor and Minimal API for Aspire 13.0.2 on 12/11/2025.

I then setup SpecKit toolkit with `uv tool install specify-cli --from git+https://github.com/github/spec-kit.git`
Then used `specify init .` selecting Copilot and Powershell, to add the SpecKit files to the project.

You can see the instructions in the repo here: https://github.com/github/spec-kit

`speckit check` does not list Visual Studio, so I'll work in VS Code.

You'll need Docker or Podman to run the containers that Aspire creates.

## 📋 Project Governance

**All development is governed by the [Bike Tracking Application Constitution](.specify/memory/constitution.md).**

### SpecKit

Use SpecKit to create specifications and run the development. It is essential to keep the specifications up to date as the source of truth for the project so we can follow the SDD principles.

#### Constitution

This is the constitution prompt I used:

```markdown
Create with principles focused on code quality, testing standards, user experience consistency, and performance requirements following Clean Architecture, Functional Programming (pure and impure function sandwich), Event Sourcing and Domain Driven Development ideas to create high scalable, quality and usable web application and https API and an SQL Database. 
Suggest tests, but ask for my input before creating tests.

Focus on creating a working vertical slice of functionality for each specification. We value working software after running /speckit.implementation.

Use the MCP tools for MS Learn (for information), GitHub (source control and actions), Azure MCP for gathering information. Suggest other  MCP tools to use and record those in this constitution. Prompt me for permission to use these. If you are unsure, use a web search and MS Learn to make sure your information is up to date.

We will use the latest Aspire orchestration, latest C# features, C# with .Net 10 Minimal API for the API backend. Make sure we have the latest NuGet packages and ask to update when you see any out of date.

Microsoft Blazor .Net 10 for the front end (with responsive design and and simple UX. The user will login with an OAUTH identity (look up information in MS Learn https://learn.microsoft.com/en-us/aspnet/core/blazor/security/?view=aspnetcore-10). The user will have access to only their data and publicly available (use the latest Blazor FluentUI v4.13.2 https://www.fluentui-blazor.net/, componentize with "Design Tokens", create a DesignTheme using these colors for a palette "FFCDA4, FFB170, FF7400, D96200, A74C00" to follow our branding)).

The database will be an Azure SQL database. We will use the new SDK style database project to handle all database changes. Use the latest Entity Framework Core. Following Event Sourcing, create tables to store each event. Current projections will be created in a background Azure Function listening for Change Event Streaming Events (CES) and stored in a different read only table.

The application will be hosted in Azure.  With Aspire, host the application in Azure Container Apps. Use Managed Identity. All secrets must be in the Azure Secret Manager.

DevOps: Pipelines will be for GitHub Actions using the Aspire and `azd` tooling to deploy. Create templates actions for easier reuse.
```

### Business Rules/Features

We are creating a new product called Biker Commuter. We want to enable users to quickly and easily track their bike rides. 
They will use this data to track the savings instead of driving, see the historical weather and use this data for deciding on what to wear on a ride, give motivation to ride more and possible share with others.  

It will track the distance, time, current weather, expenses, gas prices, gallons of gas saved, and Co2 saved. 
It will also give an estimate of the total savings based on the mileage rate and a different savings based on average gas prices and vehicle miles per gallon. 
The product will also have a feature to track the number of rides and the average distance per ride. 
We will show some charts and graphs to visualize the miles and savings. 
The user will be able to see these for the current year, the total for all the years and be able to drill into each month or day.

## Links To Share

- https://den.dev/blog/github-spec-kit/
- Read the philosphy at https://github.com/github/spec-kit/blob/main/spec-driven.md#streamlining-sdd-with-commands
- Playlist of training videos - https://www.youtube.com/watch?v=pijfhJ725hY&list=PL4cUxeGkcC9h9RbDpG8ZModUzwy45tLjb
- https://www.youtube.com/@DenDev is a maintainer and has many videos on SpecKit
- https://developer.microsoft.com/blog/spec-driven-development-spec-kit