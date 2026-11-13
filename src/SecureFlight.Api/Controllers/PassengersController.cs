using Microsoft.AspNetCore.Mvc;
using SecureFlight.Api.Utils;
using SecureFlight.Core.DataTransferObjects;
using SecureFlight.Core.Entities;
using SecureFlight.Core.Interfaces;
using SecureFlight.Core.Mapping;

namespace SecureFlight.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class PassengersController(
    IService<Passenger> personService,
    IService<Flight> flightService,
    IService<PassengerFlight> passengerFlightService)
    : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<PassengerDataTransferObject>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Get()
    {
        var passengers = await personService.GetAllAsync();
        return passengers.Succeeded
            ? Ok(passengers.Result.Select(x => x.ToPassengerDto()))
            : Problem(passengers.ErrorResult.Message, statusCode: passengers.ErrorResult.Code.ToHttpStatusCode(), title: "Error Retrieving Passengers");
    }
    
    [HttpGet("/flights/{flightId:long}/passengers")]
    [ProducesResponseType(typeof(IEnumerable<PassengerDataTransferObject>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPassengersByFlight(long flightId)
    {
        var flight = await flightService.FindAsync(flightId);
        if (!flight.Succeeded)
        {
            return Problem(flight.ErrorResult.Message, statusCode: flight.ErrorResult.Code.ToHttpStatusCode(), title: "Error Retrieving Passengers for Flight");
        }
        
        var passengers = await personService.FilterAsync(p => p.Flights.Any(x => x.Id == flightId));
        return passengers.Succeeded
            ? Ok(passengers.Result.Select(x => x.ToPassengerDto()))
            : Problem(passengers.ErrorResult.Message, statusCode: passengers.ErrorResult.Code.ToHttpStatusCode(), title: "Error Retrieving Passengers for Flight");
    }
    
    [HttpPost("/passenger/{passengerId:long}/{flightId:long}")]
    [ProducesResponseType(typeof(IEnumerable<PassengerDataTransferObject>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddPassengerToFlight(long passengerId, long flightId)
    {
        try
        {
            if (string.IsNullOrEmpty(passengerId.ToString()))
            {
                return Problem(detail: "Passenger Id cannot be null.", 
                    statusCode: StatusCodes.Status400BadRequest, 
                    title: "Error.");
            }
            
            if (string.IsNullOrEmpty(flightId.ToString()))
            {
                return Problem(detail: "Flight Id cannot be null.", 
                    statusCode: StatusCodes.Status400BadRequest, 
                    title: "Error.");
            }
            
            var existingPassenger = await personService.FindAsync(passengerId);
            
            if(existingPassenger == null)
                return Problem(detail: "Passenger was not found.", 
                    statusCode: StatusCodes.Status404NotFound, 
                    title: "Error.");

            var existingFlight = await flightService.FindAsync(flightId);
            
            if(existingFlight == null)
                return Problem(detail: "Flight was not found.", 
                    statusCode: StatusCodes.Status404NotFound, 
                    title: "Error.");
            
            var passengerFlight = new PassengerFlight
            {
                FlightId = existingFlight.Result.Id,
                PassengerId = existingPassenger.Result.Id
            };

            // var result = passengerFlightService.UpdateAsync(passengerFlight);

        }
        catch (Exception e)
        {
            return Problem(detail: "There is an internal error.", 
                statusCode: StatusCodes.Status500InternalServerError, 
                title: "Error.");
        }
        var flight = await flightService.FindAsync(flightId);
        if (!flight.Succeeded)
        {
            return Problem(flight.ErrorResult.Message, statusCode: flight.ErrorResult.Code.ToHttpStatusCode(), title: "Error Retrieving Passengers for Flight");
        }
        
        var passengers = await personService.FilterAsync(p => p.Flights.Any(x => x.Id == flightId));
        return passengers.Succeeded
            ? Ok(passengers.Result.Select(x => x.ToPassengerDto()))
            : Problem(passengers.ErrorResult.Message, statusCode: passengers.ErrorResult.Code.ToHttpStatusCode(), title: "Error Retrieving Passengers for Flight");
    }
}