using Parcel.NET.Abstractions.Models;

namespace Parcel.NET.Docs.Playground.Models;

public class PackageFormModel
{
    public double Weight { get; set; } = 2.5;
    public double? Length { get; set; }
    public double? Width { get; set; }
    public double? Height { get; set; }

    public Package ToPackage() => new()
    {
        Weight = Weight,
        Dimensions = Length.HasValue && Width.HasValue && Height.HasValue
            ? new Dimensions { Length = Length.Value, Width = Width.Value, Height = Height.Value }
            : null
    };
}
