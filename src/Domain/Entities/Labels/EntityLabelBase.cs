using System.ComponentModel.DataAnnotations;
using Domain.Entities.Users;
using Domain.Enums;
using Domain.Validation.Exceptions;
using Domain.ValueObjects;

namespace Domain.Entities.Labels;

public abstract class EntityLabelBase : IHasId
{
    protected EntityLabelBase()
    {
    }

    protected EntityLabelBase(
        string title,
        HexColor hexcolor = null,
        User createdBy = null)
    {
        Title = title;
        HexColor = hexcolor ?? HexColor.Random();
        CreatedById = createdBy?.Id;
    }

    public long Id { get; protected set; }

    [Required]
    [StringLength(50)]
    public string Title { get; protected set; }

    [Required]
    [StringLength(7)]
    public HexColor HexColor { get; protected set; }

    public long? CreatedById { get; protected set; }

    public virtual User CreatedBy { get; protected set; }

    public void Update(
        string title,
        HexColor hexcolor = null)
    {
        Title = title;
        HexColor = hexcolor ?? HexColor.Random();
    }

    public virtual void CouldBeUpdatedByOrFail(User user)
    {
        if (user.Id == CreatedById || user.Has(Role.Admin))
        {
            return;
        }

        throw new NoPermissionsException();
    }
}