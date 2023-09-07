using GraphLib;
using NecFillLib;

namespace RouteLib
{
    /// <summary>
    /// Function signature for many of the raceway
    /// filtering functions used in the short path
    /// calculation.
    /// </summary>
    public delegate bool RWFilter(Raceway r);

    public enum RWTypeEnum
    {
        TRAY,
        DROP,
        OTHER,
    }

    /// <summary>
    /// Node is a concept in the raceway network,
    /// while vertex is a concept in the graph
    /// data structure.
    /// </summary>
    public record Node(string ID) : IVertex;

    /// <summary>
    /// Raceway is a concept in the cable routing,
    /// while edge is a concept in the graph
    /// data structure.
    /// </summary>
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
    /// Segregation systems that can be handled in a raceway.
    /// You can exclude certain raceway from this list if segregation
    /// system validation are not needed such as for DROP and JUMP.
    /// </summary>
    public class TraySegSystem : HashSet<(string RacewayID, string SystemID)> { }

    /// <summary>
    /// Raceway in the network that have been blocked to all routings
    /// </summary>
    public class BlockRW : HashSet<string> { }

    /// <summary>
    /// Raceway with high preference for the current route.
    /// The weight of these raceway will be temporary scaled
    /// to a lower value during routing.
    /// </summary>
    public class PreferRW : HashSet<string> { }

    /// <summary>
    /// Raceway must be excluded for the current route
    /// </summary>
    public class ExcludeRW : HashSet<string> { }

    /// <summary>
    /// Raceway must be part of the route in
    /// the same order as specified. However, other
    /// raceway can appear in between 
    /// of these required raceways.
    /// </summary>
    public class IncludeRW : List<string> { }

    /// <summary>
    /// These are cable trays and their current cable fill. 
    /// The cable fill of these trays will be checked to avoid
    /// over fill. You can exclude certain raceway from this list 
    /// if tray fill validation are not needed such as for
    /// DROP, JUMP, and trenches since these are not cable tray.
    /// </summary>
    public class NecTrayFill : Dictionary<string, TrayFill> { }

    /// <summary>
    /// Maintain the data about the raceway network
    /// needed for cable routing. This define a common
    /// requirement for all cables.
    /// </summary>
    public record Router()
    {
        /// <summary>
        /// Graph data structure of the raceway network.
        /// </summary>
        public IGraph Network { get; init; }
        public TraySegSystem RWSystems { get; init; } = new();
        public BlockRW BlockedRWs { get; init; } = new();
        public NecTrayFill RWFills { get; init; } = new();
    };

    /// <summary>
    /// Maintain the data about the routing requirement
    /// for the cables. This define a specific requirement
    /// for each cable.
    /// </summary>
    public record RouteSpec : ShortPathAlgo.ISPRouteSpec
    {
        public string CableID { get; init; }
        public Node FromNode { get; init; }
        public Node ToNode { get; init; }
        public string SegSystemID { get; init; }
        public PreferRW PreferRWs { get; init; } = new();
        public IncludeRW IncludeRWs { get; init; } = new();
        public ExcludeRW ExcludeRWs { get; init; } = new();
        /// <summary>
        /// If multiple cables must have the same route, such
        /// as single condutor cable or parallel cables, you can
        /// include the cable fills for all of those cables.
        /// </summary>
        public IEnumerable<CableFill> CableFills { get; init; } = new List<CableFill>();

        // parameters used by the short path algorithm
        IVertex ShortPathAlgo.ISPRouteSpec.Source => FromNode;
        IVertex ShortPathAlgo.ISPRouteSpec.Target => ToNode;
        double ShortPathAlgo.ISPRouteSpec.CutOffLength => 0.0;
    }
}