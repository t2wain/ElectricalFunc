using GraphLib;
using NecFillLib;

namespace RouteLib
{

    public delegate bool RWFilter(Raceway r);

    public enum RWTypeEnum
    {
        TRAY,
        DROP,
        OTHER,
    }

    public record Node(string ID) : IVertex;

    public record Raceway : IEdge
    {
        public string ID { get; init; }
        public Node FromNode { get; init; }
        public Node ToNode { get; init; }
        public EdgeWeight Weight { get; init; } = new(1.0);
        public RWTypeEnum Type { get; init; } = RWTypeEnum.TRAY;
        public IVertex FromVertex => FromNode;
        public IVertex ToVertex => ToNode;
    }

    /// <summary>
    /// Segregation systems that can be handled in a raceway
    /// </summary>
    public class TraySegSystem : HashSet<(string RacewayID, string SystemID)> { }

    /// <summary>
    /// Raceway in the network that have been blocked to all routings
    /// </summary>
    public class BlockRW : HashSet<string> { }

    /// <summary>
    /// Raceway with high preference for the current route
    /// </summary>
    public class PreferRW : HashSet<string> { }

    /// <summary>
    /// Raceway must be excluded for the current route
    /// </summary>
    public class ExcludeRW : HashSet<string> { }

    public class IncludeRW : List<string> { }

    public class NecTrayFill : Dictionary<string, TrayFill> { }

    public record Router()
    {
        public IGraph Network { get; init; }
        public TraySegSystem RWSystems { get; init; } = new();
        public BlockRW BlockedRWs { get; init; } = new();
        public NecTrayFill RWFills { get; init; } = new();
    };

    public record RouteSpec : ShortPathAlgo.ISPRouteSpec
    {
        public string CableID { get; init; }
        public Node FromNode { get; init; }
        public Node ToNode { get; init; }
        public string SegSystemID { get; init; }
        public PreferRW PreferRWs { get; init; } = new();
        public IncludeRW IncludeRWs { get; init; } = new();
        public ExcludeRW ExcludeRWs { get; init; } = new();
        public IEnumerable<CableFill> CableFills { get; init; } = new List<CableFill>();

        IVertex ShortPathAlgo.ISPRouteSpec.Source => FromNode;
        IVertex ShortPathAlgo.ISPRouteSpec.Target => ToNode;
        double ShortPathAlgo.ISPRouteSpec.CutOffLength => 0.0;
    }
}