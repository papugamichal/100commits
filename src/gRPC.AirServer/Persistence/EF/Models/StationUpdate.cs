using System.ComponentModel.DataAnnotations.Schema;

namespace gRPC.Server.Persistence.EF.Models;

public class StationUpdate
{
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public Guid Id { get; set; }
    public string StationName { get; set; }
    public DateTimeOffset Created { get; set; }
    public string Data { get; set; }
}