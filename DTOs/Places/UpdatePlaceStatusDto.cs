using System.ComponentModel.DataAnnotations;
using soromaps_api.Models.Enums;

namespace soromaps_api.DTOs.Places;

public sealed record UpdatePlaceStatusDto(
    [Required(ErrorMessage = "place.status_required")]
    PlaceStatus Status
);
