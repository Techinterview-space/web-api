using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

public class BaseModel : HasDatesBase, IBaseModel
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Key]
    public long Id { get; protected set; }

    public bool New()
    {
        return Id == default;
    }
}