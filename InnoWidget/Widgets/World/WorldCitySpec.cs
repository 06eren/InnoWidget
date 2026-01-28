using System;

namespace InnoWidget.Widgets.World;

public sealed record WorldCitySpec(string City, TimeZoneInfo TimeZone, double Latitude, double Longitude);
