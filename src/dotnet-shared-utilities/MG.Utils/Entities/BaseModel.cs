using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MG.Utils.Interfaces;

namespace MG.Utils.Entities
{
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
}