# Izenda Custom Bootstrapper

 :warning: **This project is designed for demonstration purposes, please ensure that security and customization meet the standards of your company.**
 
 
## Overview
A custom bootstrapper allows you to modify requests and responses to the Izenda API. 

## A Few Uses cases
1. Filtering down a large list of tenants based on some custom criteria.
2. Removing items from filter field data.


## Required references:

1. Izenda.BI.API.dll  
2. Izenda.BI.Framework.dll 
3. Nancy.dll

## Installation

1. Build the project and copy the dll to the bin folder of your Izenda API. 
  
   
2. Update the Web.config (API) file to use the custom bootstrapper.
```
  <!--Izenda-->
  <nancyFx>
    <bootstrapper assembly="IzendaCustomBootstrapper" type="IzendaCustomBootstrapper.CustomBootstrapper" />
  </nancyFx>
  
```
4. Restart the API instance
