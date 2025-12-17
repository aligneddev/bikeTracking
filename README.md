# Bike Tracking Application

Created with SpecKit and Specification Driven Development (SDD) principles to demonstrate the using SpecKit. I will start with a PowerPoint presentation. TODO: add link to presentation.

https://github.com/github/spec-kit/blob/main/spec-driven.md#core-principles

## Starting Point

I created the `startingPoint-preConstitution` branch with `aspire new` chosing the Blazor and Minimal API for Aspire 13.0.2 on 12/11/2025.

I then setup SpecKit toolkit with `uv tool install specify-cli --from git+https://github.com/github/spec-kit.git`
Then used `specify init .` selecting Copilot and Powershell, to add the SpecKit files to the project.

You can see the instructions in the repo here: https://github.com/github/spec-kit

You'll need Docker or Podman to run the containers that Aspire creates.


## SpecKit


### Constitution


### Business Rules/Features

We are creating a new product called Biker Commuter. We want to enable users to quickly and easily track their bike rides. 
They will use this data to track the savings instead of driving, see the historical weather and use this data for deciding on what to wear on a ride, give motivation to ride more and possible share with others.  

It will track the distance, time, current weather, expenses, gas prices, gallons of gas saved, and Co2 saved. 
It will also give an estimate of the total savings based on the mileage rate and a different savings based on average gas prices and vehicle miles per gallon. 
The product will also have a feature to track the number of rides and the average distance per ride. 
We will show some charts and graphs to visualize the miles and savings. 
The user will be able to see these for the current year, the total for all the years and be able to drill into each month or day.

- The user will be able to add a ride with a date, time, distance, and notes. The user will be able to edit the ride at any time.
  - The user will be able to see the current weather for the ride date and time. This will be stored with the ride.
  - This is the main feature of the application.
- The user will be able to import rides from a CSV file. The user will be able to export rides to a CSV file.
- The user will only be able to see the total distance, time, current weather, expenses, gas prices, gallons of gas saved, Co2 saved, and the number of rides for the current year and all years for themself.
- The user will be able to see the average distance per ride for the current year and all years for themself.
- The user will be able to see the total savings based on the mileage rate and a different savings based on average gas prices and vehicle miles per gallon for the current year and all years for themself.
- The user will be able to edit the mileage past rides, but only back 3 months. The user will be able to delete rides, but only back 3 months.
- The user will be able to add expenses with a date and dollar amount and a note. The user will be able to edit the expenses at any time.
- The user will be able to import expenses from a CSV file. The user will be able to export expenses to a CSV file.
- The user will be able to add gas prices with a date and dollar amount. The user will be able to edit the gas prices at any time.
- The user will be able to add the vehicle with miles per gallon with a date and vehicle name. The user will only be able to add a new vehicle.
  - This mpg will be used to calculate the gallons of gas saved and Co2 saved.
- The user will be able to add a mileage rate with a date and dollar amount. The user will only be able to add a new mileage rate.

