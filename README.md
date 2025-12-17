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

### Business Rules/Features

We are creating a new product called Biker Commuter. We want to enable users to quickly and easily track their bike rides. 
They will use this data to track the savings instead of driving, see the historical weather and use this data for deciding on what to wear on a ride, give motivation to ride more and possible share with others.  

It will track the distance, time, current weather, expenses, gas prices, gallons of gas saved, and Co2 saved. 
It will also give an estimate of the total savings based on the mileage rate and a different savings based on average gas prices and vehicle miles per gallon. 
The product will also have a feature to track the number of rides and the average distance per ride. 
We will show some charts and graphs to visualize the miles and savings. 
The user will be able to see these for the current year, the total for all the years and be able to drill into each month or day.
