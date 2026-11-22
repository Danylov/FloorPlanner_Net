// Who creates SaveRoomRequest, and when:
//
// Our code never calls "new SaveRoomRequest(...)". ASP.NET Core creates it automatically
// (model binding and the JSON input formatter) for every POST and PUT request to RoomsController,
// after routing and before the controller action (Create or Update) is called.
//
// Steps:
// 1. Routing selects the action. The route values (planId, id) are taken from the URL.
// 2. [ApiController] treats the parameter of a complex type as the request body: the JSON sent by the client,
//    for example {"name":"Living","height":2.7,"corners":[{"x":0,"z":0}, ...]}, is converted
//    into a SaveRoomRequest object (with nested PointDto objects).
// 3. The validation attributes ([Required], [StringLength], [Range], [MinLength]) are checked.
//    If something is wrong, [ApiController] responds with 400 itself and the action is not called.
// 4. Otherwise the action is called with the ready object, and it only reads request.Name,
//    request.Height and request.Corners.
//
// GET and DELETE have no request body, so this type is not used for them.
using System.ComponentModel.DataAnnotations;

namespace FloorPlanner.Api.Dtos;

/// <summary>The request body for creating or updating a room.</summary>
public sealed record SaveRoomRequest(
    [Required, StringLength(100, MinimumLength = 1)] string Name,
    [Range(1.5, 10)] double Height,
    [Required, MinLength(3)] List<PointDto> Corners);
